using System.Collections.Generic;
using Defective.JSON;
using RootMotion.FinalIK;
using UnityEngine;
using UnityEngine.Perception.Randomization.Parameters;
using UnityEngine.Perception.Randomization.Samplers;

public class HandManager : MonoBehaviour
{
    public StringGameObjectDictionary leftHand = new StringGameObjectDictionary(),
        rightHand = new StringGameObjectDictionary();

    public GameObject wristR,
        wristL;
    public GameObject leftHandTarget,
        rightHandTarget;

    [HideInInspector]
    public GameObject leftHandLabeling,
        rightHandLabeling;

#pragma warning disable CS0436 // Type conflicts with imported type
    [InspectorButton("FindFingers")]
#pragma warning restore CS0436 // Type conflicts with imported type
    public bool findFingers;

#pragma warning disable CS0436 // Type conflicts with imported type
    [InspectorButton("GraspDexObjectButton")]
#pragma warning restore CS0436 // Type conflicts with imported type
    public bool grasp;

    public GameObject interactedObject;

    public DexGraspNet.DexGraspData dexGraspData;

    public int index_grasp = 0;

    public bool handRightButton = true;

    FullBodyBipedIK fullBodyBipedIK;

    public Vector3Parameter handPositionParameter = new Vector3Parameter
    {
        x = new UniformSampler(0.2f, 0.5f),
        y = new UniformSampler(-0.15f, 0.2f),
        z = new UniformSampler(0.3f, 0.4f)
    };

    UniformSampler indexSampler = new UniformSampler();

    public void FindFingers()
    {
        //leftHand = new StringGameObjectDictionary();

        leftHand["index_01_left"] = Utils.RecursiveFindChild(transform, "index_01_left").gameObject;
        leftHand["index_02_left"] = Utils.RecursiveFindChild(transform, "index_02_left").gameObject;
        leftHand["index_03_left"] = Utils.RecursiveFindChild(transform, "index_03_left").gameObject;

        leftHand["middle_01_left"] = Utils
            .RecursiveFindChild(transform, "middle_01_left")
            .gameObject;
        leftHand["middle_02_left"] = Utils
            .RecursiveFindChild(transform, "middle_02_left")
            .gameObject;
        leftHand["middle_03_left"] = Utils
            .RecursiveFindChild(transform, "middle_03_left")
            .gameObject;

        leftHand["ring_01_left"] = Utils.RecursiveFindChild(transform, "ring_01_left").gameObject;
        leftHand["ring_02_left"] = Utils.RecursiveFindChild(transform, "ring_02_left").gameObject;
        leftHand["ring_03_left"] = Utils.RecursiveFindChild(transform, "ring_03_left").gameObject;

        leftHand["pinky_01_left"] = Utils.RecursiveFindChild(transform, "pinky_01_left").gameObject;
        leftHand["pinky_02_left"] = Utils.RecursiveFindChild(transform, "pinky_02_left").gameObject;
        leftHand["pinky_03_left"] = Utils.RecursiveFindChild(transform, "pinky_03_left").gameObject;

        leftHand["thumb_01_left"] = Utils.RecursiveFindChild(transform, "thumb_01_left").gameObject;
        leftHand["thumb_02_left"] = Utils.RecursiveFindChild(transform, "thumb_02_left").gameObject;
        leftHand["thumb_03_left"] = Utils.RecursiveFindChild(transform, "thumb_03_left").gameObject;

        wristL = Utils.RecursiveFindChild(transform, "wrist_left").gameObject;
        //leftHand["palm_left"] = Utils.RecursiveFindChild(transform, "palm_left").gameObject;

        //rightHand = new StringGameObjectDictionary();

        rightHand["index_01_right"] = Utils
            .RecursiveFindChild(transform, "index_01_right")
            .gameObject;
        rightHand["index_02_right"] = Utils
            .RecursiveFindChild(transform, "index_02_right")
            .gameObject;
        rightHand["index_03_right"] = Utils
            .RecursiveFindChild(transform, "index_03_right")
            .gameObject;

        rightHand["middle_01_right"] = Utils
            .RecursiveFindChild(transform, "middle_01_right")
            .gameObject;
        rightHand["middle_02_right"] = Utils
            .RecursiveFindChild(transform, "middle_02_right")
            .gameObject;
        rightHand["middle_03_right"] = Utils
            .RecursiveFindChild(transform, "middle_03_right")
            .gameObject;

        rightHand["ring_01_right"] = Utils
            .RecursiveFindChild(transform, "ring_01_right")
            .gameObject;
        rightHand["ring_02_right"] = Utils
            .RecursiveFindChild(transform, "ring_02_right")
            .gameObject;
        rightHand["ring_03_right"] = Utils
            .RecursiveFindChild(transform, "ring_03_right")
            .gameObject;

        rightHand["pinky_01_right"] = Utils
            .RecursiveFindChild(transform, "pinky_01_right")
            .gameObject;
        rightHand["pinky_02_right"] = Utils
            .RecursiveFindChild(transform, "pinky_02_right")
            .gameObject;
        rightHand["pinky_03_right"] = Utils
            .RecursiveFindChild(transform, "pinky_03_right")
            .gameObject;

        rightHand["thumb_01_right"] = Utils
            .RecursiveFindChild(transform, "thumb_01_right")
            .gameObject;
        rightHand["thumb_02_right"] = Utils
            .RecursiveFindChild(transform, "thumb_02_right")
            .gameObject;
        rightHand["thumb_03_right"] = Utils
            .RecursiveFindChild(transform, "thumb_03_right")
            .gameObject;

        wristR = Utils.RecursiveFindChild(transform, "wrist_right").gameObject;
        //rightHand["palm_right"] = Utils.RecursiveFindChild(transform, "palm_right").gameObject;
    }

    public void ResetFingersRot(bool rightHandReset)
    {
        if (!rightHandReset)
        {
            foreach (string key in leftHand.Keys)
            {
                var finger = leftHand[key];
                if (key.Contains("thumb"))
                    continue;
                else
                    finger.transform.localEulerAngles = Vector3.forward * -10;
            }
            leftHand["thumb_01_left"].transform.localEulerAngles = new Vector3(-10, 0, -22.5f);
            leftHand["thumb_02_left"].transform.localEulerAngles = new Vector3(-22.5f, 0, 0);
            leftHand["thumb_03_left"].transform.localEulerAngles = new Vector3(-22.5f, 0, 0);
        }
        else
        {
            foreach (string key in rightHand.Keys)
            {
                var finger = rightHand[key];
                if (key.Contains("thumb"))
                    continue;
                else
                    finger.transform.localEulerAngles = Vector3.forward * 10;
            }
            rightHand["thumb_01_right"].transform.localEulerAngles = new Vector3(-10, 0, 22.5f);
            rightHand["thumb_02_right"].transform.localEulerAngles = new Vector3(-22.5f, 0, 0);
            rightHand["thumb_03_right"].transform.localEulerAngles = new Vector3(-22.5f, 0, 0);
        }
    }

    public void GraspDexObjectButton()
    {
        GraspDexObject(interactedObject, handRight: handRightButton);
    }

    public void GraspDexObject(
        GameObject objectToTouch = null,
        bool randomize_grasp_idx = false,
        bool handRight = true,
        JSONObject jsonGrasps = null
    )
    {
        //current vars
        var current_hand = handRight ? rightHand : leftHand;
        var hand_str = handRight ? "right" : "left";

        //reset FingerPos
        ResetFingersRot(handRight);

        //reset object pos and rot
        objectToTouch.transform.position = Vector3.zero;
        objectToTouch.transform.eulerAngles = Vector3.zero;

        //read file
        JSONObject jsonObj = new JSONObject(
            objectToTouch.GetComponent<DexGraspAnnotation>().annotation.text
        );

        int current_index_grasp = 0;

        if (jsonGrasps != null)
        {
            indexSampler.range = new FloatRange(0, jsonGrasps.count);
            current_index_grasp = int.Parse(
                jsonGrasps[(int)indexSampler.Sample()]["idx_grasp"].ToString()
            );
        }
        else
        {
            //randomize index of grasp
            indexSampler.range = new FloatRange(0, jsonObj.count);
            current_index_grasp = randomize_grasp_idx ? (int)indexSampler.Sample() : index_grasp;
        }

        //convert data
        dexGraspData = new DexGraspNet.DexGraspData(jsonObj[current_index_grasp], hand_str);

        //set finger pose
        foreach (string key in dexGraspData.data.fingersRot.Keys)
        {
            if (key.Contains("thumb"))
                continue;
            current_hand[key].transform.localEulerAngles = dexGraspData.data.fingersRot[key];
        }

        //set thumb pose
        current_hand["thumb_01_" + hand_str]
            .transform.RotateAround(
                current_hand["thumb_01_" + hand_str].transform.position,
                current_hand["thumb_03_" + hand_str].transform.up,
                dexGraspData.data.fingersRot["thumb_01_" + hand_str].y
            );
        current_hand["thumb_01_" + hand_str]
            .transform.RotateAround(
                current_hand["thumb_01_" + hand_str].transform.position,
                current_hand["thumb_03_" + hand_str].transform.forward,
                dexGraspData.data.fingersRot["thumb_01_" + hand_str].z
            );
        current_hand["thumb_02_" + hand_str].transform.localEulerAngles = dexGraspData
            .data
            .fingersRot["thumb_02_" + hand_str];
        current_hand["thumb_03_" + hand_str].transform.localEulerAngles = dexGraspData
            .data
            .fingersRot["thumb_03_" + hand_str];

        //create tmp pivots
        GameObject pivot = new GameObject("pivot");
        GameObject pivot_child = new GameObject("pivot_child");
        pivot_child.transform.SetParent(pivot.transform);
        pivot_child.transform.localEulerAngles = new Vector3(0, -90, 50);

        //set pose
        pivot.transform.position = dexGraspData.data.translation;
        pivot.transform.eulerAngles = Vector3.zero;
        pivot.transform.Rotate(dexGraspData.data.rot.z * Vector3.forward, Space.Self);
        pivot.transform.Rotate(dexGraspData.data.rot.y * Vector3.up, Space.Self);
        pivot.transform.Rotate(dexGraspData.data.rot.x * Vector3.right, Space.Self);
        objectToTouch.transform.localScale = new Vector3(
            dexGraspData.data.scale + 0.01f,
            dexGraspData.data.scale + 0.01f,
            dexGraspData.data.scale + 0.01f
        );
        objectToTouch.transform.SetParent(pivot_child.transform);
        pivot.transform.SetParent(wristR.transform);
        pivot.transform.localPosition = Vector3.zero;
        pivot.transform.localEulerAngles = Vector3.zero;
        pivot_child.transform.localEulerAngles = Vector3.zero;

        if (!handRight)
        {
            pivot.transform.SetParent(wristL.transform);
            pivot.transform.localPosition = Vector3.zero;
            pivot.transform.localEulerAngles = new Vector3(180, 0, 0);
            pivot.transform.localScale = new Vector3(-1, -1, -1);
        }

        objectToTouch.transform.parent = handRight ? wristR.transform : wristL.transform;
        pivot.transform.parent = null;
        DestroyImmediate(pivot);
    }

    public void ApplyHandPoseNoObject(DexGraspNet.DexGraspData dexGraspData, bool handRight = true)
    {
        //current vars
        var current_hand = handRight ? rightHand : leftHand;
        var hand_str = handRight ? "right" : "left";

        //reset FingerPos
        ResetFingersRot(handRight);

        //set finger pose
        foreach (string key in dexGraspData.data.fingersRot.Keys)
        {
            if (key.Contains("thumb"))
                continue;
            current_hand[key].transform.localEulerAngles = dexGraspData.data.fingersRot[key];
        }

        //set thumb pose
        current_hand["thumb_01_" + hand_str]
            .transform.RotateAround(
                current_hand["thumb_01_" + hand_str].transform.position,
                current_hand["thumb_03_" + hand_str].transform.up,
                dexGraspData.data.fingersRot["thumb_01_" + hand_str].y
            );
        current_hand["thumb_01_" + hand_str]
            .transform.RotateAround(
                current_hand["thumb_01_" + hand_str].transform.position,
                current_hand["thumb_03_" + hand_str].transform.forward,
                dexGraspData.data.fingersRot["thumb_01_" + hand_str].z
            );
        current_hand["thumb_02_" + hand_str].transform.localEulerAngles = dexGraspData
            .data
            .fingersRot["thumb_02_" + hand_str];
        current_hand["thumb_03_" + hand_str].transform.localEulerAngles = dexGraspData
            .data
            .fingersRot["thumb_03_" + hand_str];
    }

    public void ApplyRandomPose(GameObject object_)
    {
        //read file
        JSONObject jsonObj = new JSONObject(
            object_.GetComponent<DexGraspAnnotation>().annotation.text
        );

        //randomize index of grasp
        indexSampler.range = new FloatRange(0, jsonObj.count);
        var current_index_grasp = (int)(indexSampler.Sample());

        //convert data
        dexGraspData = new DexGraspNet.DexGraspData(jsonObj[current_index_grasp], "right");

        //scale
        object_.transform.localScale = new Vector3(
            dexGraspData.data.scale + 0.01f,
            dexGraspData.data.scale + 0.01f,
            dexGraspData.data.scale + 0.01f
        );

        //rotation
        object_.transform.Rotate(dexGraspData.data.rot.z * Vector3.forward, Space.Self);
        object_.transform.Rotate(dexGraspData.data.rot.y * Vector3.up, Space.Self);
        object_.transform.Rotate(dexGraspData.data.rot.x * Vector3.right, Space.Self);
    }

    public void SetHandTargets(GameObject leftHandTarget, GameObject rightHandTarget)
    {
        this.leftHandTarget = leftHandTarget;
        this.rightHandTarget = rightHandTarget;

        fullBodyBipedIK = gameObject.GetComponent<FullBodyBipedIK>();
        leftHandTarget.transform.position = wristL.transform.position;
        leftHandTarget.transform.eulerAngles = wristL.transform.eulerAngles;

        rightHandTarget.transform.position = wristR.transform.position;
        rightHandTarget.transform.eulerAngles = wristR.transform.eulerAngles;

        fullBodyBipedIK.solver.leftHandEffector.target = leftHandTarget.transform;
        fullBodyBipedIK.solver.rightHandEffector.target = rightHandTarget.transform;
    }

    public void RestPosition()
    {
        leftHandTarget.transform.position = transform.TransformPoint(
            transform.InverseTransformPoint(fullBodyBipedIK.references.pelvis.transform.position)
                + new Vector3(-0.3f, 0, 0)
        );
        rightHandTarget.transform.position = transform.TransformPoint(
            transform.InverseTransformPoint(fullBodyBipedIK.references.pelvis.transform.position)
                + new Vector3(0.3f, 0, 0)
        );

        fullBodyBipedIK.solver.leftHandEffector.positionWeight = 1;
        fullBodyBipedIK.solver.rightHandEffector.positionWeight = 1;
    }

    public void RandomizeHandsPose()
    {
        var leftOffset = handPositionParameter.Sample();
        leftOffset = new Vector3(-leftOffset.x, leftOffset.y, leftOffset.z);
        var rightOffset = handPositionParameter.Sample();
        leftHandTarget.transform.position = transform.TransformPoint(
            transform.InverseTransformPoint(fullBodyBipedIK.references.spine[1].transform.position)
                + leftOffset
        );
        leftHandTarget.transform.eulerAngles = wristL.transform.eulerAngles;
        rightHandTarget.transform.position = transform.TransformPoint(
            transform.InverseTransformPoint(fullBodyBipedIK.references.spine[1].transform.position)
                + rightOffset
        );
        rightHandTarget.transform.eulerAngles = wristR.transform.eulerAngles;

        UniformSampler fingerRotSampler_x = new UniformSampler();
        fingerRotSampler_x.range = new FloatRange(-5, 0);

        UniformSampler fingerRotSampler_z = new UniformSampler();
        fingerRotSampler_z.range = new FloatRange(-50, 0);

        //set random finger pose
        //right
        rightHand["index_01_right"].transform.localEulerAngles = new Vector3(
            fingerRotSampler_x.Sample(),
            0,
            fingerRotSampler_z.Sample()
        );
        rightHand["index_02_right"].transform.localEulerAngles = new Vector3(
            0,
            0,
            fingerRotSampler_z.Sample()
        );
        rightHand["index_03_right"].transform.localEulerAngles = new Vector3(
            0,
            0,
            fingerRotSampler_z.Sample()
        );
        rightHand["middle_01_right"].transform.localEulerAngles = new Vector3(
            fingerRotSampler_x.Sample(),
            0,
            fingerRotSampler_z.Sample()
        );
        rightHand["middle_02_right"].transform.localEulerAngles = new Vector3(
            0,
            0,
            fingerRotSampler_z.Sample()
        );
        rightHand["middle_03_right"].transform.localEulerAngles = new Vector3(
            0,
            0,
            fingerRotSampler_z.Sample()
        );
        rightHand["ring_01_right"].transform.localEulerAngles = new Vector3(
            fingerRotSampler_x.Sample(),
            0,
            fingerRotSampler_z.Sample()
        );
        rightHand["ring_02_right"].transform.localEulerAngles = new Vector3(
            0,
            0,
            fingerRotSampler_z.Sample()
        );
        rightHand["ring_03_right"].transform.localEulerAngles = new Vector3(
            0,
            0,
            fingerRotSampler_z.Sample()
        );
        rightHand["pinky_01_right"].transform.localEulerAngles = new Vector3(
            fingerRotSampler_x.Sample(),
            0,
            fingerRotSampler_z.Sample()
        );
        rightHand["pinky_02_right"].transform.localEulerAngles = new Vector3(
            0,
            0,
            fingerRotSampler_z.Sample()
        );
        rightHand["pinky_03_right"].transform.localEulerAngles = new Vector3(
            0,
            0,
            fingerRotSampler_z.Sample()
        );
        rightHand["thumb_02_right"].transform.localEulerAngles = new Vector3(
            fingerRotSampler_x.Sample(),
            0,
            fingerRotSampler_x.Sample()
        );

        //left
        leftHand["index_01_left"].transform.localEulerAngles = new Vector3(
            -fingerRotSampler_x.Sample(),
            0,
            -fingerRotSampler_z.Sample()
        );
        leftHand["index_02_left"].transform.localEulerAngles = new Vector3(
            0,
            0,
            -fingerRotSampler_z.Sample()
        );
        leftHand["index_03_left"].transform.localEulerAngles = new Vector3(
            0,
            0,
            -fingerRotSampler_z.Sample()
        );
        leftHand["middle_01_left"].transform.localEulerAngles = new Vector3(
            -fingerRotSampler_x.Sample(),
            0,
            -fingerRotSampler_z.Sample()
        );
        leftHand["middle_02_left"].transform.localEulerAngles = new Vector3(
            0,
            0,
            -fingerRotSampler_z.Sample()
        );
        leftHand["middle_03_left"].transform.localEulerAngles = new Vector3(
            0,
            0,
            -fingerRotSampler_z.Sample()
        );
        leftHand["ring_01_left"].transform.localEulerAngles = new Vector3(
            -fingerRotSampler_x.Sample(),
            0,
            -fingerRotSampler_z.Sample()
        );
        leftHand["ring_02_left"].transform.localEulerAngles = new Vector3(
            0,
            0,
            -fingerRotSampler_z.Sample()
        );
        leftHand["ring_03_left"].transform.localEulerAngles = new Vector3(
            0,
            0,
            -fingerRotSampler_z.Sample()
        );
        leftHand["pinky_01_left"].transform.localEulerAngles = new Vector3(
            -fingerRotSampler_x.Sample(),
            0,
            -fingerRotSampler_z.Sample()
        );
        leftHand["pinky_02_left"].transform.localEulerAngles = new Vector3(
            0,
            0,
            -fingerRotSampler_z.Sample()
        );
        leftHand["pinky_03_left"].transform.localEulerAngles = new Vector3(
            0,
            0,
            -fingerRotSampler_z.Sample()
        );
        leftHand["thumb_02_left"].transform.localEulerAngles = new Vector3(
            -fingerRotSampler_x.Sample(),
            0,
            -fingerRotSampler_x.Sample()
        );
    }

    public void RandomizeHandsPose(List<GameObject> objects)
    {
        var leftOffset = handPositionParameter.Sample();
        leftOffset = new Vector3(-leftOffset.x, leftOffset.y, leftOffset.z);
        var rightOffset = handPositionParameter.Sample();
        leftHandTarget.transform.position = transform.TransformPoint(
            transform.InverseTransformPoint(fullBodyBipedIK.references.spine[1].transform.position)
                + leftOffset
        );
        leftHandTarget.transform.eulerAngles = wristL.transform.eulerAngles;
        rightHandTarget.transform.position = transform.TransformPoint(
            transform.InverseTransformPoint(fullBodyBipedIK.references.spine[1].transform.position)
                + rightOffset
        );
        rightHandTarget.transform.eulerAngles = wristR.transform.eulerAngles;

        var objectToTouch = objects[0];

        //reset FingerPos
        ResetFingersRot(true);
        ResetFingersRot(false);

        List<string> hands_str = new List<string>();
        hands_str.Add("right");
        hands_str.Add("left");

        foreach (var hand_str in hands_str)
        {
            var current_hand = hand_str == "right" ? rightHand : leftHand;
            //read file
            JSONObject jsonObj = new JSONObject(
                objectToTouch.GetComponent<DexGraspAnnotation>().annotation.text
            );

            //randomize index of grasp
            indexSampler.range = new FloatRange(0, jsonObj.count);
            var current_index_grasp = (int)(indexSampler.Sample());

            //convert data
            dexGraspData = new DexGraspNet.DexGraspData(jsonObj[current_index_grasp], hand_str);

            //set finger pose
            foreach (string key in dexGraspData.data.fingersRot.Keys)
            {
                if (key.Contains("thumb"))
                    continue;
                current_hand[key].transform.localEulerAngles = dexGraspData.data.fingersRot[key];
            }

            //set thumb pose
            current_hand["thumb_01_" + hand_str]
                .transform.RotateAround(
                    current_hand["thumb_01_" + hand_str].transform.position,
                    current_hand["thumb_03_" + hand_str].transform.up,
                    dexGraspData.data.fingersRot["thumb_01_" + hand_str].y
                );
            current_hand["thumb_01_" + hand_str]
                .transform.RotateAround(
                    current_hand["thumb_01_" + hand_str].transform.position,
                    current_hand["thumb_03_" + hand_str].transform.forward,
                    dexGraspData.data.fingersRot["thumb_01_" + hand_str].z
                );
            current_hand["thumb_02_" + hand_str].transform.localEulerAngles = dexGraspData
                .data
                .fingersRot["thumb_02_" + hand_str];
            current_hand["thumb_03_" + hand_str].transform.localEulerAngles = dexGraspData
                .data
                .fingersRot["thumb_03_" + hand_str];
        }
    }
}
