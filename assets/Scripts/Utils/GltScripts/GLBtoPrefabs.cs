using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class GLBtoPrefabs : MonoBehaviour
{
    [InspectorButton("StartConversion")]
    public bool startConversion;

    public void StartConversion()
    {
        GameObject object_to_convert;

        string path_models = EditorUtility.OpenFolderPanel(
            "Path Models",
            "Assets/DexGraspNet/GLB",
            ""
        );
        string save_path = EditorUtility.OpenFolderPanel("Save Path", "Assets/Prefabs/DexGraspNet", "");

        string[] folders = Directory.GetDirectories(path_models);

        for (int i = 0; i < folders.Length; i++)
        {
            var tmp = folders[i].Replace("\\", "/").Split("/");
            var current_save_path = Path.Combine(save_path, tmp[tmp.Length - 1]).Replace("\\", "/");

            Directory.CreateDirectory(current_save_path);

            string current_folder = "Assets/" + folders[i].Replace("\\", "/").Split("Assets/")[1];
            var files = Directory.GetFiles(current_folder, "*.glb"); //[0].Replace("\\", "/");
            foreach (string file in files)
            {
                tmp = file.Replace("\\", "/").Split("/");
                string filename_save = tmp[tmp.Length - 1].Replace(".glb", ".prefab");
                Debug.Log(filename_save);
                object_to_convert = GameObject.Instantiate(
                    AssetDatabase.LoadAssetAtPath<GameObject>(file)
                );
                PrefabUtility.SaveAsPrefabAsset(object_to_convert, Path.Combine(current_save_path, filename_save));

                DestroyImmediate(object_to_convert);
            }
        }

        GUIUtility.ExitGUI();
    }
}
