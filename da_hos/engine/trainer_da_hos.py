# Copyright (c) Facebook, Inc. and its affiliates. All Rights Reserved
import os
import time
import torch
from torch.cuda.amp import autocast

from da_hos.data.build import (
    build_detection_semisup_train_loader_two_crops,
    build_detection_test_loader,
)
from da_hos.data.dataset_mapper import DAHOSDatasetMapperTwoCropSeparate
from da_hos.engine.trainer import ATeacherTrainer
from da_hos.evaluation.epick_evaluation import EPICKEvaluator
import da_hos.modeling


class DAHOSTrainer(ATeacherTrainer):
    def __init__(self, cfg):
        ATeacherTrainer.__init__(self, cfg)
        self._amp_enabled = cfg.SOLVER.AMP.ENABLED

    @classmethod
    def build_train_loader(cls, cfg):
        mapper = DAHOSDatasetMapperTwoCropSeparate(cfg, True)
        return build_detection_semisup_train_loader_two_crops(cfg, mapper)

    @classmethod
    def build_evaluator(cls, cfg, dataset_name, output_folder=None, task="hand_obj"):
        if output_folder is None:
            output_folder = os.path.join(cfg.OUTPUT_DIR, "inference")
        return EPICKEvaluator(dataset_name, output_dir=output_folder, eval_task=task)

    @classmethod
    def build_test_loader(cls, cfg, dataset_name):
        mapper = DAHOSDatasetMapperTwoCropSeparate(cfg, False)
        return build_detection_test_loader(cfg, dataset_name, mapper=mapper)

    def run_step_full_semisup(self):
        if self._amp_enabled:
            self.amp_run_step_full_semisup()
            return

        self._trainer.iter = self.iter
        assert self.model.training, "Model was changed to eval mode!"

        start = time.perf_counter()

        data = next(self._trainer._data_loader_iter)
        # data_q and data_k from different augmentations (q:strong, k:weak)
        # label_strong, label_weak, unlabed_strong, unlabled_weak
        label_data_q, label_data_k, unlabel_data_q, unlabel_data_k = data
        data_time = time.perf_counter() - start

        # BURN-IN STAGE (SUPERVISED TRAINING WITH LABELED DATA)
        if self.iter < self.cfg.SEMISUPNET.BURN_UP_STEP:
            # input both strong and weak supervised data into model
            label_data_q.extend(label_data_k)

            record_dict, _, _, _ = self.model(label_data_q, branch="supervised")

            # weight losses
            loss_dict = {
                key: record_dict[key] * 1
                for key in record_dict.keys()
                if key[:4] == "loss"
            }
            losses = sum(loss_dict.values())

        else:
            if self.iter == self.cfg.SEMISUPNET.BURN_UP_STEP:
                # update copy the the whole model
                self._update_teacher_model(keep_rate=0.00)

            elif (
                self.iter - self.cfg.SEMISUPNET.BURN_UP_STEP
            ) % self.cfg.SEMISUPNET.TEACHER_UPDATE_ITER == 0:
                self._update_teacher_model(keep_rate=self.cfg.SEMISUPNET.EMA_KEEP_RATE)

            record_dict = {}

            #  0. REMOVE UNLABELED DATA LABELS
            unlabel_data_q = self.remove_label(unlabel_data_q)
            unlabel_data_k = self.remove_label(unlabel_data_k)

            #  1. GENERATE THE PSEUDO-LABEL USING TEACHER MODEL
            with torch.no_grad():
                _, proposals_rpn_unsup_k, proposals_roih_unsup_k, _ = (
                    self.model_teacher(unlabel_data_k, branch="unsup_data_weak")
                )

            #  2. PSEUDO-LABELING
            cur_threshold = self.cfg.SEMISUPNET.BBOX_THRESHOLD

            joint_proposal_dict = {}
            joint_proposal_dict["proposals_rpn"] = proposals_rpn_unsup_k
            # Process pseudo labels and thresholding
            pesudo_proposals_rpn_unsup_k, nun_pseudo_bbox_rpn = (
                self.process_pseudo_label(
                    proposals_rpn_unsup_k, cur_threshold, "rpn", "thresholding"
                )
            )

            joint_proposal_dict["proposals_pseudo_rpn"] = pesudo_proposals_rpn_unsup_k

            # Pseudo_labeling for ROI head (bbox location/objectness)
            pesudo_proposals_roih_unsup_k, _ = self.process_pseudo_label(
                proposals_roih_unsup_k, cur_threshold, "roih", "thresholding"
            )
            joint_proposal_dict["proposals_pseudo_roih"] = pesudo_proposals_roih_unsup_k

            # 3. ADD PSEUDO-LABEL TO UNLABELED DATA
            unlabel_data_q = self.add_label(
                unlabel_data_q, joint_proposal_dict["proposals_pseudo_roih"]
            )
            unlabel_data_k = self.add_label(
                unlabel_data_k, joint_proposal_dict["proposals_pseudo_roih"]
            )

            all_label_data = label_data_q + label_data_k
            all_unlabel_data = unlabel_data_q

            # 4. INPUT BOTH STRONGLY AND WEAKLY AUGMENTED LABELED DATA INTO STUDENT MODEL
            record_all_label_data, _, _, _ = self.model(
                all_label_data, branch="supervised"
            )
            record_dict.update(record_all_label_data)

            # 5. INPUT STRONGLY AUGMENTED UNLABELED DATA INTO MODEL
            record_all_unlabel_data, _, _, _ = self.model(
                all_unlabel_data, branch="supervised_target"
            )
            new_record_all_unlabel_data = {
                key + "_pseudo": record_all_unlabel_data[key]
                for key in record_all_unlabel_data.keys()
            }

            record_dict.update(new_record_all_unlabel_data)

            #### QUESTO PASSO NON MI CONVINCE DOVREBBERO ESSERE TUTTI E DUE STRONG

            # 6. INPUT WEAKLY LABELED DATA (SOURCE) AND WEAKLY UNLABELED DATA (TARGET) TO STUDENT MODEL
            for i_index in range(len(unlabel_data_k)):
                for k, v in unlabel_data_k[i_index].items():
                    label_data_k[i_index][k + "_unlabeled"] = v

            all_domain_data = label_data_k
            record_all_domain_data, _, _, _ = self.model(
                all_domain_data, branch="domain"
            )

            record_dict.update(record_all_domain_data)

            # weight losses
            loss_dict = {}
            for key in record_dict.keys():
                if key.startswith("loss"):
                    if key == "loss_rpn_loc_pseudo" or key == "loss_box_reg_pseudo":
                        # pseudo bbox regression <- 0
                        loss_dict[key] = record_dict[key] * 0
                    elif key[-6:] == "pseudo":  # unsupervised loss
                        loss_dict[key] = (
                            record_dict[key] * self.cfg.SEMISUPNET.UNSUP_LOSS_WEIGHT
                        )
                    elif key == "loss_D_img_s" or key == "loss_D_img_t":
                        # set weight for discriminator
                        loss_dict[key] = (
                            record_dict[key] * self.cfg.SEMISUPNET.DIS_LOSS_WEIGHT
                        )  # Need to modify defaults and yaml
                    else:  # supervised loss
                        loss_dict[key] = record_dict[key] * 1

            losses = sum(loss_dict.values())

        metrics_dict = record_dict
        metrics_dict["data_time"] = data_time
        self._write_metrics(metrics_dict)

        self.optimizer.zero_grad()
        losses.backward()

        self.optimizer.step()

    def amp_run_step_full_semisup(self):
        self._trainer.iter = self.iter
        assert self.model.training, "Model was changed to eval mode!"

        start = time.perf_counter()

        data = next(self._trainer._data_loader_iter)
        # data_q and data_k from different augmentations (q:strong, k:weak)
        # label_strong, label_weak, unlabed_strong, unlabled_weak
        label_data_q, label_data_k, unlabel_data_q, unlabel_data_k = data
        data_time = time.perf_counter() - start

        with autocast(dtype=self._trainer.precision):

            # BURN-IN STAGE (SUPERVISED TRAINING WITH LABELED DATA)
            if self.iter < self.cfg.SEMISUPNET.BURN_UP_STEP:
                # input both strong and weak supervised data into model
                label_data_q.extend(label_data_k)

                record_dict, _, _, _ = self.model(label_data_q, branch="supervised")

                # weight losses
                loss_dict = {
                    key: record_dict[key] * 1
                    for key in record_dict.keys()
                    if key[:4] == "loss"
                }
                losses = sum(loss_dict.values())

            else:
                if self.iter == self.cfg.SEMISUPNET.BURN_UP_STEP:
                    # update copy the the whole model
                    self._update_teacher_model(keep_rate=0.00)

                elif (
                    self.iter - self.cfg.SEMISUPNET.BURN_UP_STEP
                ) % self.cfg.SEMISUPNET.TEACHER_UPDATE_ITER == 0:
                    self._update_teacher_model(
                        keep_rate=self.cfg.SEMISUPNET.EMA_KEEP_RATE
                    )

                record_dict = {}

                #  0. REMOVE UNLABELED DATA LABELS
                unlabel_data_q = self.remove_label(unlabel_data_q)
                unlabel_data_k = self.remove_label(unlabel_data_k)

                #  1. GENERATE THE PSEUDO-LABEL USING TEACHER MODEL
                with torch.no_grad():
                    _, proposals_rpn_unsup_k, proposals_roih_unsup_k, _ = (
                        self.model_teacher(unlabel_data_k, branch="unsup_data_weak")
                    )

                #  2. PSEUDO-LABELING
                cur_threshold = self.cfg.SEMISUPNET.BBOX_THRESHOLD

                joint_proposal_dict = {}
                joint_proposal_dict["proposals_rpn"] = proposals_rpn_unsup_k
                # Process pseudo labels and thresholding
                pesudo_proposals_rpn_unsup_k, nun_pseudo_bbox_rpn = (
                    self.process_pseudo_label(
                        proposals_rpn_unsup_k, cur_threshold, "rpn", "thresholding"
                    )
                )

                joint_proposal_dict["proposals_pseudo_rpn"] = (
                    pesudo_proposals_rpn_unsup_k
                )

                # Pseudo_labeling for ROI head (bbox location/objectness)
                pesudo_proposals_roih_unsup_k, _ = self.process_pseudo_label(
                    proposals_roih_unsup_k, cur_threshold, "roih", "thresholding"
                )
                joint_proposal_dict["proposals_pseudo_roih"] = (
                    pesudo_proposals_roih_unsup_k
                )

                # 3. ADD PSEUDO-LABEL TO UNLABELED DATA
                unlabel_data_q = self.add_label(
                    unlabel_data_q, joint_proposal_dict["proposals_pseudo_roih"]
                )
                unlabel_data_k = self.add_label(
                    unlabel_data_k, joint_proposal_dict["proposals_pseudo_roih"]
                )

                all_label_data = label_data_q + label_data_k
                all_unlabel_data = unlabel_data_q

                # 4. INPUT BOTH STRONGLY AND WEAKLY AUGMENTED LABELED DATA INTO STUDENT MODEL
                record_all_label_data, _, _, _ = self.model(
                    all_label_data, branch="supervised"
                )
                record_dict.update(record_all_label_data)

                # 5. INPUT STRONGLY AUGMENTED UNLABELED DATA INTO MODEL
                record_all_unlabel_data, _, _, _ = self.model(
                    all_unlabel_data, branch="supervised_target"
                )
                new_record_all_unlabel_data = {
                    key + "_pseudo": record_all_unlabel_data[key]
                    for key in record_all_unlabel_data.keys()
                }

                record_dict.update(new_record_all_unlabel_data)

                #### QUESTO PASSO NON MI CONVINCE DOVREBBERO ESSERE TUTTI E DUE STRONG

                # 6. INPUT WEAKLY LABELED DATA (SOURCE) AND WEAKLY UNLABELED DATA (TARGET) TO STUDENT MODEL
                for i_index in range(len(unlabel_data_k)):
                    for k, v in unlabel_data_k[i_index].items():
                        label_data_k[i_index][k + "_unlabeled"] = v

                all_domain_data = label_data_k
                record_all_domain_data, _, _, _ = self.model(
                    all_domain_data, branch="domain"
                )

                record_dict.update(record_all_domain_data)

                # weight losses
                loss_dict = {}
                for key in record_dict.keys():
                    if key.startswith("loss"):
                        if key == "loss_rpn_loc_pseudo" or key == "loss_box_reg_pseudo":
                            # pseudo bbox regression <- 0
                            loss_dict[key] = record_dict[key] * 0
                        elif key[-6:] == "pseudo":  # unsupervised loss
                            loss_dict[key] = (
                                record_dict[key] * self.cfg.SEMISUPNET.UNSUP_LOSS_WEIGHT
                            )
                        elif key == "loss_D_img_s" or key == "loss_D_img_t":
                            # set weight for discriminator
                            loss_dict[key] = (
                                record_dict[key] * self.cfg.SEMISUPNET.DIS_LOSS_WEIGHT
                            )  # Need to modify defaults and yaml
                        else:  # supervised loss
                            loss_dict[key] = record_dict[key] * 1

                losses = sum(loss_dict.values())

        metrics_dict = record_dict
        metrics_dict["data_time"] = data_time
        self._write_metrics(metrics_dict)

        self.optimizer.zero_grad()
        losses.backward()

        self.optimizer.step()
