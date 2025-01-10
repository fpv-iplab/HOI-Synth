using RootMotion.FinalIK;
using UnityEditor;
using UnityEngine;

public static class SaveHumans
{
    [MenuItem("CONTEXT/Transform/Save Humans...")]
    public static void SaveHuman(UnityEditor.MenuCommand menuCommand)
    {
        var current = menuCommand.context as Transform;

        AssetDatabase.CreateFolder("Assets", "Generated_Humans");

        foreach (Transform child in current) // iterate over children
        {
            AssetDatabase.CreateFolder("Assets/Generated_Humans", child.name);

            foreach (SkinnedMeshRenderer sk in child.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                Mesh m = sk.sharedMesh;
                string mesh_path =
                    "Assets/Generated_Humans/" + child.name + "/" + sk.name + ".asset";
                SaveElement(m, mesh_path, true);

                foreach (Material mat in sk.materials)
                {
                    string file_path =
                        "Assets/Generated_Humans/" + child.name + "/" + mat.name + ".mat";
                    SaveElement(mat, file_path);
                }
            }

            FullBodyBipedIK fbik = child.gameObject.AddComponent<FullBodyBipedIK>();
            RiggingHuman.AutoRiggingModel(fbik);

            var pb = PrefabUtility.SaveAsPrefabAsset(
                child.gameObject,
                "Assets/Generated_Humans/" + child.name + "/human.prefab"
            );
            pb.SetActive(true);
            AssignFaceBodyEyeMaterials.AssignMaterials(pb.GetComponent<SkinnedMeshRenderer>());
        }
    }

    public static void SaveElement(Mesh meshToSave, string path, bool optimizeMesh)
    {
        if (optimizeMesh)
            MeshUtility.Optimize(meshToSave);
        AssetDatabase.CreateAsset(meshToSave, path);
        AssetDatabase.SaveAssets();
    }

    public static void SaveElement(Material mat, string path)
    {
        AssetDatabase.CreateAsset(mat, path);
        AssetDatabase.SaveAssets();
    }
}
