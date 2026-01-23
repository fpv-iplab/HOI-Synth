# HOI-Synth: Domain Adaptation Branch

## Model Zoo
We provide pre-trained models for different datasets and training settings. All models are trained using the configurations released in this repository and can be directly used for evaluation or fine-tuning.

### VISOR

| Setting   | Backbone | Training Set | Overall AP@50 | Download |
| :---:     | :---: | :---: | :---: | :---: |
| UDA       | ResNet-101-FPN | HOI-Synth | 33.33 | [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/VISOR/S_hoisynth-T_visor/model.pth) \| [config](configs/da_hos_resnet_101-FPN/VISOR/S_hoisynth-T_visor.yaml) |
| SSDA      | ResNet-101-FPN | HOI-Synth + VISOR 10% | 44.22 | [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/VISOR/S_hoisynth+visor10-T_visor/model.pth) \| [config](configs/da_hos_resnet_101-FPN/VISOR/S_hoisynth+visor10-T_visor.yaml) |
| SSDA      | ResNet-101-FPN | HOI-Synth + VISOR 25% | 45.55 | [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/VISOR/S_hoisynth+visor25-T_visor/model.pth) \| [config](configs/da_hos_resnet_101-FPN/VISOR/S_hoisynth+visor25-T_visor.yaml) |
| SSDA      | ResNet-101-FPN | HOI-Synth + VISOR 50% | 46.47 | [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/VISOR/S_hoisynth+visor50-T_visor/model.pth) \| [config](configs/da_hos_resnet_101-FPN/VISOR/S_hoisynth+visor50-T_visor.yaml) |
| FSDA      | ResNet-101-FPN | HOI-Synth + VISOR 100% | 46.48 | [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/VISOR/S_hoisynth+visor100-T_visor/model.pth) \| [config](configs/da_hos_resnet_101-FPN/VISOR/S_hoisynth+visor100-T_visor.yaml) |

## Training & Evaluation

We provide scripts for training and evaluating models using the `train_net.py` and `eval_net.py` entry points, following the standard Detectron2 workflow.

### Training

To train a model, select the configuration file corresponding to the desired experimental setting and specify the number of GPUs.

**Example: UDA Training (HOI-Synth → VISOR)**

To reproduce the UDA result (Row 1 of the table):

```bash
python train_net.py --num-gpus 4 --config-file configs/da_hos_resnet_101-FPN/VISOR/S_hoisynth-T_visor.yaml 
```

### Evaluation

For evaluation, use the `eval_net.py` script and provide the path to the trained model checkpoint.

**Example: Evaluation on VISOR**

```bash
python eval_net.py \
  --weights {weights} \
  --task {task} \
  --annotations {dataset_path} \
  --images {dataset_images_path} \
  --config-file {config_path}
```

#### Supported Evaluation Tasks

The `--task` argument specifies the prediction head to be evaluated and can take one of the following values:

* `combineHO`: joint hand–object interaction detection
* `hand_obj`: hand–object detection without interaction classification
* `handside`: hand side classification (left / right)
* `contact`: hand–object contact state classification


## Acknowledgements

This code is heavily built upon the following repositories. We thank the authors for their great work:

- [Detectron2](https://github.com/facebookresearch/detectron2)
- [Adaptive Teacher](https://github.com/facebookresearch/adaptive_teacher)
- [VISOR-HOS](https://github.com/epic-kitchens/VISOR-HOS)

Please adhere to the respective licenses of these repositories if you plan to use or redistribute this code.

