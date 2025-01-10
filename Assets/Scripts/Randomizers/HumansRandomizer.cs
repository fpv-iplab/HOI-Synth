using System;
using System.Collections.Generic;
using RootMotion.FinalIK;
using UnityEngine;
using UnityEngine.Perception.GroundTruth;
using UnityEngine.Perception.GroundTruth.LabelManagement;
using UnityEngine.Perception.Randomization.Randomizers;

[Serializable]
[AddRandomizerMenu("CustomHumansRandomizer")]
public class CustomNavMeshPlacementRandomizerTag : RandomizerTag { }

public class HumansRandomizer : Randomizer, ICustomRandomizer
{
    //CustomCameraController cameraController;
    CustomCameraController[] cameraControllers;

    public bool justHand = true;

    public Material trasparentMat;

    GameObject h_Container;

    public GameObject activeHuman;

    GameObject leftHandTarget,
        rightHandTarget,
        handTargetsContainer;

    public bool ready { get; set; }

    public bool rigg_human = true;

    public bool resetHumanIt = true;

    protected override void OnAwake()
    {
        h_Container = new GameObject("HandsLabelingContainer");
        handTargetsContainer = new GameObject("HandTargetsContainer");
        rightHandTarget = new GameObject("RightHandTarget");
        rightHandTarget.transform.SetParent(handTargetsContainer.transform);
        leftHandTarget = new GameObject("LeftHandTarget");
        leftHandTarget.transform.SetParent(handTargetsContainer.transform);
    }

    protected override void OnScenarioStart()
    {
        base.OnScenarioStart();
        cameraControllers = UnityEngine.Object.FindObjectsByType<CustomCameraController>(
            FindObjectsSortMode.None
        );
    }

    protected override void OnIterationStart()
    {
        if (activeHuman != null)
        {
            ready = true;
            return;
        }

        foreach (Transform child in h_Container.transform)
            UnityEngine.Object.Destroy(child.gameObject);

        if (
            UnityEngine.Object.FindObjectsByType<CustomHumanTag>(FindObjectsSortMode.None).Length
            == 1
        )
        {
            activeHuman = GameObject
                .FindObjectsByType<CustomHumanTag>(FindObjectsSortMode.None)[0]
                .gameObject;

            FullBodyBipedIK fbik;
            if (rigg_human && activeHuman.gameObject.GetComponent<FullBodyBipedIK>() == null)
                fbik = activeHuman.gameObject.AddComponent<FullBodyBipedIK>();
            else
                fbik = activeHuman.gameObject.GetComponent<FullBodyBipedIK>();

            RiggingHuman.AutoRiggingModel(fbik);

            if (activeHuman.GetComponent<CustomNavMeshPlacementRandomizerTag>() == null)
                activeHuman.AddComponent<CustomNavMeshPlacementRandomizerTag>();
            activeHuman.layer = 6;
            foreach (Transform g in activeHuman.transform.GetComponentsInChildren<Transform>())
                g.gameObject.layer = 6;

            if (activeHuman.GetComponent<HandManager>() == null)
                activeHuman.AddComponent<HandManager>();

            HandManager handManager = activeHuman.GetComponent<HandManager>();
            handManager.FindFingers();
            handManager.SetHandTargets(leftHandTarget, rightHandTarget);

            if (justHand)
            {
                CreateHandCopyLabeling(activeHuman, true);
                CreateHandCopyLabeling(activeHuman, false);
            }
            else
            {
                CreateArmCopyLabeling(activeHuman, true);
                CreateArmCopyLabeling(activeHuman, false);
            }

            foreach (CustomCameraController cameraController in cameraControllers)
                cameraController.Head = activeHuman.GetComponent<FullBodyBipedIK>().references.head;

            ready = true;
        }
    }

    public GameObject GetActiveHuman()
    {
        return activeHuman;
    }

    private void CreateArmCopyLabeling(GameObject human, bool isRightHand)
    {
        var human_copy = GameObject.Instantiate(human);
        human_copy.name = isRightHand ? "Hand_R_Labeling" : "Hand_L_Labeling";
        human_copy.transform.parent = h_Container.transform;

        var arm = isRightHand
            ? human_copy.GetComponent<FullBodyBipedIK>().references.rightUpperArm.gameObject
            : human_copy.GetComponent<FullBodyBipedIK>().references.leftUpperArm.gameObject;
        var root = human_copy.transform.GetChild(0).gameObject;

        foreach (var comp in human_copy.GetComponents<Component>())
        {
            if ((comp is Transform) || (comp is SkinnedMeshRenderer))
                continue;
            UnityEngine.Object.Destroy(comp);
        }

        arm.transform.parent = human_copy.transform;
        root.transform.parent = arm.transform;
        root.transform.localScale = new Vector3(0.000001f, 0.000001f, 0.000001f);
        root.transform.localPosition = Vector3.zero;

        CopyTransformHand copyTransformHand = human_copy.AddComponent<CopyTransformHand>();
        copyTransformHand.SetHandTarget(
            isRightHand
                ? human.GetComponent<FullBodyBipedIK>().references.rightUpperArm.gameObject
                : human.GetComponent<FullBodyBipedIK>().references.leftUpperArm.gameObject
        );

        Labeling labeling = human_copy.AddComponent<Labeling>();
        labeling.labels.Add(isRightHand ? "Hand_R" : "Hand_L");
        human_copy.layer = 7;
        foreach (Transform g in human_copy.transform.GetComponentsInChildren<Transform>())
            g.gameObject.layer = 7;

        var mats = human_copy.GetComponent<SkinnedMeshRenderer>().materials;
        for (int i = 0; i < mats.Length; i++)
            mats[i] = trasparentMat;
        human_copy.GetComponent<SkinnedMeshRenderer>().materials = mats;

        foreach (
            var skinnedMeshRenderer in human_copy.GetComponentsInChildren<SkinnedMeshRenderer>()
        )
        {
            mats = skinnedMeshRenderer.materials;
            for (int i = 0; i < mats.Length; i++)
                mats[i] = trasparentMat;
            skinnedMeshRenderer.materials = mats;
        }

        foreach (var jointLabel in human_copy.GetComponentsInChildren<JointLabel>())
        {
            jointLabel.labels = new List<string>();
            jointLabel.labels.Add(jointLabel.gameObject.name);
        }

        HandManager handManager = human.GetComponent<HandManager>();
        if (isRightHand)
            handManager.rightHandLabeling = human_copy;
        else
            handManager.leftHandLabeling = human_copy;
    }

    private void CreateHandCopyLabeling(GameObject human, bool isRightHand)
    {
        var human_copy = GameObject.Instantiate(human);
        human_copy.name = isRightHand ? "Hand_R_Labeling" : "Hand_L_Labeling";
        human_copy.transform.parent = h_Container.transform;

        var hand = isRightHand
            ? human_copy.GetComponent<FullBodyBipedIK>().references.rightHand.gameObject
            : human_copy.GetComponent<FullBodyBipedIK>().references.leftHand.gameObject;
        var root = human_copy.transform.GetChild(0).gameObject;

        foreach (var comp in human_copy.GetComponents<Component>())
        {
            if ((comp is Transform) || (comp is SkinnedMeshRenderer))
                continue;
            UnityEngine.Object.Destroy(comp);
        }

        hand.transform.parent = human_copy.transform;
        root.transform.parent = hand.transform;
        root.transform.localScale = new Vector3(0.000001f, 0.000001f, 0.000001f);
        root.transform.localPosition = Vector3.zero;

        CopyTransformHand copyTransformHand = human_copy.AddComponent<CopyTransformHand>();
        copyTransformHand.SetHandTarget(
            isRightHand
                ? human.GetComponent<FullBodyBipedIK>().references.rightHand.gameObject
                : human.GetComponent<FullBodyBipedIK>().references.leftHand.gameObject
        );

        Labeling labeling = human_copy.AddComponent<Labeling>();
        labeling.labels.Add(isRightHand ? "Hand_R" : "Hand_L");
        human_copy.layer = 7;
        foreach (Transform g in human_copy.transform.GetComponentsInChildren<Transform>())
            g.gameObject.layer = 7;

        var mats = human_copy.GetComponent<SkinnedMeshRenderer>().materials;
        for (int i = 0; i < mats.Length; i++)
            mats[i] = trasparentMat;
        human_copy.GetComponent<SkinnedMeshRenderer>().materials = mats;

        foreach (
            var skinnedMeshRenderer in human_copy.GetComponentsInChildren<SkinnedMeshRenderer>()
        )
        {
            mats = skinnedMeshRenderer.materials;
            for (int i = 0; i < mats.Length; i++)
                mats[i] = trasparentMat;
            skinnedMeshRenderer.materials = mats;
        }

        foreach (var jointLabel in human_copy.GetComponentsInChildren<JointLabel>())
        {
            jointLabel.labels = new List<string>();
            jointLabel.labels.Add(jointLabel.gameObject.name);
        }

        HandManager handManager = human.GetComponent<HandManager>();
        if (isRightHand)
            handManager.rightHandLabeling = human_copy;
        else
            handManager.leftHandLabeling = human_copy;
    }

    protected override void OnIterationEnd()
    {
        //foreach (Transform child in h_Container.transform)
        //    UnityEngine.Object.Destroy(child.gameObject);
        //UnityEngine.Object.Destroy(activeHuman);
        if (resetHumanIt)
            activeHuman = null;
        ready = false;
    }
}
