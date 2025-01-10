using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class AssignAnnotationsToPrefabDexGrasp : MonoBehaviour
{
#pragma warning disable CS0436 // Type conflicts with imported type
    [InspectorButton("StartAssign")]
#pragma warning restore CS0436 // Type conflicts with imported type
    public bool startConversion;

    public void StartAssign()
    {
        string path_prefab = EditorUtility.OpenFolderPanel(
            "Path Models",
            "Assets/Prefabs/DexGraspNet",
            ""
        );
        string path_annotations = EditorUtility.OpenFolderPanel(
            "Path Models",
            "Assets/DexGraspNet/data",
            ""
        );
        string[] folders = Directory.GetDirectories(path_prefab);

        for (int i = 0; i < folders.Length; i++)
        {
            string current_folder = folders[i].Replace("\\", "/").Split("Assets/")[1];

            string[] files = Directory.GetFiles("Assets/" + current_folder, "*prefab");

            foreach (var file in files)
            {
                var current_file = file.Replace("\\", "/");
                var filename = current_file.Split("/").Last();
                var dir_ = current_file.Split("/").SkipLast(1).Last();
                print(filename);
                var annotation_filename = filename.Replace(".prefab", ".json");

                var annotation_file =
                    "Assets/"
                    + Directory
                        .GetFiles(
                            Path.Combine(path_annotations, dir_),
                            filename.Replace(".prefab", ".json")
                        )[0]
                        .Replace("\\", "/")
                        .Split("Assets/")[1];
                TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(annotation_file);

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(current_file);

                var obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

                obj.GetComponent<DexGraspAnnotation>().annotation = textAsset;

                PrefabUtility.ApplyPrefabInstance(obj, InteractionMode.UserAction);
                DestroyImmediate(obj);
            }
        }
        GUIUtility.ExitGUI();
    }
}
