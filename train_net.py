import os
from datetime import datetime

import detectron2.utils.comm as comm
from detectron2.config import get_cfg
from detectron2.engine import (
    default_argument_parser,
    default_setup,
    launch,
)
from detectron2.projects.point_rend import add_pointrend_config  # type: ignore

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

    if comm.is_main_process():
        if args.resume:
            cfg.OUTPUT_DIR = args.output_dir
        else:
            cfg.OUTPUT_DIR = os.path.join(
                args.output_dir,
                f"{cfg.DATASETS.TRAIN[0]}_{args.timestamp}",
            )

    cfg.freeze()
    default_setup(cfg, args)
    return cfg


def main(args):

    ### Setup cfg
    cfg = setup(args)

    register_da_hos_datasets(cfg)

    trainer_obj = DAHOSTrainer
    trainer = trainer_obj(cfg)

    trainer.resume_or_load(resume=args.resume)
    return trainer.train()


if __name__ == "__main__":

    parser = default_argument_parser()
    parser._option_string_actions["--config-file"].default = (
        "./configs/da_hos_resnet_101-FPN/VISOR/(S)hoisynth-(T)visor.yaml"
    )

    parser.add_argument(
        "--output_dir",
        default="./checkpoints/",
        help="Output dir.",
    )

    args = parser.parse_args()
    args.timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")

    print("Command Line Args:", args)

    # run
    launch(
        main,
        args.num_gpus,
        num_machines=args.num_machines,
        machine_rank=args.machine_rank,
        dist_url=args.dist_url,
        args=(args,),
    )
