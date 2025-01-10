using System;
using System.Collections.Generic;
using Defective.JSON;
using MyBox;
using UnityEditor;
using UnityEngine;
using UnityEngine.Perception.Randomization.Parameters;
using UnityEngine.Perception.Randomization.Randomizers;
using UnityEngine.Perception.Randomization.Samplers;

[Serializable]
[AddRandomizerMenu("ObjectRandomizer")]
public class ObjectRandomizer : Randomizer, ICustomRandomizer
{
    public List<string> prefabs_paths;

    [HideInInspector]
    public UniformSampler uniformSampler;

#pragma warning disable CS0436 // Type conflicts with imported type
    [InspectorButton("OpenPrefabPathObj")]
#pragma warning restore CS0436 // Type conflicts with imported type
    public bool prefabPath;
    public Vector3 offset = new Vector3(1000, 1000, 1000);

    [HideInInspector]
    public List<GameObject> current_objects;

    public IntegerParameter number_of_objects_in_scene;

    public GameObject m_Container;

    public bool ready { get; set; }

    public bool random = true,
        destroyObjectIt = true,
        selectObjectsFromFile = false;
    JSONObject jsonObj;

    [ConditionalField(nameof(selectObjectsFromFile))]
    public TextAsset fileObjects;

    public int idx = -1;

    [HideInInspector]
    public bool destroyAtEnd = false;

    protected override void OnAwake()
    {
        if (selectObjectsFromFile)
        {
            List<string> tmp_prefabs_paths = new List<string>();
            jsonObj = new JSONObject(fileObjects.text);
            foreach (var key in jsonObj.keys)
            {
                var idx = SearchObjIdx(key);
                if (idx == -1)
                    continue;
                tmp_prefabs_paths.Add(prefabs_paths[idx]);
            }
            uniformSampler.range = new FloatRange(0, tmp_prefabs_paths.Count);
            prefabs_paths = tmp_prefabs_paths;
        }
    }

    protected override void OnIterationStart()
    {
        if (current_objects.Count == 0)
        {
            for (int i = 0; i < number_of_objects_in_scene.Sample(); i++)
            {
                try
                {
                    idx = random ? (int)uniformSampler.Sample() : idx + 1;
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabs_paths[idx]);
                    var obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    obj.transform.position = Vector3.zero;
                    obj.transform.rotation = Quaternion.Euler(Vector3.zero);

                    current_objects.Add(obj);
                    current_objects[i].transform.parent = m_Container.transform;
                    current_objects[i].transform.position = offset;
                }
                catch (Exception e)
                {
                    Debug.Log($"Error. Skipped: {e.ToString()}");
                }
            }
        }
        ready = true;
    }

    protected override void OnIterationEnd()
    {
        if (destroyObjectIt || destroyAtEnd)
        {
            DestroyAllObjects();
            destroyAtEnd = false;
        }

        ready = false;
    }

    public void DestroyAllObjects()
    {
        foreach (var g in current_objects)
            UnityEngine.Object.Destroy(g);
        current_objects.Clear();
    }

    int SearchObjIdx(string objName)
    {
        for (int i = 0; i < prefabs_paths.Count; i++)
        {
            if (prefabs_paths[i].Contains(objName))
                return i;
        }
        return -1;
    }
}
