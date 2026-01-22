# Copyright (c) Facebook, Inc. and its affiliates. All Rights Reserved

import copy
import cv2
from detectron2.modeling.roi_heads.roi_heads import ROI_HEADS_REGISTRY
import numpy as np
import logging
import torch
import torch.nn as nn
from torch.nn import functional as F
from typing import Dict, Tuple, List, Optional
from collections import OrderedDict

from detectron2.modeling.meta_arch.build import META_ARCH_REGISTRY
from detectron2.modeling.meta_arch.rcnn import GeneralizedRCNN
from detectron2.config import configurable

from detectron2.modeling.proposal_generator import build_proposal_generator
from detectron2.modeling.backbone import build_backbone, Backbone
from detectron2.modeling.roi_heads import build_roi_heads
from detectron2.utils.events import get_event_storage
from detectron2.structures import ImageList, Boxes
from detectron2.data.detection_utils import convert_image_to_rgb
from da_hos.evaluation.hos_postprocessing import get_incontact_obj, get_center

from da_hos.modeling.meta_arch.extra_modules import FCDiscriminator_img, grad_reverse
from detectron2.layers.nms import batched_nms

from detectron2.structures.instances import Instances


def calculate_iou(box_a, box_b):
    # Calculate the coordinates of the intersection rectangle
    x_left = max(box_a[0], box_b[0])
    y_top = max(box_a[1], box_b[1])
    x_right = min(box_a[2], box_b[2])
    y_bottom = min(box_a[3], box_b[3])

    # If the intersection is valid (non-negative area)
    if x_right > x_left and y_bottom > y_top:
        intersection_area = (x_right - x_left) * (y_bottom - y_top)
        box_a_area = (box_a[2] - box_a[0]) * (box_a[3] - box_a[1])
        box_b_area = (box_b[2] - box_b[0]) * (box_b[3] - box_b[1])
        iou = intersection_area / float(box_a_area + box_b_area - intersection_area)
    else:
        iou = 0.0

    return iou

def find_matching_boxes(boxes_a, boxes_b, threshold=0.05):
    matching_indices = []

    for idx_b, box_b in enumerate(boxes_b):
        for box_a in boxes_a:
            iou = calculate_iou(box_a, box_b)
            if iou >= threshold and iou<0.85:
                matching_indices.append(idx_b)
                break  # Exit the inner loop once a match is found for efficiency

    return matching_indices


def draw_boxes(image_np, boxes_tensor):
    image_bgr = cv2.cvtColor(image_np, cv2.COLOR_BGR2RGB)
    
    for box in boxes_tensor:
        x1, y1, x2, y2 = box.cpu().numpy().astype(int)
        cv2.rectangle(image_bgr, (x1, y1), (x2, y2), (0, 255, 0), 2)  # Green color, thickness 2
    cv2.imwrite('tmp.jpg', image_bgr)

def draw_hos(batched_input, instances, score=0.5):
    image_np = batched_input["image"].permute(1, 2, 0).numpy()
    image_np = cv2.cvtColor(image_np, cv2.COLOR_BGR2RGB)
    image_np = cv2.cvtColor(image_np, cv2.COLOR_BGR2RGB)

    orig_shape = (batched_input["height"], batched_input["width"])
    new_shape = (image_np.shape[0], image_np.shape[1])

    instances = instances[instances.scores > score]
    hands = instances[instances.pred_classes == 0]

    offset_hands = hands.pred_offsets
    bboxes_hands = hands.pred_boxes.tensor

    boxes_tensor = instances.pred_boxes.tensor
    
    for box in boxes_tensor:
        x1, y1, x2, y2 = box.cpu().numpy().astype(int)
        cv2.rectangle(image_np, (x1, y1), (x2, y2), (0, 255, 0), 2)  # Green color, thickness 2

    for hand_box, offset in zip(bboxes_hands, offset_hands):
        offset[2] = resized_distance(offset[2], orig_shape, new_shape)
        h_center = get_center(hand_box.cpu().numpy())
        scalar = 1000
        offset_vec = [ offset[0]*offset[2]*scalar, offset[1]*offset[2]*scalar ] 
        pred_o_center = [h_center[0]+offset_vec[0], h_center[1]+offset_vec[1]]
        cv2.line(image_np, (int(h_center[0]), int(h_center[1])), (int(pred_o_center[0]), int(pred_o_center[1])), (255, 0, 0), 2)  # Draw a line from (50, 50) to (200, 200)

    cv2.imwrite('tmp.jpg', image_np)

def resized_distance(original_distance, original_size, new_size):
    original_height, original_width = original_size
    new_height, new_width = new_size

    # Calculate the resizing ratios
    height_ratio = new_height / original_height
    width_ratio = new_width / original_width

    # Apply the resizing ratios to the original distance
    new_distance = original_distance * min(height_ratio, width_ratio)

    return new_distance

@META_ARCH_REGISTRY.register()
class DAobjTwoStagePseudoLabGeneralizedRCNN(GeneralizedRCNN):
    @configurable
    def __init__(
        self,
        *,
        backbone: Backbone,
        proposal_generator: nn.Module,
        roi_heads: nn.Module,
        pixel_mean: Tuple[float],
        pixel_std: Tuple[float],
        input_format: Optional[str] = None,
        vis_period: int = 0,
        dis_type: str,
    ):
        super(GeneralizedRCNN, self).__init__()
        self.backbone = backbone
        self.proposal_generator = proposal_generator
        self.roi_heads = roi_heads

        self.input_format = input_format
        self.vis_period = vis_period
        if vis_period > 0:
            assert input_format is not None, "input_format is required for visualization!"

        self.register_buffer("pixel_mean", torch.tensor(pixel_mean).view(-1, 1, 1), False)
        self.register_buffer("pixel_std", torch.tensor(pixel_std).view(-1, 1, 1), False)
        assert (
            self.pixel_mean.shape == self.pixel_std.shape
        ), f"{self.pixel_mean} and {self.pixel_std} have different shapes!"

        self.dis_type = dis_type        
        self.D_img = FCDiscriminator_img(self.backbone._out_feature_channels[self.dis_type]) # Need to know the channel

    def build_discriminator(self):
        self.D_img = FCDiscriminator_img(self.backbone._out_feature_channels[self.dis_type]).to(self.device) # Need to know the channel

    @classmethod
    def from_config(cls, cfg):
        backbone = build_backbone(cfg)
        return {
            "backbone": backbone,
            "proposal_generator": build_proposal_generator(cfg, backbone.output_shape()),
            "roi_heads": build_roi_heads(cfg, backbone.output_shape()),
            "input_format": cfg.INPUT.FORMAT,
            "vis_period": cfg.VIS_PERIOD,
            "pixel_mean": cfg.MODEL.PIXEL_MEAN,
            "pixel_std": cfg.MODEL.PIXEL_STD,
            "dis_type": cfg.SEMISUPNET.DIS_TYPE,
        }

    def preprocess_image_train(self, batched_inputs: List[Dict[str, torch.Tensor]]):
        """
        Normalize, pad and batch the input images.
        """
        images = [x["image"].to(self.device) for x in batched_inputs]
        images = [(x - self.pixel_mean) / self.pixel_std for x in images]
        images = ImageList.from_tensors(images, self.backbone.size_divisibility)

        images_t = [x["image_unlabeled"].to(self.device) for x in batched_inputs]
        images_t = [(x - self.pixel_mean) / self.pixel_std for x in images_t]
        images_t = ImageList.from_tensors(images_t, self.backbone.size_divisibility)

        return images, images_t

    def forward(self, batched_inputs, branch="supervised", given_proposals=None, val_mode=False):
        if self.D_img == None:
            self.build_discriminator()
        if (not self.training) and (not val_mode):  # only conduct when testing mode
            return self.inference(batched_inputs)

        source_label = 0
        target_label = 1

        if branch == "domain":

            images_s, images_t = self.preprocess_image_train(batched_inputs)
            features = self.backbone(images_s.tensor)
           
            features_s = grad_reverse(features[self.dis_type])
            D_img_out_s = self.D_img(features_s)
            loss_D_img_s = F.binary_cross_entropy_with_logits(D_img_out_s, torch.FloatTensor(D_img_out_s.data.size()).fill_(source_label).to(self.device))

            features_t = self.backbone(images_t.tensor)
            
            features_t = grad_reverse(features_t[self.dis_type])
            D_img_out_t = self.D_img(features_t)
            loss_D_img_t = F.binary_cross_entropy_with_logits(D_img_out_t, torch.FloatTensor(D_img_out_t.data.size()).fill_(target_label).to(self.device))

            losses = {}
            losses["loss_D_img_s"] = loss_D_img_s
            losses["loss_D_img_t"] = loss_D_img_t
            return losses, [], [], None

        images = self.preprocess_image(batched_inputs)

        if "instances" in batched_inputs[0]:
            gt_instances = [x["instances"].to(self.device) for x in batched_inputs]
        else:
            gt_instances = None

        features = self.backbone(images.tensor)

        if branch == "supervised":
            features_s = grad_reverse(features[self.dis_type])
            D_img_out_s = self.D_img(features_s)
            loss_D_img_s = F.binary_cross_entropy_with_logits(D_img_out_s, torch.FloatTensor(D_img_out_s.data.size()).fill_(source_label).to(self.device))

            
            # Region proposal network
            proposals_rpn, proposal_losses = self.proposal_generator(
                images, features, gt_instances
            )

            # roi_head lower branch
            _, detector_losses = self.roi_heads(
                images,
                features,
                proposals_rpn,
                compute_loss=True,
                targets=gt_instances,
                branch=branch,
            )

            # visualization
            if self.vis_period > 0:
                storage = get_event_storage()
                if storage.iter % self.vis_period == 0:
                    self.visualize_training(batched_inputs, proposals_rpn, branch)

            losses = {}
            losses.update(detector_losses)
            losses.update(proposal_losses)
            losses["loss_D_img_s"] = loss_D_img_s*0.001
            return losses, [], [], None

        elif branch == "supervised_target":

            # Region proposal network
            proposals_rpn, proposal_losses = self.proposal_generator(
                images, features, gt_instances
            )

            # roi_head lower branch
            _, detector_losses = self.roi_heads(
                images,
                features,
                proposals_rpn,
                compute_loss=True,
                targets=gt_instances,
                branch=branch,
            )

            # visualization
            if self.vis_period > 0:
                storage = get_event_storage()
                if storage.iter % self.vis_period == 0:
                    self.visualize_training(batched_inputs, proposals_rpn, branch)

            losses = {}
            losses.update(detector_losses)
            losses.update(proposal_losses)

            return losses, [], [], None

        elif branch == "unsup_data_weak":
            """
            unsupervised weak branch: input image without any ground-truth label; output proposals of rpn and roi-head
            """
            # Region proposal network
            proposals_rpn, _ = self.proposal_generator(
                images, features, None, compute_loss=False
            )

            # roi_head lower branch (keep this for further production)
            # notice that we do not use any target in ROI head to do inference!
            proposals_roih, ROI_predictions = self.roi_heads(
                images,
                features,
                proposals_rpn,
                targets=None,
                compute_loss=False,
                branch=branch,
            )

            return {}, proposals_rpn, proposals_roih, ROI_predictions

    def visualize_training(self, batched_inputs, proposals, branch=""):
        """
        This function different from the original one:
        - it adds "branch" to the `vis_name`.

        A function used to visualize images and proposals. It shows ground truth
        bounding boxes on the original image and up to 20 predicted object
        proposals on the original image. Users can implement different
        visualization functions for different models.

        Args:
            batched_inputs (list): a list that contains input to the model.
            proposals (list): a list that contains predicted proposals. Both
                batched_inputs and proposals should have the same length.
        """
        from detectron2.utils.visualizer import Visualizer

        storage = get_event_storage()
        max_vis_prop = 20

        for input, prop in zip(batched_inputs, proposals):
            img = input["image"]
            img = convert_image_to_rgb(img.permute(1, 2, 0), self.input_format)
            v_gt = Visualizer(img, None)
            v_gt = v_gt.overlay_instances(boxes=input["instances"].gt_boxes)
            anno_img = v_gt.get_image()
            box_size = min(len(prop.proposal_boxes), max_vis_prop)
            v_pred = Visualizer(img, None)
            v_pred = v_pred.overlay_instances(
                boxes=prop.proposal_boxes[0:box_size].tensor.cpu().numpy()
            )
            prop_img = v_pred.get_image()
            vis_img = np.concatenate((anno_img, prop_img), axis=1)
            vis_img = vis_img.transpose(2, 0, 1)
            vis_name = (
                "Left: GT bounding boxes "
                + branch
                + ";  Right: Predicted proposals "
                + branch
            )
            storage.put_image(vis_name, vis_img)
            break  # only visualize one image in a batch


@META_ARCH_REGISTRY.register()
class DAHOSMultiBackboneRCNN(DAobjTwoStagePseudoLabGeneralizedRCNN):
    def __init__(self,cfg):
        super(DAobjTwoStagePseudoLabGeneralizedRCNN, self).__init__(cfg)

        self.backbone_contact = build_backbone(cfg)        
        self.dis_type = cfg.SEMISUPNET.DIS_TYPE        
        self.D_img = FCDiscriminator_img(self.backbone._out_feature_channels[self.dis_type]) # Need to know the channel
        self.last_features_source = None
        self.last_features_target = None
        self.source_label = 0
        self.target_label = 1

        self.depth_enabled = cfg.MODEL.get("DEPTH_ENABLED", False)
        self.weak_enabled = cfg.MODEL.get("WEAK_LABEL", False)
        self.bbox_thresh  = cfg.SEMISUPNET.BBOX_THRESHOLD
            
    @classmethod
    def from_config(cls, cfg):
        return super(DAobjTwoStagePseudoLabGeneralizedRCNN, cls).from_config(cfg)
    
    def preprocess_depth(self, batched_inputs: List[Dict[str, torch.Tensor]]):
        images = [self._move_to_current_device(x["depth"]).to(dtype=torch.float32) for x in batched_inputs]
        images = ImageList.from_tensors(images, self.backbone.size_divisibility, padding_constraints=self.backbone.padding_constraints,)
        return images
    
    def forward_domain(self):
        
        features_s = grad_reverse(self.last_features_source["features"][self.dis_type])    
        D_img_out_s = self.D_img(features_s)
        loss_D_img_s = F.binary_cross_entropy_with_logits(D_img_out_s, torch.FloatTensor(D_img_out_s.data.size()).fill_(self.source_label).to(self.device))

        features_t = grad_reverse(self.last_features_target["features"][self.dis_type])
        D_img_out_t = self.D_img(features_t)
        loss_D_img_t = F.binary_cross_entropy_with_logits(D_img_out_t, torch.FloatTensor(D_img_out_t.data.size()).fill_(self.target_label).to(self.device))

        ## CONTACT
        #features_contact_s = grad_reverse(self.last_features_source["features_contact"][self.dis_type])    
        #D_img_out_contact_s = self.D_img(features_contact_s)
        #loss_D_img_s += F.binary_cross_entropy_with_logits(D_img_out_contact_s, torch.FloatTensor(D_img_out_s.data.size()).fill_(self.source_label).to(self.device))

        #features_contact_t = grad_reverse(self.last_features_target["features_contact"][self.dis_type])    
        #D_img_out_contact_t = self.D_img(features_contact_t)
        #loss_D_img_t += F.binary_cross_entropy_with_logits(D_img_out_contact_t, torch.FloatTensor(D_img_out_t.data.size()).fill_(self.target_label).to(self.device))

        losses = {}
        losses["loss_D_img_s"] = loss_D_img_s
        losses["loss_D_img_t"] = loss_D_img_t
        return losses, [], [], None

    def get_hands(self, instances):
        if len(instances) == 0: return []
        hands_predictions = instances[instances.pred_classes == 0]
        return hands_predictions
    
    def get_objects(self, instances):
        if len(instances) == 0: return []
        objects_predictions = instances[instances.pred_classes == 1]
        return objects_predictions

    def get_hands_incontact_instances(self, instances):
        hands_predictions = self.get_hands(instances)
        if len(hands_predictions) == 0: return []
        hands_predictions = hands_predictions[hands_predictions.pred_contacts.argmax(dim=1) == 1]
        return hands_predictions

    def selection_by_nms(self, batched_inputs, proposals_roih):
        new_proposals_roih = []
        try:
            for i, instances in enumerate(proposals_roih):
                hands_c = self.get_hands_incontact_instances(instances)
                hands_c = hands_c[hands_c.scores > self.bbox_thresh]
                objects = self.get_objects(instances)

                device = hands_c.pred_boxes.tensor.device
                hands = self.get_hands(instances)
                new_elements = {
                    "pred_boxes": hands.pred_boxes.tensor,
                    "pred_classes": hands.pred_classes,
                    "scores": hands.scores,
                    "pred_handsides": hands.pred_handsides,
                    "pred_contacts": hands.pred_contacts,
                    "pred_offsets": hands.pred_offsets
                }

                if len(hands_c) > 0 and len(objects) > 0: 
                    if len(objects) > len(hands_c) + 1:
                        mean_score = objects.scores.mean()
                        new_objects = objects[objects.scores > mean_score]
                    else:
                        new_objects = objects

                    indices = batched_nms(
                        boxes=new_objects.pred_boxes.tensor,
                        scores=new_objects.scores,
                        idxs=new_objects.pred_classes, 
                        iou_threshold=0.5
                    )
                    if len(indices) == 0: 
                        continue

                    new_objects = new_objects[indices]

                    orig_shape = (batched_inputs[i]["height"], batched_inputs[i]["width"])
                    new_shape = (batched_inputs[i]["image"].shape[1], batched_inputs[i]["image"].shape[2])

                    obj_bboxes = [box.cpu().numpy() for box in new_objects.pred_boxes.tensor]

                    selected_objects = []
                    for hand_bbox, hand_offset in zip(hands_c.pred_boxes.tensor.cpu().numpy(), hands_c.pred_offsets.cpu().numpy()):
                        hand_offset[2] = resized_distance(hand_offset[2], orig_shape, new_shape)
                        selected_objects.append(get_incontact_obj(hand_bbox, hand_offset, obj_bboxes))
                    
                    idxs = list(set([s_o[1] for s_o in selected_objects]))
                    for idx in idxs:
                        new_elements["scores"] = torch.cat((new_elements["scores"], torch.tensor([0.95]).to(device)))
                        new_elements["pred_boxes"] = torch.cat((new_elements["pred_boxes"], torch.tensor(obj_bboxes[idx]).unsqueeze(0).to(device)))
                        new_elements["pred_handsides"] = torch.cat((new_elements["pred_handsides"], torch.tensor([[1, 0]]).to(device)))
                        new_elements["pred_contacts"] = torch.cat((new_elements["pred_contacts"], torch.tensor([[1, 0]]).to(device)))
                        new_elements["pred_classes"] = torch.cat((new_elements["pred_classes"], torch.tensor([1]).to(device)))
                        new_elements["pred_offsets"] = torch.cat((new_elements["pred_offsets"], torch.tensor([[-1, -1, -1]]).to(device)))
                
                new_elements["pred_boxes"] = Boxes(new_elements["pred_boxes"])
                new_proposals_roih.append(Instances(instances._image_size, **new_elements))

        except Exception as e:
            print("An error occurred:", e)
            return proposals_roih
        return new_proposals_roih

    def merge_gt_and_pred(self, proposals_roih, gt_instances):
        if isinstance(gt_instances, type(None)): 
            return proposals_roih
        
        new_proposals_roih = []
        for current_proposal_roi, current_gt_instances in zip(proposals_roih, gt_instances):
            objs_instances = current_proposal_roi[current_proposal_roi.pred_classes == 1]
            device = objs_instances.pred_boxes.tensor.device
            new_elements = {
                "pred_boxes": torch.cat((objs_instances.pred_boxes.tensor, current_gt_instances.gt_boxes.tensor)),
                "pred_classes": torch.cat((objs_instances.pred_classes, current_gt_instances.gt_classes)),
                "scores": torch.cat((objs_instances.scores, torch.tensor([0.95 for _ in range(len(current_gt_instances))]).to(device))),
                "pred_offsets": torch.cat((objs_instances.pred_offsets, current_gt_instances.gt_offsets)),
                "pred_handsides": torch.cat(
                    (
                        objs_instances.pred_handsides, 
                        torch.tensor([[0.0, 1.0] if c.item() == 1 else [1.0, 0.0] for c in current_gt_instances.gt_handsides]).to(device)
                    )),
                "pred_contacts": torch.cat(
                    (
                        objs_instances.pred_contacts, 
                        torch.tensor([[0.0, 1.0] if c.item() == 1 else [1.0, 0.0] for c in current_gt_instances.gt_contacts]).to(device)
                    )),
            }
            new_elements["pred_boxes"] = Boxes(new_elements["pred_boxes"])
            new_proposals_roih.append(Instances(objs_instances._image_size, **new_elements))

        return new_proposals_roih

    def forward(self, batched_inputs, branch="supervised", given_proposals=None, val_mode=False):
        if (not self.training) and (not val_mode):  # only conduct when testing mode
            return self.inference(batched_inputs)

        if branch == "domain":
            return self.forward_domain()

        images = self.preprocess_image(batched_inputs)
        depths = self.preprocess_depth(batched_inputs) if self.depth_enabled else None

        try:
            gt_instances = [x["instances"].to(self.device) for x in batched_inputs] if "instances" in batched_inputs[0] else None
        except:
            gt_instances = None
            
        features = self.backbone(images.tensor)
        features_contact = self.backbone_contact(images.tensor) if not self.depth_enabled else self.backbone_contact(depths.tensor)
        
        if branch == "supervised":
            self.last_features_source = {"features": features, "features_contact": features_contact}

            features_s = grad_reverse(features[self.dis_type])
            D_img_out_s = self.D_img(features_s)
            loss_D_img_s = F.binary_cross_entropy_with_logits(D_img_out_s, torch.FloatTensor(D_img_out_s.data.size()).fill_(self.source_label).to(self.device))

            # Region proposal network
            proposals_rpn, proposal_losses = self.proposal_generator(images, features, gt_instances)

            # roi_head lower branch
            _, detector_losses = self.roi_heads(
                images,
                features,
                features_contact,
                proposals_rpn,
                compute_loss=True,
                targets=gt_instances,
                branch=branch,
            )

            # visualization
            if self.vis_period > 0:
                storage = get_event_storage()
                if storage.iter % self.vis_period == 0:
                    self.visualize_training(batched_inputs, proposals_rpn, branch)

            losses = {}
            losses.update(detector_losses)
            losses.update(proposal_losses)
            losses["loss_D_img_s"] = loss_D_img_s*0.001
            return losses, [], [], None

        elif branch == "supervised_target":
            self.last_features_target = {"features": features, "features_contact": features_contact}

            # Region proposal network
            proposals_rpn, proposal_losses = self.proposal_generator(images, features, gt_instances)

            # roi_head lower branch
            _, detector_losses = self.roi_heads(
                images,
                features,
                features_contact,
                proposals_rpn,
                compute_loss=True,
                targets=gt_instances,
                branch=branch,
            )

            # visualization
            if self.vis_period > 0:
                storage = get_event_storage()
                if storage.iter % self.vis_period == 0:
                    self.visualize_training(batched_inputs, proposals_rpn, branch)

            losses = {}
            losses.update(detector_losses)
            losses.update(proposal_losses)

            return losses, [], [], None

        elif branch == "unsup_data_weak":
            # Region proposal network
            proposals_rpn, _ = self.proposal_generator(images, features, None, compute_loss=False)

            # roi_head lower branch (keep this for further production)
            # notice that we do not use any target in ROI head to do inference!
            proposals_roih, ROI_predictions = self.roi_heads(
                images,
                features,
                features_contact,
                proposals_rpn,
                targets=None,
                compute_loss=False,
                branch=branch,
            )

            if self.weak_enabled:
                proposals_roih = self.merge_gt_and_pred(proposals_roih=proposals_roih, gt_instances=gt_instances)

            return {}, proposals_rpn, proposals_roih, ROI_predictions
        
    def inference(
        self,
        batched_inputs: List[Dict[str, torch.Tensor]],
        do_postprocess: bool = True,
    ):

        assert not self.training

        images = self.preprocess_image(batched_inputs)
        depths = self.preprocess_depth(batched_inputs) if self.depth_enabled else None

        features = self.backbone(images.tensor)
        features_contact = self.backbone_contact(images.tensor) if not self.depth_enabled else self.backbone_contact(depths.tensor)

        if self.proposal_generator is not None:
            proposals, _ = self.proposal_generator(images, features, None)
        else:
            assert "proposals" in batched_inputs[0]
            proposals = [x["proposals"].to(self.device) for x in batched_inputs]
        results, _ = self.roi_heads(images, features, features_contact, proposals, None)
        
        if do_postprocess:
            assert not torch.jit.is_scripting(), "Scripting is not supported for postprocess."
            return GeneralizedRCNN._postprocess(results, batched_inputs, images.image_sizes)
        return results

@META_ARCH_REGISTRY.register()
class DAHOSMultiBackboneFreezedRCNN(DAobjTwoStagePseudoLabGeneralizedRCNN):
    def __init__(self,cfg):
        super(DAobjTwoStagePseudoLabGeneralizedRCNN, self).__init__(cfg)

        self.backbone_contact = build_backbone(cfg)        
        self.dis_type = cfg.SEMISUPNET.DIS_TYPE        
        self.D_img = FCDiscriminator_img(self.backbone._out_feature_channels[self.dis_type]) # Need to know the channel
        self.last_features_source = None
        self.last_features_target = None
        self.source_label = 0
        self.target_label = 1

        self.depth_enabled = cfg.MODEL.get("DEPTH_ENABLED", False)

        for name, param in self.named_parameters():
            if "contact" in name:
                param.requires_grad = True
            else:
                param.requires_grad = False
            
            if "fpn_output5" in name or "fpn_output4" in name or "fpn_output3" in name:
                param.requires_grad = False
            
    @classmethod
    def from_config(cls, cfg):
        return super(DAobjTwoStagePseudoLabGeneralizedRCNN, cls).from_config(cfg)
    
    def preprocess_depth(self, batched_inputs: List[Dict[str, torch.Tensor]]):
        images = [self._move_to_current_device(x["depth"]).to(dtype=torch.float32) for x in batched_inputs]
        images = ImageList.from_tensors(images, self.backbone.size_divisibility, padding_constraints=self.backbone.padding_constraints,)
        return images


    def forward(self, batched_inputs, branch="supervised", given_proposals=None, val_mode=False):
        if (not self.training) and (not val_mode):  # only conduct when testing mode
            return self.inference(batched_inputs)

        images = self.preprocess_image(batched_inputs)
        depths = self.preprocess_depth(batched_inputs) if self.depth_enabled else None

        try:
            gt_instances = [x["instances"].to(self.device) for x in batched_inputs] if "instances" in batched_inputs[0] else None
        except:
            gt_instances = None
            
        with torch.no_grad():
            features = self.backbone(images.tensor)

        features_contact = self.backbone_contact(images.tensor) if not self.depth_enabled else self.backbone_contact(depths.tensor)
        
        if branch == "supervised":
            
            with torch.no_grad():
                # Region proposal network
                proposals_rpn, proposal_losses = self.proposal_generator(images, features, gt_instances)

            # roi_head lower branch
            _, detector_losses = self.roi_heads(
                images,
                features,
                features_contact,
                proposals_rpn,
                compute_loss=True,
                targets=gt_instances,
                branch=branch,
            )

            # visualization
            if self.vis_period > 0:
                storage = get_event_storage()
                if storage.iter % self.vis_period == 0:
                    self.visualize_training(batched_inputs, proposals_rpn, branch)

            losses = {}
            losses.update(detector_losses)
            losses.update(proposal_losses)
            return losses, [], [], None

        elif branch == "supervised_target":
            
            with torch.no_grad():
                # Region proposal network
                proposals_rpn, proposal_losses = self.proposal_generator(images, features, gt_instances)

            # roi_head lower branch
            _, detector_losses = self.roi_heads(
                images,
                features,
                features_contact,
                proposals_rpn,
                compute_loss=True,
                targets=gt_instances,
                branch=branch,
            )

            # visualization
            if self.vis_period > 0:
                storage = get_event_storage()
                if storage.iter % self.vis_period == 0:
                    self.visualize_training(batched_inputs, proposals_rpn, branch)

            losses = {}
            losses.update(detector_losses)
            losses.update(proposal_losses)

            return losses, [], [], None

        elif branch == "unsup_data_weak":
            with torch.no_grad():
                # Region proposal network
                proposals_rpn, _ = self.proposal_generator(images, features, None, compute_loss=False)

                # roi_head lower branch (keep this for further production)
                # notice that we do not use any target in ROI head to do inference!
                proposals_roih, ROI_predictions = self.roi_heads(
                    images,
                    features,
                    features_contact,
                    proposals_rpn,
                    targets=None,
                    compute_loss=False,
                    branch=branch,
                )

            return {}, proposals_rpn, proposals_roih, ROI_predictions
        
    def inference(
        self,
        batched_inputs: List[Dict[str, torch.Tensor]],
        do_postprocess: bool = True,
    ):

        assert not self.training

        images = self.preprocess_image(batched_inputs)
        depths = self.preprocess_depth(batched_inputs) if self.depth_enabled else None

        features = self.backbone(images.tensor)
        features_contact = self.backbone_contact(images.tensor) if not self.depth_enabled else self.backbone_contact(depths.tensor)

        if self.proposal_generator is not None:
            proposals, _ = self.proposal_generator(images, features, None)
        else:
            assert "proposals" in batched_inputs[0]
            proposals = [x["proposals"].to(self.device) for x in batched_inputs]
        results, _ = self.roi_heads(images, features, features_contact, proposals, None)
        
        if do_postprocess:
            assert not torch.jit.is_scripting(), "Scripting is not supported for postprocess."
            return GeneralizedRCNN._postprocess(results, batched_inputs, images.image_sizes)
        return results
    
