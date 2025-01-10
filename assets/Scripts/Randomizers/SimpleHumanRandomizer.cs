using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Perception.Randomization.Randomizers;
using UnityEngine.Perception.Randomization.Samplers;

[Serializable]
[AddRandomizerMenu("SimpleHumanRandomizer")]
public class SimpleHumanRandomizer : Randomizer, ICustomRandomizer
{
    public List<GameObject> humans;

    [HideInInspector]
    public bool ready { get; set; }

    [HideInInspector]
    public UniformSampler uniformSampler;

    public GameObject activeHuman;

    protected override void OnAwake()
    {
        base.OnAwake();
    }

    protected override void OnIterationStart()
    {
        var idx = (int)uniformSampler.Sample();
        activeHuman = (GameObject)PrefabUtility.InstantiatePrefab(humans[idx]);
        activeHuman.transform.position = Vector3.zero;
        activeHuman.transform.rotation = Quaternion.Euler(Vector3.zero);
        activeHuman.AddComponent<CustomHumanTag>();

        ready = true;
    }

    protected override void OnIterationEnd()
    {
        UnityEngine.Object.Destroy(activeHuman);
        ready = false;
    }
}
