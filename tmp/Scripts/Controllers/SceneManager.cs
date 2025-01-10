using System;
using System.Collections.Generic;
using System.Linq;
using Defective.JSON;
using MyBox;
using UnityEngine;
using UnityEngine.Perception.GroundTruth;
using UnityEngine.Perception.GroundTruth.DataModel;
using UnityEngine.Perception.GroundTruth.LabelManagement;
using UnityEngine.Perception.Randomization.Samplers;
using UnityEngine.Perception.Randomization.Scenarios;

public class SceneManager : MonoBehaviour
{
    public bool randomGrasp = false,
        grasps_from_file = false;

    [ConditionalField(nameof(grasps_from_file))]
    public TextAsset fileGrasps;

    JSONObject jsonGrasps = null;

    public Vector2 placementAreaOtherObjects;

    [HideInInspector]
    public bool interactionInThisFrame,
        twoInteractions;

    //private
    [HideInInspector]
    public CustomScenario customScenario;

    [HideInInspector]
    public GameObject objectsContainer;

    [HideInInspector]
    public UniformSampler handSampler,
        interactionSampler,
        otherObjectsSampler,
        twoInteractionsSampler;

    [HideInInspector]
    public List<InteractionAnnotation> interactionAnnotations;

    [HideInInspector]
    public MetricDefinition interactionMetricDefinition;

    public bool debug = false;

    [ConditionalField(nameof(debug))]
    public bool skip_grasp = false;

    void Start()
    {
        customScenario = (CustomScenario)ScenarioBase.activeScenario;
        objectsContainer = customScenario.GetObjectsContainer();
        handSampler = new UniformSampler { range = new FloatRange(0, 1) };
        interactionSampler = new UniformSampler { range = new FloatRange(0, 1) };
        otherObjectsSampler = new UniformSampler { range = new FloatRange(0, 1) };
        twoInteractionsSampler = new UniformSampler { range = new FloatRange(0, 1) };

        if (grasps_from_file)
            jsonGrasps = new JSONObject(fileGrasps.text);

        interactionMetricDefinition = new MetricDefinition(
            "Interaction",
            "interaction",
            "Interactions occurred in this frame. First element is the instance_id of hand, while second is the instance id of the active object"
        );

        DatasetCapture.RegisterMetric(interactionMetricDefinition);

        interactionAnnotations = new List<InteractionAnnotation>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (
            customScenario.AllCustomRandomizersReady()
            && customScenario.scenarioState == CustomScenario.ScenarioState.Idle
        )
        {
            if (skip_grasp)
            {
                customScenario.StartCapture();
            }
            else
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

                    interactionInThisFrame =
                        interactionSampler.Sample() < customScenario.constants.probInteraction;
                    twoInteractions =
                        twoInteractionsSampler.Sample()
                            < customScenario.constants.probTwoInteractions
                        && interactionInThisFrame;

                    if (interactionInThisFrame)
                    {
                        var objectToTouch = allObjects[0];
                        var handRight = Mathf.RoundToInt(handSampler.Sample()) == 1;

                        handManager.GraspDexObject(
                            objectToTouch: objectToTouch,
                            handRight: handRight,
                            randomize_grasp_idx: true,
                            jsonGrasps: grasps_from_file ? jsonGrasps[objectToTouch.name] : null
                        );

                        interactionAnnotations.Add(
                            new InteractionAnnotation(
                                handRight
                                    ? (int)
                                        handManager
                                            .rightHandLabeling.GetComponent<Labeling>()
                                            .instanceId
                                    : (int)
                                        handManager
                                            .leftHandLabeling.GetComponent<Labeling>()
                                            .instanceId,
                                (int)objectToTouch.GetComponent<Labeling>().instanceId
                            )
                        );

                        if (twoInteractions)
                        {
                            objectToTouch = allObjects[1];

                            handManager.GraspDexObject(
                                objectToTouch: objectToTouch,
                                handRight: !handRight,
                                randomize_grasp_idx: true,
                                jsonGrasps: grasps_from_file ? jsonGrasps[objectToTouch.name] : null
                            );

                            interactionAnnotations.Add(
                                new InteractionAnnotation(
                                    !handRight
                                        ? (int)
                                            handManager
                                                .rightHandLabeling.GetComponent<Labeling>()
                                                .instanceId
                                        : (int)
                                            handManager
                                                .leftHandLabeling.GetComponent<Labeling>()
                                                .instanceId,
                                    (int)objectToTouch.GetComponent<Labeling>().instanceId
                                )
                            );
                        }

                        if (randomGrasp)
                            handManager.RandomizeHandsPose();
                    }

                    if (otherObjectsSampler.Sample() < customScenario.constants.probOtherObjects)
                    {
                        var index_skip = 0;
                        if (interactionInThisFrame)
                            index_skip = 1;
                        if (twoInteractions)
                            index_skip = 2;

                        foreach (var obj in allObjects.Skip(index_skip))
                            handManager.ApplyRandomPose(obj);
                        Utils.RandomizeObjectsPosition(
                            customScenario.GetActiveHuman(),
                            handManager.wristR,
                            allObjects.Skip(index_skip).ToList<GameObject>(),
                            placementAreaOtherObjects
                        );
                    }

                    var metric = new InteractionMetric(
                        new List<InteractionAnnotation>(interactionAnnotations),
                        interactionMetricDefinition
                    );

                    DatasetCapture.ReportMetric(interactionMetricDefinition, metric);

                    customScenario.StartCapture();
                }
                catch (Exception e)
                {
                    Debug.Log($"Error. Skipped: {e.ToString()}");
                    customScenario.NextIteraction();
                }
            }
        }
    }

    public virtual void Reset()
    {
        interactionAnnotations.Clear();
    }
}
