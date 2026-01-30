# Data Generation Pipeline

This repository contains the Data Generation Pipeline used in: 
  - "Are Synthetic Data Useful for Egocentric Hand-Object Interaction Detection?" (ECCV 2024).
  - "Leveraging Synthetic Data for Enhancing Egocentric Hand-Object Interaction Detection" (extended journal version under review).

The pipeline is built on [Unity3D](https://unity.com/) and leverages the [Unity Perception](https://docs.unity3d.com/Packages/com.unity.perception@1.0/manual/index.html) package to generate large-scale, photorealistic synthetic datasets for Egocentric Human-Object Interaction (HOI) detection. It integrates assets from [HM3D](https://github.com/matterport/habitat-matterport-3dresearch) (environments) and [DexGraspNet](https://pku-epic.github.io/DexGraspNet/) (hand-object grasps) to create diverse interaction scenarios.

## Installation

### Prerequisites

* **Unity Hub** and **Unity Editor** (Recommended version: `2022.3.25f1`).
* [**FinalIK**](https://assetstore.unity.com/packages/tools/animation/final-ik-14290?srsltid=AfmBOoqJpJtSds7fI2v2WBtUg4okV9iXErRVX2MkkIkLO5vFHMJUhQKJ)

### Steps

1. **Clone the repository:**
```bash
git clone -b data_generation_pipeline https://github.com/fpv-iplab/HOI-Synth.git
```

2. **Open the Project:**
    * Add the project to Unity Hub.
    * Open it using the Unity Editor. Wait for the packages (Perception, etc.) to install automatically.

3. **Data Setup (Important):**
    * **DexGraspNet:** Download the hand-object assets and place them in `Assets/Data/DexGraspNet` (or follow the specific path in your project).
    * **HM3D Environments:** Due to licensing and size, HM3D assets must be downloaded separately. Import the GLB/FBX files into `Assets/Prefabs/Environments`.

## Quick Start
We provide a sample environment and a set of test objects directly included in the repository. This allows you to run and verify the pipeline immediately without downloading the full external datasets.

1. Open the main simulation scene: `Assets/Scenes/main.unity`.
2. Press the **Play** button in the Unity Editor.
    * The simulation will start loading environments, placing characters, and capturing frames.
    * Data (images, JSON annotations, semantic masks) will be generated in the `Perception/Output/` directory by default.

**Note**: For full-scale generation, you will need to download and import the complete HM3D and DexGraspNet datasets as described in the Data Setup section.

## License
This project is licensed under the [Creative Commons Attribution-NonCommercial 4.0 International (CC BY-NC 4.0)](https://creativecommons.org/licenses/by-nc/4.0/).

## Ackowledgements
This research has been supported by the project Future Artificial Intelligence Research (FAIR) – PNRR MUR Cod. PE0000013 - CUP: E63C22001940006 <br>
This research has been partially supported by the project EXTRA-EYE - PRIN 2022 - CUP E53D23008280006 - Finanziato dall’Unione Europea - Next Generation EU 
