# HOI-Synth: Domain Adaptation for Hand-Object Interaction

This repository contains the code, pretrained models, and configuration files for:

1. **"Are Synthetic Data Useful for Egocentric Hand-Object Interaction Detection?"**  
   *ECCV 2024*. [[Paper]](https://www.ecva.net/papers/eccv_2024/papers_ECCV/papers/08953.pdf) [[arXiv]](https://arxiv.org/abs/2312.02672)

2. **"Leveraging Synthetic Data for Enhancing Egocentric Hand-Object Interaction Detection"**  
   *Extended journal version, under review*.

## Installation

## Model Zoo

We provide pre-trained models for different datasets and training settings. All models are trained using the configurations released in this repository and can be directly used for evaluation or fine-tuning.

### VISOR

| Setting |    Backbone    |      Training Set      | Overall AP@50 |                                                                                                        Download                                                                                                        |
| :-----: | :------------: | :--------------------: | :-----------: | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------: |
|   UDA   | ResNet-101-FPN |       HOI-Synth        |     33.33     |          [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/VISOR/S_hoisynth-T_visor/model.pth) \| [config](configs/da_hos_resnet_101-FPN/VISOR/S_hoisynth-T_visor.yaml)          |
|  SSDA   | ResNet-101-FPN | HOI-Synth + VISOR 10%  |     44.22     |  [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/VISOR/S_hoisynth+visor10-T_visor/model.pth) \| [config](configs/da_hos_resnet_101-FPN/VISOR/S_hoisynth+visor10-T_visor.yaml)  |
|  SSDA   | ResNet-101-FPN | HOI-Synth + VISOR 25%  |     45.55     |  [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/VISOR/S_hoisynth+visor25-T_visor/model.pth) \| [config](configs/da_hos_resnet_101-FPN/VISOR/S_hoisynth+visor25-T_visor.yaml)  |
|  SSDA   | ResNet-101-FPN | HOI-Synth + VISOR 50%  |     46.47     |  [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/VISOR/S_hoisynth+visor50-T_visor/model.pth) \| [config](configs/da_hos_resnet_101-FPN/VISOR/S_hoisynth+visor50-T_visor.yaml)  |
|  FSDA   | ResNet-101-FPN | HOI-Synth + VISOR 100% |     46.48     | [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/VISOR/S_hoisynth+visor100-T_visor/model.pth) \| [config](configs/da_hos_resnet_101-FPN/VISOR/S_hoisynth+visor100-T_visor.yaml) |

### EgoHOS
| Setting |    Backbone    |      Training Set      | Overall AP@50 |                                                                                                        Download                                                                                                        |
| :-----: | :------------: | :--------------------: | :-----------: | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------: |
|   UDA   | ResNet-101-FPN |       HOI-Synth        |     28.16     |          [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/EgoHOS/S_hoisynth-T_egohos/model.pth) \| [config](configs/da_hos_resnet_101-FPN/EgoHOS/S_hoisynth-T_egohos.yaml)          |
|  SSDA   | ResNet-101-FPN | HOI-Synth + EgoHOS 10%  |     36.68     |  [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/EgoHOS/S_hoisynth+egohos10-T_egohos/model.pth) \| [config](configs/da_hos_resnet_101-FPN/EgoHOS/S_hoisynth+egohos10-T_egohos.yaml)  |
|  SSDA   | ResNet-101-FPN | HOI-Synth + EgoHOS 25%  |     37.16     |  [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/EgoHOS/S_hoisynth+egohos25-T_egohos/model.pth) \| [config](configs/da_hos_resnet_101-FPN/EgoHOS/S_hoisynth+egohos25-T_egohos.yaml)  |
|  SSDA   | ResNet-101-FPN | HOI-Synth + EgoHOS 50%  |     39.85     |  [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/EgoHOS/S_hoisynth+egohos50-T_egohos/model.pth) \| [config](configs/da_hos_resnet_101-FPN/EgoHOS/S_hoisynth+egohos50-T_egohos.yaml)  |
|  FSDA   | ResNet-101-FPN | HOI-Synth + EgoHOS 100% |     39.61     | [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/EgoHOS/S_hoisynth+egohos100-T_egohos/model.pth) \| [config](configs/da_hos_resnet_101-FPN/EgoHOS/S_hoisynth+egohos100-T_egohos.yaml) |


### ENIGMA-51
| Setting |    Backbone    |      Training Set        | In-Domain   | Overall AP@50 |                                                                                                        Download                                                                                                        |
| :-----: | :------------: | :--------------------:   | :--------:  | :-----------: | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------: |
|   UDA   | ResNet-101-FPN |       HOI-Synth          |     X       |     28.16     |          [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/ENIGMA-51/S_hoisynth_outdomain-T_enigma/model.pth) \| [config](configs/da_hos_resnet_101-FPN/ENIGMA-51/S_hoisynth_outdomain-T_enigma.yaml)          |
|   UDA   | ResNet-101-FPN |       HOI-Synth          |     V       |     34.78     |          [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/ENIGMA-51/S_hoisynth_indomain-T_enigma/model.pth) \| [config](configs/da_hos_resnet_101-FPN/ENIGMA-51/S_hoisynth_indomain-T_enigma.yaml)          |
|  SSDA   | ResNet-101-FPN | HOI-Synth + ENIGMA-51 10% |     X       |     36.68     |  [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/ENIGMA-51/S_hoisynth_outdomain+enigma10-T_enigma/model.pth) \| [config](configs/da_hos_resnet_101-FPN/ENIGMA-51/S_hoisynth_outdomain+enigma10-T_enigma.yaml)  |
|  SSDA   | ResNet-101-FPN | HOI-Synth + ENIGMA-51 10% |     V       |     56.69     |  [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/ENIGMA-51/S_hoisynth_indomain+enigma10-T_enigma/model.pth) \| [config](configs/da_hos_resnet_101-FPN/ENIGMA-51/S_hoisynth_indomain+enigma10-T_enigma.yaml)  |
|  SSDA   | ResNet-101-FPN | HOI-Synth + ENIGMA-51 25% |     X       |     37.16     |  [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/ENIGMA-51/S_hoisynth_outdomain+enigma25-T_enigma/model.pth) \| [config](configs/da_hos_resnet_101-FPN/ENIGMA-51/S_hoisynth_outdomain+enigma25-T_enigma.yaml)  |
|  SSDA   | ResNet-101-FPN | HOI-Synth + ENIGMA-51 25% |     V       |     37.16     |  [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/ENIGMA-51/S_hoisynth_indomain+enigma25-T_enigma/model.pth) \| [config](configs/da_hos_resnet_101-FPN/ENIGMA-51/S_hoisynth_indomain+enigma25-T_enigma.yaml)  |
|  SSDA   | ResNet-101-FPN | HOI-Synth + ENIGMA-51 50% |     X       |     39.85     |  [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/ENIGMA-51/S_hoisynth_outdomain+enigma50-T_enigma/model.pth) \| [config](configs/da_hos_resnet_101-FPN/ENIGMA-51/S_hoisynth_outdomain+enigma50-T_enigma.yaml)  |
|  SSDA   | ResNet-101-FPN | HOI-Synth + ENIGMA-51 50% |     V       |     61.93     |  [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/ENIGMA-51/S_hoisynth_indomain+enigma50-T_enigma/model.pth) \| [config](configs/da_hos_resnet_101-FPN/ENIGMA-51/S_hoisynth_indomain+enigma50-T_enigma.yaml) |
|  FSDA   | ResNet-101-FPN | HOI-Synth + ENIGMA-51 100% |     X       |     39.61     | [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/ENIGMA-51/S_hoisynth_outdomain+enigma100-T_enigma/model.pth) \| [config](configs/da_hos_resnet_101-FPN/ENIGMA-51/S_hoisynth_outdomain+enigma100-T_enigma.yaml) |
|  FSDA   | ResNet-101-FPN | HOI-Synth + ENIGMA-51 100% |     V       |     39.61     | [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/ENIGMA-51/S_hoisynth_indomain+enigma100-T_enigma/model.pth) \| [config](configs/da_hos_resnet_101-FPN/ENIGMA-51/S_hoisynth_indomain+enigma100-T_enigma.yaml) |


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

- `combineHO`: joint hand–object interaction detection
- `hand_obj`: hand–object detection without interaction classification
- `handside`: hand side classification (left / right)
- `contact`: hand–object contact state classification

## Citation

## Acknowledgements

This code is heavily built upon the following repositories. We thank the authors for their great work:

- [Detectron2](https://github.com/facebookresearch/detectron2)
- [Adaptive Teacher](https://github.com/facebookresearch/adaptive_teacher)
- [VISOR-HOS](https://github.com/epic-kitchens/VISOR-HOS)

Please adhere to the respective licenses of these repositories if you plan to use or redistribute this code.
