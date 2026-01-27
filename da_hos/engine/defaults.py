import torch

import detectron2.data.transforms as T
from detectron2.checkpoint import DetectionCheckpointer
from detectron2.data import (
    MetadataCatalog,
)
from detectron2.modeling import build_model

from da_hos.engine.trainer_da_hos import DAHOSTrainer
from da_hos.modeling.meta_arch.ts_ensemble import EnsembleTSModel
import da_hos.modeling


class DAHOSPredictor:

    def __init__(self, cfg):
        self.cfg = cfg.clone()

        self.model = DAHOSTrainer.build_model(self.cfg)
        self.model_teacher = DAHOSTrainer.build_model(self.cfg)
        self.ensem_ts_model = EnsembleTSModel(self.model_teacher, self.model)
        DetectionCheckpointer(
           self.ensem_ts_model, save_dir=cfg.OUTPUT_DIR
        ).resume_or_load(cfg.MODEL.WEIGHTS)

        self.aug = T.ResizeShortestEdge(
            [cfg.INPUT.MIN_SIZE_TEST, cfg.INPUT.MIN_SIZE_TEST], cfg.INPUT.MAX_SIZE_TEST
        )
        self.ensem_ts_model.eval()
        self.input_format = cfg.INPUT.FORMAT
        assert self.input_format in ["RGB", "BGR"], self.input_format

    def __call__(self, original_image):
        """
        Args:
            original_image (np.ndarray): an image of shape (H, W, C) (in BGR order).

        Returns:
            predictions (dict):
                the output of the model for one image only.
                See :doc:`/tutorials/models` for details about the format.
        """
        with torch.no_grad():  # https://github.com/sphinx-doc/sphinx/issues/4258
            # Apply pre-processing to image.
            if self.input_format == "RGB":
                # whether the model expects BGR inputs or RGB
                original_image = original_image[:, :, ::-1]
            height, width = original_image.shape[:2]
            image = self.aug.get_transform(original_image).apply_image(original_image)
            image = torch.as_tensor(image.astype("float32").transpose(2, 0, 1))

            inputs = {"image": image, "height": height, "width": width}
            predictions = self.model([inputs])[0]
            return predictions