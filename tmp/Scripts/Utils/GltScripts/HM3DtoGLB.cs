using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GLTFast;
using GLTFast.Export;
using UnityEditor;
using UnityEngine;

public class HM3DtoGLB : MonoBehaviour
{
 [InspectorButton("StartConversion")]
    public bool startConversion;

    public void StartConversion()
    {
        var exportSettings = new ExportSettings
        {
            Format = GltfFormat.Binary,
            FileConflictResolution = FileConflictResolution.Overwrite,
            // Export everything except cameras or animation
            ComponentMask = ~(ComponentType.Camera | ComponentType.Animation),
            // Boost light intensities
            LightIntensityFactor = 100f,
            Compression = Compression.Draco,
            DracoSettings = new DracoExportSettings { positionQuantization = 12 }
        };

        GameObject object_to_convert;

        string path_models = EditorUtility.OpenFolderPanel(
            "Path Prefabs",
            "Assets/Prefabs/HM3D/",
            ""
        );
        string[] folders = Directory.GetDirectories(path_models);

        string save_path = EditorUtility.OpenFolderPanel("Save Path", "Assets/Tmp/", "");

        for (int i = 0; i < folders.Length; i++)
        {
            var tmp = folders[i].Replace("\\", "/").Split("/");
            var current_save_path = Path.Combine(save_path, tmp[tmp.Length - 1]).Replace("\\", "/");
            print(current_save_path);

            Directory.CreateDirectory(current_save_path);

            string current_folder = "Assets/" + folders[i].Replace("\\", "/").Split("Assets/")[1];
            var files = Directory.GetFiles(current_folder, "*.fbx"); //[0].Replace("\\", "/");
            foreach (string file in files)
            {
                print(file);
                tmp = file.Replace("\\", "/").Split("/");
                string filename_save = tmp[tmp.Length - 1].Replace(".fbx", ".glb");
                Debug.Log(filename_save);
                object_to_convert = GameObject.Instantiate(
                    AssetDatabase.LoadAssetAtPath<GameObject>(file)
                );
                var export = new GameObjectExport(exportSettings);
                GameObject[] gameObjectArray = { object_to_convert };
                // Add a scene
                export.AddScene(gameObjectArray);
                export.SaveToFileAndDispose(Path.Combine(current_save_path, filename_save));
                DestroyImmediate(object_to_convert);
            }

        }
        GUIUtility.ExitGUI();
    }
}
