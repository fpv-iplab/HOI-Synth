using System;
using System.Collections.Generic;
using Defective.JSON;
using UnityEditor;
using UnityEngine;
using UnityEngine.Perception.Randomization.Randomizers;
using UnityEngine.Perception.Randomization.Samplers;

[Serializable]
[AddRandomizerMenu("ManualEnvironmentRandomizer")]
public class ManualEnvironmentRandomizer : Randomizer, ICustomRandomizer
{
    public bool ready { get; set; }
    bool envPlaced = false;

    [Tooltip("The Rotation offset applied to all objects.")]
    public Vector3 rotationOffset;

#pragma warning disable CS0436 // Type conflicts with imported type
    [InspectorButton("OpenPrefabPathEnv")]
#pragma warning restore CS0436 // Type conflicts with imported type
    public bool prefabPath;
    public List<string> prefabs_paths;
    List<string> envs_list;
    JSONObject jsonObj;

    public TextAsset filePoses;

    int tmp_current_idx_env = 0,
        idx_view = 0,
        current_number_poses = 1;

    GameObject m_Container,
        activeEnvironment = null;
    GameObject human;
    UniformSampler uniformSampler = new UniformSampler();

    protected override void OnAwake()
    {
        m_Container = new GameObject("EnvironmentContainer");
        m_Container.AddComponent<NavMeshSurfaceRandomizerTag>();
        jsonObj = new JSONObject(filePoses.text);
        envs_list = jsonObj.keys;
    }

    protected override void OnIterationStart()
    {
        if (activeEnvironment == null)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                prefabs_paths[SearchEnvName(envs_list[tmp_current_idx_env])]
            );
            activeEnvironment = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            activeEnvironment.transform.position = Vector3.zero;
            activeEnvironment.transform.rotation = Quaternion.Euler(
                rotationOffset.x,
                rotationOffset.y,
                rotationOffset.z
            );
            activeEnvironment.transform.parent = m_Container.transform;
        }
        envPlaced = true;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if (envPlaced && !ready)
        {
            //Place Human
            if (
                UnityEngine
                    .Object.FindObjectsByType<CustomHumanTag>(FindObjectsSortMode.None)
                    .Length == 1
            )
            {
                var list_of_poses = jsonObj[envs_list[tmp_current_idx_env]];
                current_number_poses = list_of_poses.count;
                //uniformSampler = new UniformSampler(0, list_of_poses.count);
                var current_pose = list_of_poses[idx_view];
                human = UnityEngine
                    .Object.FindObjectsByType<CustomHumanTag>(FindObjectsSortMode.None)[0]
                    .gameObject;

                human.transform.position = new Vector3(
                    current_pose["position"][0].floatValue,
                    current_pose["position"][1].floatValue,
                    current_pose["position"][2].floatValue
                );

                human.transform.eulerAngles = new Vector3(
                    current_pose["rotation"][0].floatValue,
                    current_pose["rotation"][1].floatValue,
                    current_pose["rotation"][2].floatValue
                );

                uniformSampler.range = new FloatRange(-40, 40);
                human.transform.Rotate(uniformSampler.Sample() * Vector3.up);

                ready = true;
            }
        }
    }

    protected override void OnIterationEnd()
    {
        if (++idx_view == current_number_poses)
            DestroyCurrentEnv();
        ready = false;
        envPlaced = false;
    }

    public void DestroyCurrentEnv()
    {
        idx_view = 0;
        tmp_current_idx_env = (tmp_current_idx_env + 1) % envs_list.Count;
        UnityEngine.Object.Destroy(activeEnvironment);
        EditorUtility.UnloadUnusedAssetsImmediate();
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
        activeEnvironment = null;
    }

    int SearchEnvName(string envName)
    {
        for (int i = 0; i < prefabs_paths.Count; i++)
        {
            if (prefabs_paths[i].Contains(envName))
                return i;
        }
        return 0;
    }
}
