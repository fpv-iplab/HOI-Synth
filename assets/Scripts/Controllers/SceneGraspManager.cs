using System;
using System.Collections.Generic;
using Defective.JSON;
using UnityEngine;
using UnityEngine.Perception.Randomization.Scenarios;

public class SceneGraspManager : SceneManager
{
    public int current_grasp_idx = 0;
    public int current_max_grasp_idx = -1;

    public int max_objects = 0;
    JSONObject jsonObj;
    ObjectRandomizer objectRandomizer;

    void Start()
    {
        customScenario = (CustomScenario)ScenarioBase.activeScenario;
        objectsContainer = customScenario.GetObjectsContainer();

        foreach (var randomizer in customScenario.randomizers)
        {
            if (!typeof(ObjectRandomizer).IsAssignableFrom(randomizer.GetType()))
                continue;
            objectRandomizer = (ObjectRandomizer)randomizer;
        }

        max_objects = objectRandomizer.prefabs_paths.Count;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (
            customScenario.AllCustomRandomizersReady()
            && customScenario.scenarioState == CustomScenario.ScenarioState.Idle
        )
        {
            try
            {
                // Get list of current objects in scene
                customScenario.currentActiveObjects = new List<GameObject>();
                var allObjects = Utils.GetFirstChilds(objectsContainer.transform);

                // Get the current human
                HandManager handManager = customScenario
                    .GetActiveHuman()
                    .GetComponent<HandManager>();
                handManager.RestPosition();
                handManager.RandomizeHandsPose(allObjects);

                if (current_max_grasp_idx == -1)
                {
                    jsonObj = new JSONObject(
                        allObjects[0].GetComponent<DexGraspAnnotation>().annotation.text
                    );
                    current_max_grasp_idx = jsonObj.count;
                }

                var dexGraspData = new DexGraspNet.DexGraspData(
                    jsonObj[current_grasp_idx],
                    "right"
                );
                handManager.ApplyHandPoseNoObject(dexGraspData, true);
                dexGraspData = new DexGraspNet.DexGraspData(jsonObj[current_grasp_idx], "left");
                handManager.ApplyHandPoseNoObject(dexGraspData, false);
                current_grasp_idx++;

                if (current_grasp_idx == current_max_grasp_idx)
                {
                    objectRandomizer.destroyAtEnd = true;
                    current_max_grasp_idx = -1;
                    current_grasp_idx = 0;

                    if (objectRandomizer.idx == max_objects - 1)
                        customScenario.scenarioComplete = true;
                }

                customScenario.StartCapture();
            }
            catch (Exception e)
            {
                Debug.Log($"Error. Skipped: {e.ToString()}");
                customScenario.NextIteraction();
            }
        }
    }

    public override void Reset() { }
}
