# HOI-Synth: Domain Adaptation for Hand-Object Interaction

This repository contains the code, pretrained models, and configuration files for:

1. **"Are Synthetic Data Useful for Egocentric Hand-Object Interaction Detection?"**  
   *ECCV 2024*. [[Paper]](https://www.ecva.net/papers/eccv_2024/papers_ECCV/papers/08953.pdf) [[arXiv]](https://arxiv.org/abs/2312.02672)

2. **"Leveraging Synthetic Data for Enhancing Egocentric Hand-Object Interaction Detection"**  
   *Extended journal version, under review*.

## Installation

### Prerequisites
The code is tested on **Ubuntu 24.04** with **Python 3.10** and **CUDA 12.x**.

### 1. Clone the repository
```bash
git clone -b baseline-code [https://github.com/fpv-iplab/HOI-Synth.git](https://github.com/fpv-iplab/HOI-Synth.git)
cd HOI-Synth

```

### 2. Environment Setup

We provide an `environment.yml` file to automatically install all dependencies, including PyTorch and Detectron2.

```bash
# 1. Create the environment
conda env create -f environment.yml

# 2. Activate the environment
conda activate hoisynth

```

### 3. Verify Installation

To ensure Detectron2 and PyTorch are correctly communicating with your GPU:

```bash
python -c "import torch; print(f'Torch: {torch.__version__}, CUDA: {torch.cuda.is_available()}')"
# Should print True for CUDA

```

## Model Zoo

We provide pre-trained models for different datasets and training settings. All models are trained using the configurations released in this repository and can be directly used for evaluation or fine-tuning.

### Downloading Pretrained Weights

Download the pretrained models:
- [**ResNet-101-FPN**](https://iplab.dmi.unict.it/sharing2/HOI-Synth/weights/model_final_3f4d2a_at.pkl)
- [**ConvNeXt-Small**](https://iplab.dmi.unict.it/sharing2/HOI-Synth/weights/convnext_small.pkl)

After downloading, place the files in the `weights/` directory.

### VISOR

| Setting |    Backbone    |      Training Set      | Overall AP@50 |                                                                                                        Download                                                                                                        |
| :-----: | :------------: | :--------------------: | :-----------: | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------: |
|   UDA   | ResNet-101-FPN |       HOI-Synth        |     33.33     |          [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/VISOR/S_hoisynth-T_visor/model.pth) \| [config](configs/da_hos_resnet_101-FPN/VISOR/S_hoisynth-T_visor.yaml)          |
|  SSDA   | ResNet-101-FPN | HOI-Synth + VISOR 10%  |     44.22     |  [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/VISOR/S_hoisynth+visor10-T_visor/model.pth) \| [config](configs/da_hos_resnet_101-FPN/VISOR/S_hoisynth+visor10-T_visor.yaml)  |
|  SSDA   | ResNet-101-FPN | HOI-Synth + VISOR 25%  |     45.55     |  [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/VISOR/S_hoisynth+visor25-T_visor/model.pth) \| [config](configs/da_hos_resnet_101-FPN/VISOR/S_hoisynth+visor25-T_visor.yaml)  |
|  SSDA   | ResNet-101-FPN | HOI-Synth + VISOR 50%  |     46.47     |  [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/VISOR/S_hoisynth+visor50-T_visor/model.pth) \| [config](configs/da_hos_resnet_101-FPN/VISOR/S_hoisynth+visor50-T_visor.yaml)  |
|  FSDA   | ResNet-101-FPN | HOI-Synth + VISOR 100% |     46.48     | [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/VISOR/S_hoisynth+visor100-T_visor/model.pth) \| [config](configs/da_hos_resnet_101-FPN/VISOR/S_hoisynth+visor100-T_visor.yaml) |

### VISOR Aligned Sets

| Setting |    Backbone    |      Training Set      | Overall AP@50 |                                                                                                        Download                                                                                                        |
| :-----: | :------------: | :--------------------: | :-----------: | :-------------------------------------------------------
|  UDA   | ResNet-101-FPN | HOI-Synth Aligned Environments (5k)  |     31.59     |  [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/VISOR/aligned_sets/S_hoisynth_aligned_environments-T_visor/model.pth) \| [config](configs/da_hos_resnet_101-FPN/VISOR/S_hoisynth_aligned_environments-T_visor.yaml)  |
|  UDA   | ResNet-101-FPN | HOI-Synth Aligned Objects (5k) |     32.60     |  [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/VISOR/aligned_sets/S_hoisynth_aligned_objects-T_visor/model.pth) \| [config](configs/da_hos_resnet_101-FPN/VISOR/S_hoisynth_aligned_objects-T_visor.yaml)  |
|  UDA   | ResNet-101-FPN | HOI-Synth Aligned Grasps (5k) |     31.84     |  [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/VISOR/aligned_sets/S_hoisynth_aligned_grasps-T_visor/model.pth) \| [config](configs/da_hos_resnet_101-FPN/VISOR/S_hoisynth_aligned_grasps-T_visor.yaml)  |
|  UDA   | ResNet-101-FPN | HOI-Synth Aligned All |     33.98     | [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/VISOR/aligned_sets/S_hoisynth_aligned_all-T_visor/model.pth) \| [config](configs/da_hos_resnet_101-FPN/VISOR/S_hoisynth_aligned_all-T_visor.yaml) |

### VISOR – ConvNeXt

To replicate the experiments with the ConvNeXt backbone, clone the ConvNeXt implementation inside the `projects` directory:

```bash
cd projects
git clone --branch detectron2-port https://github.com/shivamsnaik/ConvNeXt.git
```

Then, follow the installation instructions provided in the cloned repository to install the required dependencies.

| Setting |    Backbone    |      Training Set      | Overall AP@50 |                                                                                                        Download 
| :-----: | :------------: | :--------------------: | :-----------: | :-------------------------------------------------------
|  FSDA   | ConvNeXt-S | HOI-Synth + VISOR 100%   |     XX.XX    |  [model]() \| [config](configs/da_hos_ConvNeXt/VISOR/ConvNeXt_S_hoisynth+visor100-T_visor.yaml)  |
|

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
|   UDA   | ResNet-101-FPN |       HOI-Synth          |     X       |     06.87     |          [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/ENIGMA-51/S_hoisynth_outdomain-T_enigma/model.pth) \| [config](configs/da_hos_resnet_101-FPN/ENIGMA-51/S_hoisynth_outdomain-T_enigma.yaml)          |
|   UDA   | ResNet-101-FPN |       HOI-Synth          |     V       |     34.78     |          [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/ENIGMA-51/S_hoisynth_indomain-T_enigma/model.pth) \| [config](configs/da_hos_resnet_101-FPN/ENIGMA-51/S_hoisynth_indomain-T_enigma.yaml)          |
|  SSDA   | ResNet-101-FPN | HOI-Synth + ENIGMA-51 10% |     X       |     57.08     |  [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/ENIGMA-51/S_hoisynth_outdomain+enigma10-T_enigma/model.pth) \| [config](configs/da_hos_resnet_101-FPN/ENIGMA-51/S_hoisynth_outdomain+enigma10-T_enigma.yaml)  |
|  SSDA   | ResNet-101-FPN | HOI-Synth + ENIGMA-51 10% |     V       |     56.69     |  [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/ENIGMA-51/S_hoisynth_indomain+enigma10-T_enigma/model.pth) \| [config](configs/da_hos_resnet_101-FPN/ENIGMA-51/S_hoisynth_indomain+enigma10-T_enigma.yaml)  |
|  SSDA   | ResNet-101-FPN | HOI-Synth + ENIGMA-51 25% |     X       |     58.17     |  [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/ENIGMA-51/S_hoisynth_outdomain+enigma25-T_enigma/model.pth) \| [config](configs/da_hos_resnet_101-FPN/ENIGMA-51/S_hoisynth_outdomain+enigma25-T_enigma.yaml)  |
|  SSDA   | ResNet-101-FPN | HOI-Synth + ENIGMA-51 25% |     V       |     59.48     |  [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/ENIGMA-51/S_hoisynth_indomain+enigma25-T_enigma/model.pth) \| [config](configs/da_hos_resnet_101-FPN/ENIGMA-51/S_hoisynth_indomain+enigma25-T_enigma.yaml)  |
|  SSDA   | ResNet-101-FPN | HOI-Synth + ENIGMA-51 50% |     X       |     63.25     |  [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/ENIGMA-51/S_hoisynth_outdomain+enigma50-T_enigma/model.pth) \| [config](configs/da_hos_resnet_101-FPN/ENIGMA-51/S_hoisynth_outdomain+enigma50-T_enigma.yaml)  |
|  SSDA   | ResNet-101-FPN | HOI-Synth + ENIGMA-51 50% |     V       |     61.93     |  [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/ENIGMA-51/S_hoisynth_indomain+enigma50-T_enigma/model.pth) \| [config](configs/da_hos_resnet_101-FPN/ENIGMA-51/S_hoisynth_indomain+enigma50-T_enigma.yaml) |
|  FSDA   | ResNet-101-FPN | HOI-Synth + ENIGMA-51 100% |     X       |     64.41     | [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/ENIGMA-51/S_hoisynth_outdomain+enigma100-T_enigma/model.pth) \| [config](configs/da_hos_resnet_101-FPN/ENIGMA-51/S_hoisynth_outdomain+enigma100-T_enigma.yaml) |
|  FSDA   | ResNet-101-FPN | HOI-Synth + ENIGMA-51 100% |     V       |     64.20     | [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/ENIGMA-51/S_hoisynth_indomain+enigma100-T_enigma/model.pth) \| [config](configs/da_hos_resnet_101-FPN/ENIGMA-51/S_hoisynth_indomain+enigma100-T_enigma.yaml) |


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

## Acknowledgements

This code is heavily built upon the following repositories. We thank the authors for their great work:

- [Detectron2](https://github.com/facebookresearch/detectron2)
- [Adaptive Teacher](https://github.com/facebookresearch/adaptive_teacher)
- [VISOR-HOS](https://github.com/epic-kitchens/VISOR-HOS)

Please adhere to the respective licenses of these repositories if you plan to use or redistribute this code.