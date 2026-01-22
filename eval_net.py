#!/usr/bin/env python3
# Copyright (c) Facebook, Inc. and its affiliates.
# Modified by Rosario Leonardi, 2025.

import os

from detectron2.checkpoint import DetectionCheckpointer
from detectron2.config import get_cfg
from detectron2.engine import (
    default_argument_parser,
    default_setup,
    launch,
)

from detectron2.projects.point_rend import add_pointrend_config
from da_hos.modeling.meta_arch.ts_ensemble import EnsembleTSModel

from da_hos.config import add_ateacher_config
from da_hos.data.datasets.da_hos import register_da_hos_datasets
from da_hos.engine.trainer_da_hos import DAHOSTrainer


def setup(args):
    """
    Create configs and perform basic setups.
    """
    cfg = get_cfg()
    cfg.set_new_allowed(True)

    # Pointrend
    add_pointrend_config(cfg)

    # adaptive teacher
    add_ateacher_config(cfg)

    cfg.merge_from_file(args.config_file)
    cfg.merge_from_list(args.opts)

    cfg.DATASETS.TEST = ("test_dataset",)
    cfg.DATASETS.TEST_ANNOTATIONS_PATH = (args.annotations,)
    cfg.DATASETS.TEST_IMAGES_PATH = (args.images,)

    cfg.TASK = args.task
    cfg.MODEL.WEIGHTS = args.weights

    output_dir = os.path.join(args.weights[: args.weights.rfind("/") + 1], f"eval")
    cfg.OUTPUT_DIR = output_dir

    os.makedirs(output_dir, exist_ok=True)

    cfg.freeze()
    default_setup(cfg, args)
    return cfg


def main(args):
    cfg = setup(args)

    register_da_hos_datasets(cfg, test=True, task=args.task)

    model = DAHOSTrainer.build_model(cfg)
    model_teacher = DAHOSTrainer.build_model(cfg)
    ensem_ts_model = EnsembleTSModel(model_teacher, model)
    DetectionCheckpointer(ensem_ts_model, save_dir=cfg.OUTPUT_DIR).resume_or_load(
        cfg.MODEL.WEIGHTS, resume=args.resume
    )

    evaluators = [
        DAHOSTrainer.build_evaluator(cfg, cfg.DATASETS.TEST[0], task=args.task)
    ]

    res = DAHOSTrainer.test(cfg, ensem_ts_model.modelTeacher, evaluators)
    return res


if __name__ == "__main__":
    parser = default_argument_parser()

    parser._option_string_actions["--config-file"].default = (
        "./configs/da_hos_resnet_101-FPN/VISOR/S_hoisynth-T_visor.yaml"
    )

    parser.add_argument(
        "--task",
        default="hand_obj",
        choices=["hand_obj", "handside", "contact", "combineHO"],
    )

    parser.add_argument(
        "--annotations",
        help="Dataset path of val annotations.",
        required=True,
    )

    parser.add_argument("--images", help="Dataset path of val images.", required=True)

    parser.add_argument("--weights", help="Weights pth path", required=True)

    args = parser.parse_args()

    # run
    launch(
        main,
        args.num_gpus,
        num_machines=args.num_machines,
        machine_rank=args.machine_rank,
        dist_url=args.dist_url,
        args=(args,),
    )
