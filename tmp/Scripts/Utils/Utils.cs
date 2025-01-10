using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Perception.Randomization.Samplers;
using UnityEngine.Perception.Randomization.Utilities;

[Serializable]
public class StringGameObjectDictionary : SerializableDictionary<string, GameObject> { }

[Serializable]
public class StringFloatDictionary : SerializableDictionary<string, float> { }

[Serializable]
public class StringVector3Dictionary : SerializableDictionary<string, Vector3> { }

public static class Utils
{
    public static Transform RecursiveFindChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
            {
                return child;
            }
            else
            {
                Transform found = RecursiveFindChild(child, childName);
                if (found != null)
                {
                    return found;
                }
            }
        }
        return null;
    }

    public static Bounds GetBounds(GameObject obj)
    {
        Bounds bounds = new Bounds();
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            //Find first enabled renderer to start encapsulate from it
            foreach (Renderer renderer in renderers)
            {
                if (renderer.enabled)
                {
                    bounds = renderer.bounds;
                    break;
                }
            }
            //Encapsulate for all renderers
            foreach (Renderer renderer in renderers)
            {
                if (renderer.enabled)
                    bounds.Encapsulate(renderer.bounds);
            }
        }
        return bounds;
    }

    public static void ExtractMaterials(string assetPath, string destinationPath)
    {
        HashSet<string> hashSet = new HashSet<string>();
        IEnumerable<UnityEngine.Object> enumerable =
            from x in AssetDatabase.LoadAllAssetsAtPath(assetPath)
            where x.GetType() == typeof(Material)
            select x;
        foreach (UnityEngine.Object item in enumerable)
        {
            string path = System.IO.Path.Combine(destinationPath, item.name) + ".mat";
            path = AssetDatabase.GenerateUniqueAssetPath(path);
            string value = AssetDatabase.ExtractAsset(item, path);
            if (string.IsNullOrEmpty(value))
            {
                hashSet.Add(assetPath);
            }
        }

        foreach (string item2 in hashSet)
        {
            AssetDatabase.WriteImportSettingsIfDirty(item2);
            AssetDatabase.ImportAsset(item2, ImportAssetOptions.ForceUpdate);
        }
    }

    public static List<GameObject> GetFirstChilds(Transform _t)
    {
        List<GameObject> ts = new List<GameObject>();

        for (int i = 0; i < _t.childCount; ++i)
        {
            Transform child = _t.GetChild(i);
            ts.Add(child.gameObject);
            //execute functionality of child transform here
        }

        return ts;
    }

    public static void RandomizeObjectsPosition(
        GameObject human,
        GameObject hand,
        List<GameObject> objects,
        Vector2 placementArea,
        float separationDistance = 0.35f,
        float depth = 0.7f
    )
    {
        var seed = SamplerState.NextRandomState();
        var placementSamples = PoissonDiskSampling.GenerateSamples(
            placementArea.x,
            placementArea.y,
            separationDistance,
            seed
        );
        int i = 0;
        foreach (var instance in objects)
        {
            var sample = placementSamples[i];
            instance.transform.position = human.transform.TransformPoint(
                new Vector3(sample.x - (placementArea.x / 2), sample.y + 0.5f, depth)
            );
            i++;
        }
        placementSamples.Dispose();
    }
}
