
using System;
using Defective.JSON;
using UnityEngine;

public static class DexGraspNet
{
    [Serializable]
    public class DexGraspData
    {
        [Serializable]
        public struct DexGraspDataStruct
        {
            public StringVector3Dictionary fingersRot;
            public float scale;
            public Vector3 translation;
            public Vector3 rot;
        }

        [SerializeField]
        public DexGraspDataStruct data;

        public DexGraspData(JSONObject data_, string hand = "right")
        {

            var current_sign = hand == "right" ? 1 : -1;

            data.fingersRot = new StringVector3Dictionary();
            var qpos = data_["qpos"];
            //Get scale of the interacted obj
            data.scale = data_["scale"].floatValue;
            //Get translation
            data.translation = new Vector3(-qpos["WRJTx"].floatValue, qpos["WRJTy"].floatValue, qpos["WRJTz"].floatValue);
            //Get rotation
            data.rot = new Vector3(qpos["WRJRx"].floatValue * Mathf.Rad2Deg, -qpos["WRJRy"].floatValue * Mathf.Rad2Deg, -qpos["WRJRz"].floatValue * Mathf.Rad2Deg);

            //Get fingers rot

            //index
            data.fingersRot["index_01_" + hand] = new Vector3(-qpos["robot0:FFJ3"].floatValue * Mathf.Rad2Deg, 0, current_sign * (-qpos["robot0:FFJ2"].floatValue * Mathf.Rad2Deg + 10));
            data.fingersRot["index_02_" + hand] = new Vector3(0, 0, current_sign * (-qpos["robot0:FFJ1"].floatValue * Mathf.Rad2Deg + 10));
            data.fingersRot["index_03_" + hand] = new Vector3(0, 0, current_sign * (-qpos["robot0:FFJ0"].floatValue * Mathf.Rad2Deg + 10));

            //middle
            data.fingersRot["middle_01_" + hand] = new Vector3(-qpos["robot0:MFJ3"].floatValue * Mathf.Rad2Deg, 0, current_sign * (-qpos["robot0:MFJ2"].floatValue * Mathf.Rad2Deg + 10));
            data.fingersRot["middle_02_" + hand] = new Vector3(0, 0, current_sign * (-qpos["robot0:MFJ1"].floatValue * Mathf.Rad2Deg + 10));
            data.fingersRot["middle_03_" + hand] = new Vector3(0, 0, current_sign * (-qpos["robot0:MFJ0"].floatValue * Mathf.Rad2Deg + 10));

            //ring
            data.fingersRot["ring_01_" + hand] = new Vector3(-qpos["robot0:RFJ3"].floatValue * Mathf.Rad2Deg, 0, current_sign * (-qpos["robot0:RFJ2"].floatValue * Mathf.Rad2Deg + 10));
            data.fingersRot["ring_02_" + hand] = new Vector3(0, 0, current_sign * (-qpos["robot0:RFJ1"].floatValue * Mathf.Rad2Deg + 10));
            data.fingersRot["ring_03_" + hand] = new Vector3(0, 0, current_sign * (-qpos["robot0:RFJ0"].floatValue * Mathf.Rad2Deg + 10));

            //pinky
            data.fingersRot["pinky_01_" + hand] = new Vector3(-qpos["robot0:LFJ3"].floatValue * Mathf.Rad2Deg, 0, current_sign * (-qpos["robot0:LFJ2"].floatValue * Mathf.Rad2Deg + 20));
            data.fingersRot["pinky_02_" + hand] = new Vector3(0, 0, current_sign * (-qpos["robot0:LFJ1"].floatValue * Mathf.Rad2Deg + 10));
            data.fingersRot["pinky_03_" + hand] = new Vector3(0, 0, current_sign * (-qpos["robot0:LFJ0"].floatValue * Mathf.Rad2Deg + 10));

            //thumb
            data.fingersRot["thumb_01_" + hand] = new Vector3(0, current_sign * -qpos["robot0:THJ4"].floatValue * Mathf.Rad2Deg, current_sign * -qpos["robot0:THJ3"].floatValue * Mathf.Rad2Deg);
            data.fingersRot["thumb_02_" + hand] = new Vector3(-qpos["robot0:THJ1"].floatValue * Mathf.Rad2Deg - 22.5f, 0, current_sign * -qpos["robot0:THJ2"].floatValue * Mathf.Rad2Deg);
            data.fingersRot["thumb_03_" + hand] = new Vector3(-qpos["robot0:THJ0"].floatValue * Mathf.Rad2Deg - 22.5f, 0, 0);

        }
    }
}

