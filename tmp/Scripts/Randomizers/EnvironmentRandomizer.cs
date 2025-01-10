using System;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;
using UnityEngine.Perception.Randomization.Randomizers;
using UnityEngine.Perception.Randomization.Samplers;

[Serializable]
[AddRandomizerMenu("CustomEnvironmentRandomizer")]
public class EnvironmentRandomizer : Randomizer, ICustomRandomizer
{
    [Tooltip("The Rotation offset applied to all objects.")]
    public Vector3 rotationOffset;

    //[Tooltip("The list of the Environments.")]
    //public CategoricalParameter<GameObject> prefabs;

    public List<string> prefabs_paths;

#pragma warning disable CS0436 // Type conflicts with imported type
    [InspectorButton("OpenPrefabPathEnv")]
#pragma warning restore CS0436 // Type conflicts with imported type
    public bool prefabPath;

    [HideInInspector]
    public UniformSampler uniformSampler;

    public int number_of_iteractions_w_same_env = 1;

    public bool random = true;

    int tmp_current_iteraction;

    GameObject m_Container;
    GameObject activeEnvironment;
    NavMeshSurface navMeshSurface;

    public bool ready { get; set; }

    public int idx = 0;

    protected override void OnAwake()
    {
        m_Container = new GameObject("EnvironmentContainer");
        navMeshSurface = m_Container.AddComponent<NavMeshSurface>();
        navMeshSurface.minRegionArea = 0.1f;
        navMeshSurface.overrideVoxelSize = true;
        navMeshSurface.voxelSize = 0.1f;
        navMeshSurface.collectObjects = CollectObjects.Children;
        m_Container.AddComponent<NavMeshSurfaceRandomizerTag>();
        tmp_current_iteraction = 0;
    }

    protected override void OnIterationStart()
    {
        if (tmp_current_iteraction == 0 || activeEnvironment == null)
        {
            if (random)
                idx = (int)uniformSampler.Sample();
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabs_paths[idx]);
            activeEnvironment = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            activeEnvironment.transform.position = Vector3.zero;
            activeEnvironment.transform.rotation = Quaternion.Euler(
                rotationOffset.x,
                rotationOffset.y,
                rotationOffset.z
            );
            //activeEnvironment = GameObject.Instantiate(prefabs.Sample(), Vector3.zero, Quaternion.Euler(rotationOffset.x, rotationOffset.y, rotationOffset.z));
            activeEnvironment.transform.parent = m_Container.transform;
            if (!random)
                idx = (idx + 1) % prefabs_paths.Count;
        }
        ready = true;
    }

    protected override void OnIterationEnd()
    {
        if (++tmp_current_iteraction == number_of_iteractions_w_same_env)
            DestroyCurrentEnv();
        ready = false;
    }

    public void DestroyCurrentEnv()
    {
        tmp_current_iteraction = 0;
        navMeshSurface.navMeshData = null;
        UnityEngine.Object.Destroy(activeEnvironment);
        EditorUtility.UnloadUnusedAssetsImmediate();
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
        activeEnvironment = null;
    }
}
