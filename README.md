# HOI-Synth: Domain Adaptation Branch

### Main Results

| Backbone          | Training Set          | Test Set  | Overall AP@50 | Download  |
| :---:             | :---:                 | :---:     | :---:         |  :---:    |
| Resnet 101 FPN   | HOI-Synth              | VISOR     | 33.33         | [model](https://iplab.dmi.unict.it/sharing2/HOI-Synth/checkpoints/da_hos_resnet_101-FPN/S_hoisynth-T_visor/model.pth) \| [config](configs/da_hos_resnet_101-FPN/VISOR/S_hoisynth-T_visor.yaml) |
| Resnet 101 FPN   | HOI-Synth + VISOR 10%  | VISOR     | 44.22         | [model]() \| [config](configs/da_hos_resnet_101-FPN/VISOR/S_hoisynth+visor10-T_visor.yaml) |
| Resnet 101 FPN   | HOI-Synth + VISOR 25%  | VISOR     | 45.55         | [model]() \| [config](configs/da_hos_resnet_101-FPN/VISOR/S_hoisynth+visor25-T_visor.yaml) |
| Resnet 101 FPN   | HOI-Synth + VISOR 50%  | VISOR     | 46.47         | [model]() \| [config](configs/da_hos_resnet_101-FPN/VISOR/S_hoisynth+visor50-T_visor.yaml) |
| Resnet 101 FPN   | HOI-Synth + VISOR 100% | VISOR     | 46.48         | [model]() \| [config](configs/da_hos_resnet_101-FPN/VISOR/S_hoisynth+visor100-T_visor.yaml) |


## Acknowledgements

This code is heavily built upon the following repositories. We thank the authors for their great work:

- [Detectron2](https://github.com/facebookresearch/detectron2)
- [Adaptive Teacher](https://github.com/facebookresearch/adaptive_teacher)
- [VISOR-HOS](https://github.com/epic-kitchens/VISOR-HOS)

Please adhere to the respective licenses of these repositories if you plan to use or redistribute this code.