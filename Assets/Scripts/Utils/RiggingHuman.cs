using RootMotion.FinalIK;
using UnityEditor;
using UnityEngine;

public static class RiggingHuman
{
    [MenuItem("CONTEXT/FullBodyBipedIK/Custom Auto rigging")]
    public static void AutoRiggingModel(MenuCommand menuCommand)
    {
        FullBodyBipedIK fullBodyBiped = menuCommand.context as FullBodyBipedIK;
        Transform parent = fullBodyBiped.transform;
        fullBodyBiped.references.root = parent;
        fullBodyBiped.references.pelvis = parent.Find("root/hip");
        fullBodyBiped.references.leftThigh = parent.Find("root/hip/leg_left");
        fullBodyBiped.references.leftCalf = parent.Find("root/hip/leg_left/knee_left");
        fullBodyBiped.references.leftFoot = parent.Find(
            "root/hip/leg_left/knee_left/ankle_left/foot_left"
        );

        fullBodyBiped.references.rightThigh = parent.Find("root/hip/leg_right");
        fullBodyBiped.references.rightCalf = parent.Find("root/hip/leg_right/knee_right");
        fullBodyBiped.references.rightFoot = parent.Find(
            "root/hip/leg_right/knee_right/ankle_right/foot_right"
        );

        fullBodyBiped.references.leftUpperArm = parent.Find(
            "root/hip/spine_01/spine_02/spine_03/clavicle_left/shoulder_left"
        );
        fullBodyBiped.references.leftForearm = parent.Find(
            "root/hip/spine_01/spine_02/spine_03/clavicle_left/shoulder_left/elbow_left/"
        );
        fullBodyBiped.references.leftHand = parent.Find(
            "root/hip/spine_01/spine_02/spine_03/clavicle_left/shoulder_left/elbow_left/wrist_left"
        );

        fullBodyBiped.references.rightUpperArm = parent.Find(
            "root/hip/spine_01/spine_02/spine_03/clavicle_right/shoulder_right"
        );
        fullBodyBiped.references.rightForearm = parent.Find(
            "root/hip/spine_01/spine_02/spine_03/clavicle_right/shoulder_right/elbow_right/"
        );
        fullBodyBiped.references.rightHand = parent.Find(
            "root/hip/spine_01/spine_02/spine_03/clavicle_right/shoulder_right/elbow_right/wrist_right"
        );

        fullBodyBiped.references.head = parent.Find(
            "root/hip/spine_01/spine_02/spine_03/neck/head"
        );

        parent.Find("root/hip/leg_right/knee_right").transform.eulerAngles = new Vector3(14, 0, 0);
        parent.Find("root/hip/leg_left/knee_left").transform.eulerAngles = new Vector3(14, 0, 0);

        parent
            .Find("root/hip/spine_01/spine_02/spine_03/clavicle_left/shoulder_left/")
            .eulerAngles = new Vector3(0, 0, -36.0587845f);
        parent
            .Find("root/hip/spine_01/spine_02/spine_03/clavicle_left/shoulder_left/elbow_left/")
            .eulerAngles = new Vector3(318.444031f, 5.01323366f, 16.5137081f);

        parent
            .Find("root/hip/spine_01/spine_02/spine_03/clavicle_right/shoulder_right/")
            .eulerAngles = new Vector3(0, 0, 36.0587845f);
        parent
            .Find("root/hip/spine_01/spine_02/spine_03/clavicle_right/shoulder_right/elbow_right/")
            .eulerAngles = new Vector3(305.014404f, 7.34240818f, 330.707458f);

        fullBodyBiped.references.spine = new Transform[]
        {
            parent.Find("root/hip/spine_01"),
            parent.Find("root/hip/spine_01/spine_02"),
            parent.Find("root/hip/spine_01/spine_02/spine_03")
        };

        fullBodyBiped.SetReferences(fullBodyBiped.references, parent.Find("root/hip/spine_01/"));
    }

    public static void AutoRiggingModel(FullBodyBipedIK fullBodyBiped)
    {
        Transform parent = fullBodyBiped.transform;
        fullBodyBiped.references.root = parent;
        fullBodyBiped.references.pelvis = parent.Find("root/hip");
        fullBodyBiped.references.leftThigh = parent.Find("root/hip/leg_left");
        fullBodyBiped.references.leftCalf = parent.Find("root/hip/leg_left/knee_left");
        fullBodyBiped.references.leftFoot = parent.Find("root/hip/leg_left/knee_left/ankle_left");

        fullBodyBiped.references.rightThigh = parent.Find("root/hip/leg_right");
        fullBodyBiped.references.rightCalf = parent.Find("root/hip/leg_right/knee_right");
        fullBodyBiped.references.rightFoot = parent.Find(
            "root/hip/leg_right/knee_right/ankle_right"
        );

        fullBodyBiped.references.leftUpperArm = parent.Find(
            "root/hip/spine_01/spine_02/spine_03/clavicle_left/shoulder_left"
        );
        fullBodyBiped.references.leftForearm = parent.Find(
            "root/hip/spine_01/spine_02/spine_03/clavicle_left/shoulder_left/elbow_left/"
        );
        fullBodyBiped.references.leftHand = parent.Find(
            "root/hip/spine_01/spine_02/spine_03/clavicle_left/shoulder_left/elbow_left/wrist_left"
        );

        fullBodyBiped.references.rightUpperArm = parent.Find(
            "root/hip/spine_01/spine_02/spine_03/clavicle_right/shoulder_right"
        );
        fullBodyBiped.references.rightForearm = parent.Find(
            "root/hip/spine_01/spine_02/spine_03/clavicle_right/shoulder_right/elbow_right/"
        );
        fullBodyBiped.references.rightHand = parent.Find(
            "root/hip/spine_01/spine_02/spine_03/clavicle_right/shoulder_right/elbow_right/wrist_right"
        );

        fullBodyBiped.references.head = parent.Find(
            "root/hip/spine_01/spine_02/spine_03/neck/head"
        );

        //parent.Find("root/hip/leg_right/knee_right").transform.eulerAngles = new Vector3(14, 0, 0);
        //parent.Find("root/hip/leg_left/knee_left").transform.eulerAngles = new Vector3(14, 0, 0);

        //parent.Find("root/hip/spine_01/spine_02/spine_03/clavicle_left/shoulder_left/").eulerAngles = new Vector3(0, 0, -36.0587845f);
        //parent.Find("root/hip/spine_01/spine_02/spine_03/clavicle_left/shoulder_left/elbow_left/").eulerAngles = new Vector3(318.444031f, 5.01323366f, 16.5137081f);

        //parent.Find("root/hip/spine_01/spine_02/spine_03/clavicle_right/shoulder_right/").eulerAngles = new Vector3(0, 0, 36.0587845f);
        //parent.Find("root/hip/spine_01/spine_02/spine_03/clavicle_right/shoulder_right/elbow_right/").eulerAngles = new Vector3(305.014404f, 7.34240818f, 330.707458f);

        parent
            .Find("root/hip/spine_01/spine_02/spine_03/clavicle_left/shoulder_left/")
            .eulerAngles = new Vector3(0, 0, -45);
        parent
            .Find("root/hip/spine_01/spine_02/spine_03/clavicle_left/shoulder_left/elbow_left/")
            .localEulerAngles = new Vector3(0, -5, 9);

        parent
            .Find("root/hip/spine_01/spine_02/spine_03/clavicle_right/shoulder_right/")
            .eulerAngles = new Vector3(0, 0, 45);
        parent
            .Find("root/hip/spine_01/spine_02/spine_03/clavicle_right/shoulder_right/elbow_right/")
            .localEulerAngles = new Vector3(0, 5, -9);

        fullBodyBiped.references.spine = new Transform[]
        {
            parent.Find("root/hip/spine_01"),
            parent.Find("root/hip/spine_01/spine_02"),
            parent.Find("root/hip/spine_01/spine_02/spine_03")
        };

        fullBodyBiped.SetReferences(fullBodyBiped.references, parent.Find("root/hip/spine_01/"));
    }
}
