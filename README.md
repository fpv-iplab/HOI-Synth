# Are Synthetic Data Useful for Egocentric Hand-Object Interaction Detection?
![](assets/images/hoi_synth_pipeline.gif)

## Overview
We investigate the effectiveness of synthetic data in enhancing egocentric hand-object interaction detection. Via extensive experiments and comparative analyses on three egocentric datasets, VISOR , EgoHOS, and ENIGMA-51, our findings reveal how to exploit synthetic data for the HOI detection task when real labeled data are scarce or unavailable. Specifically, by leveraging only 10% of real labeled data, we achieve improvements in Overall AP compared to baselines trained exclusively on real data of: +5.67% on EPIC-KITCHENS VISOR, +8.24% on EgoHOS, and +11.69% on ENIGMA-51. Our analysis is supported by a novel data generation pipeline and the newly introduced HOI-Synth benchmark which augments existing datasets with synthetic images of hand-object interactions automatically labeled with hand-object contact states, bounding boxes, and pixel-wise segmentation masks.

[Project Page](https://fpv-iplab.github.io/HOI-Synth/) - [Paper](https://arxiv.org/pdf/2312.02672)

## Updates
* 01/07/2024: **Accepted at European Conference on Computer Vision (ECCV) 2024!** <br>

## Citation
If you use our HOI-Synth benchmark, data generation pipeline or this code for your research, please cite our paper:
```
@inproceedings{leonardi2025synthetic,
  title={Are Synthetic Data Useful for Egocentric Hand-Object Interaction Detection?},
  author={Leonardi, Rosario and Furnari, Antonino and Ragusa, Francesco and Farinella, Giovanni Maria},
  booktitle={European Conference on Computer Vision},
  pages={36--54},
  year={2025},
  organization={Springer}
}
```

## Table of Contents
1. [HOI-Synth benchmark](#hoi-synth-benchmark)
2. [Data Generation Pipeline](#data-generation-pipeline)
3. [Baselines](#baselines)
4. [License](#license)
5. [Ackowledgements](#ackowledgements)<br>


## HOI-Synth benchmark
The HOI-Synth benchmark extends three egocentric datasets designed to study hand-object interaction detection, EPIC-KITCHENS VISOR [1], EgoHOS [2], and ENIGMA-51 [3], with automatically labeled synthetic data obtained through the proposed HOI generation pipeline.

### Download

#### Synthetic-Data
You can download the synthetic data at the following links:

* [Annotations](https://iplab.dmi.unict.it/sharing/hoi-synth/annotations.zip)
* [Images](https://iplab.dmi.unict.it/sharing/hoi-synth/images.zip)

The format follows the standard of HOS introduced in the [VISOR-HOS GitHub repository](https://github.com/epic-kitchens/VISOR-HOS?tab=readme-ov-file). Please refer to that link for more information.

After downloading, place the images and annotations in their respective folders.

You will find several annotation files available:

- `train.json`: Contains the complete train annotations.
- `val.json`: Contains the complete val annotations.
- `train_x.json`: Contains annotations for specific percentages of data. For example, `train_10.json` contains annotations for 10% of the data.

Additionally, you will find combined annotations (e.g., Synthetic + VISOR). In such cases, move the images from the corresponding real dataset into the appropriate "images" folder.

For the Enigma-51 synthetic images (`enigma-51_synth`), there are three folders containing the different synthetic data used in the experiments (Check the paper for more information):

- **In-domain**
- **Out-domain**
- **Out-domain with FOV of the target dataset**

---

#### EPIC-KITCHENS VISOR

To download the data and the corresponding annotations for EPIC-KITCHENS VISOR, follow this link: [EPIC-KITCHENS VISOR Data Preparation](https://github.com/epic-kitchens/VISOR-HOS?tab=readme-ov-file#data-preparation).

---

#### EgoHOS
To download the images of EgoHOS, follow this link: [EgoHOS](https://github.com/owenzlz/EgoHOS).

We have converted the annotations into the HOS format, which can be downloaded at the following link: [EgoHOS Annotations](https://iplab.dmi.unict.it/sharing/hoi-synth/annotations_egohos.zip).

---

#### ENIGMA-51
You can download the ENIGMA-51 data at the following links:

* [Annotations](https://iplab.dmi.unict.it/sharing/hoi-synth/annotations_enigma_51.zip)
* [Images](https://iplab.dmi.unict.it/sharing/hoi-synth/images_enigma_51.zip)

For more information, visit the official [ENIGMA-51](https://iplab.dmi.unict.it/ENIGMA-51/) website.


## Data Generation Pipeline
Coming soon!

## Baselines 
Coming soon!

## License
This project is licensed under the [Creative Commons Attribution-NonCommercial 4.0 International (CC BY-NC 4.0)](https://creativecommons.org/licenses/by-nc/4.0/).


## Ackowledgements
This research has been supported by the project Future Artificial Intelligence Research (FAIR) – PNRR MUR Cod. PE0000013 - CUP: E63C22001940006 <br>
This research has been partially supported by the project EXTRA-EYE - PRIN 2022 - CUP E53D23008280006 - Finanziato dall’Unione Europea - Next Generation EU 

## References

* [1] Darkhalil, A., Shan, D., Zhu, B., Ma, J., Kar, A., Higgins, R., Fidler, S., Fouhey, D., Damen, D.: Epic-kitchens visor benchmark: Video segmentations and object relations. In: NeurIPS. pp. 13745–13758 (2022)
* [2] Zhang, L., Zhou, S., Stent, S., Shi, J.: Fine-grained egocentric hand-object segmentation: Dataset, model, and applications. In: ECCV. pp. 127–145 (2022)
* [3] Ragusa, F., Leonardi, R., Mazzamuto, M., Bonanno, C., Scavo, R., Furnari, A., Farinella, G. M.: ENIGMA-51: Towards a Fine-Grained Understanding of Human Behavior in Industrial Scenarios. In Proceedings of the IEEE/CVF Winter Conference on Applications of Computer Vision (pp. 4549-4559) (2024)
* [4] Wang, R., Zhang, J., Chen, J., Xu, Y., Li, P., Liu, T., Wang, H.: Dexgraspnet: A large-scale robotic dexterous grasp dataset for general objects based on simulation. In: CVPR. pp. 11359–11366 (2023)
