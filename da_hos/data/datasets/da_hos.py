from da_hos.data.datasets.epick import register_epick_instances
from detectron2.data import MetadataCatalog, build_detection_train_loader
from da_hos.data.datasets.builtin import register_coco_unlabel_instances
from detectron2.data.datasets import register_coco_instances


def register_da_hos_datasets(cfg, test=False, task="hand_obj"):

    if not test:
        ## register dataset
        register_epick_instances(
            f"{cfg.DATASETS.TRAIN[0]}",
            {},
            f"{cfg.DATASETS.TRAIN_ANNOTATIONS_PATH[0]}",
            f"{cfg.DATASETS.TRAIN_IMAGES_PATH[0]}",
        )
        MetadataCatalog.get(f"{cfg.DATASETS.TRAIN[0]}").thing_classes = [
            "hand",
            "object",
        ]
        register_coco_unlabel_instances(
            name=cfg.DATASETS.TRAIN_UNLABEL[0],
            metadata={},
            json_file=cfg.DATASETS.TRAIN_UNLABEL_ANNOTATIONS_PATH[0],
            image_root=cfg.DATASETS.TRAIN_UNLABEL_IMAGES_PATH[0],
        )

    register_epick_instances(
        f"{cfg.DATASETS.TEST[0]}",
        {},
        f"{cfg.DATASETS.TEST_ANNOTATIONS_PATH[0]}",
        f"{cfg.DATASETS.TEST_IMAGES_PATH[0]}",
    )

    if task == "hand_obj":
        MetadataCatalog.get(f"{cfg.DATASETS.TEST[0]}").thing_classes = [
            "hand",
            "object",
        ]
    elif task == "handside":
        MetadataCatalog.get(f"{cfg.DATASETS.TEST[0]}").thing_classes = ["left", "right"]
    elif task == "contact":
        MetadataCatalog.get(f"{cfg.DATASETS.TEST[0]}").thing_classes = [
            "not_incontact",
            "incontact",
        ]
    elif task == "combineHO":
        MetadataCatalog.get(f"{cfg.DATASETS.TEST[0]}").thing_classes = [
            "combineHandObj"
        ]
    else:
        assert False, "Task not found."


def register_da_hos_multi_datasets(cfg, test=False, task="hand_obj"):
    register_da_hos_datasets(cfg, test=False, task="hand_obj")
    register_epick_instances(
        name=cfg.DATASETS.TRAIN_WEAK_LABEL[0],
        metadata={},
        json_file=cfg.DATASETS.TRAIN_WEAK_ANNOTATIONS_PATH[0],
        image_root=cfg.DATASETS.TRAIN_WEAK_IMAGES_PATH[0],
    )
