using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine.Events;
using UnityEngine.Perception.GroundTruth;
using UnityEngine.Perception.Randomization.Samplers;

namespace UnityEngine.Perception.Randomization.Scenarios
{
    [RequireComponent(typeof(SceneManager))]
    [AddComponentMenu("Perception/Scenarios/CustomScenario")]
    public class CustomScenario : PerceptionScenario<CustomScenario.Constants>
    {
        /// <summary>
        /// Constants describing the execution of this scenario
        /// </summary>
        [Serializable]
        public class Constants : ScenarioConstants
        {
            /// <summary>
            /// The index of the first iteration to execute. The random seed for the randomizers in an iteration are
            /// determined by the global <see cref="ScenarioConstants.randomSeed"/> and the iteration index.
            /// </summary>
            [Tooltip("The index of the first iteration to execute.")]
            public int startIteration;

            /// <summary>
            /// The number of iterations to run, starting at startIteration.
            /// </summary>
            [Tooltip("The number of iterations to run.")]
            public int iterationCount = 100;

            [Tooltip("Probability interaction in the current iteraction.")]
            [Range(0, 1)]
            public float probInteraction = 0.5f;

            [Tooltip("Probability other objects in scene in the current iteraction.")]
            [Range(0, 1)]
            public float probOtherObjects = 0.8f;

            [Tooltip("Probability two interactions in the current iteraction.")]
            [Range(0, 1)]
            public float probTwoInteractions = 0.5f;

            public bool manualNextIteraction = false;

            public float timeForCapture = 0.3f;
        }

        bool finishIteraction = false;

        [HideInInspector]
        public bool scenarioComplete = false;

        public List<ICustomRandomizer> iCustomRandomizers;

        [HideInInspector]
        public List<GameObject> currentActiveObjects;

        SceneManager sceneManager;

        [HideInInspector]
        public UnityEvent newIteraction;

        //[HideInInspector]
        //public PerceptionCamera perceptionCamera;

        [HideInInspector]
        public PerceptionCamera[] perceptionCameras;

        public enum ScenarioState
        {
            Idle,
            Record
        };

        public ScenarioState scenarioState;

        float timeIt;

        protected override void OnAwake()
        {
            base.OnAwake();
            iCustomRandomizers = new List<ICustomRandomizer>();
            currentIteration = constants.startIteration;
            foreach (var randomizer in this.randomizers)
            {
                if (!typeof(ICustomRandomizer).IsAssignableFrom(randomizer.GetType()))
                    continue;
                if (randomizer.enabled)
                    iCustomRandomizers.Add((ICustomRandomizer)randomizer);
            }

            perceptionCameras = FindObjectsByType<PerceptionCamera>(FindObjectsSortMode.None);
        }

        protected override void OnStart()
        {
            base.OnStart();
            sceneManager = GetComponent<SceneManager>();
        }

        protected override void OnIterationStart()
        {
            newIteraction.Invoke();
            scenarioState = ScenarioState.Idle;
            timeIt = Time.unscaledTime;
            Debug.Log($"Iteraction Start: {currentIteration}");
        }

        public void StartCapture()
        {
            scenarioState = ScenarioState.Record;
            Invoke(nameof(RequestCapture), constants.timeForCapture);
        }

        public void RequestCapture()
        {
            foreach (PerceptionCamera perceptionCamera in perceptionCameras)
                perceptionCamera.RequestCapture();

            //if (!constants.manualNextIteraction)
            //    Invoke(nameof(NextIteraction), 0.2f);
            //NextIteraction();
            Invoke(nameof(NextIteraction), 0.3f);
        }

        public void NextIteraction() => finishIteraction = true;

        protected override void OnIterationEnd()
        {
            finishIteraction = false;
            sceneManager.Reset();
            float endtime = Time.unscaledTime;
            Debug.Log($"Iteraction End: {(endtime - timeIt) * 1000.0f} ms.");
            //Resources.UnloadUnusedAssets();
            //EditorUtility.UnloadUnusedAssetsImmediate();
        }

        //protected override bool isIterationComplete => customState == CustomState.End || (maxTimeIteration -= Time.deltaTime) < 0;

        protected override bool isIterationComplete => finishIteraction;
        protected override bool isScenarioComplete =>
            currentIteration >= constants.iterationCount || scenarioComplete;

        public bool AllCustomRandomizersReady()
        {
            bool ready = true;
            foreach (ICustomRandomizer iCustomRandomizer in iCustomRandomizers)
            {
                ready = ready && iCustomRandomizer.ready;
            }
            return ready;
        }

        public GameObject GetActiveHuman()
        {
            foreach (var randomizer in this.randomizers)
            {
                if (!typeof(HumansRandomizer).IsAssignableFrom(randomizer.GetType()))
                    continue;
                return ((HumansRandomizer)randomizer).GetActiveHuman();
            }
            return null;
        }

        public void DestroyCurrentEnv()
        {
            foreach (var randomizer in this.randomizers)
            {
                if (!typeof(EnvironmentRandomizer).IsAssignableFrom(randomizer.GetType()))
                    continue;
                ((EnvironmentRandomizer)randomizer).DestroyCurrentEnv();
            }
        }

        public GameObject GetObjectsContainer()
        {
            foreach (var randomizer in this.randomizers)
            {
                if (!typeof(ObjectRandomizer).IsAssignableFrom(randomizer.GetType()))
                    continue;
                return ((ObjectRandomizer)randomizer).m_Container;
            }
            return null;
        }

        public void OpenPrefabPathEnv()
        {
            var allowedExtensions = new[] { ".fbx", ".prefab", ".gltf" };
            EnvironmentRandomizer environmentRandomizer = null;
            ManualEnvironmentRandomizer manualEnvironmentRandomizer = null;

            foreach (var randomizer in this.randomizers)
            {
                if (typeof(EnvironmentRandomizer).IsAssignableFrom(randomizer.GetType()))
                    environmentRandomizer = (EnvironmentRandomizer)randomizer;
                if (typeof(ManualEnvironmentRandomizer).IsAssignableFrom(randomizer.GetType()))
                    manualEnvironmentRandomizer = (ManualEnvironmentRandomizer)randomizer;
            }

            string path_prefab = EditorUtility.OpenFolderPanel("Path Models", "", "");
            string[] folders = Directory.GetDirectories(path_prefab);

            if (environmentRandomizer != null)
                environmentRandomizer.prefabs_paths = new List<string>();
            if (manualEnvironmentRandomizer != null)
                manualEnvironmentRandomizer.prefabs_paths = new List<string>();

            for (int i = 0; i < folders.Length; i++)
            {
                string current_folder = folders[i].Replace("\\", "/").Split("Assets/")[1];
                List<string> files = Directory
                    .GetFiles("Assets/" + current_folder)
                    .Where(file => allowedExtensions.Any(file.ToLower().EndsWith))
                    .ToList();
                foreach (var file in files)
                {
                    string current_file = file.Replace("\\", "/");
                    var filename = current_file.Split("/").Last();
                    var dir_ = current_file.Split("/").SkipLast(1).Last();
                    environmentRandomizer?.prefabs_paths.Add(current_file);
                    manualEnvironmentRandomizer?.prefabs_paths.Add(current_file);
                }
            }
            if (environmentRandomizer != null)
                environmentRandomizer.uniformSampler.range = new FloatRange(
                    0,
                    environmentRandomizer.prefabs_paths.Count
                );
            GUIUtility.ExitGUI();
        }

        public void OpenPrefabPathObj()
        {
            var allowedExtensions = new[] { ".fbx", ".prefab" };

            ObjectRandomizer objectRandomizer = null;
            foreach (var randomizer in this.randomizers)
            {
                if (!typeof(ObjectRandomizer).IsAssignableFrom(randomizer.GetType()))
                    continue;
                objectRandomizer = (ObjectRandomizer)randomizer;
            }

            string path_prefab = EditorUtility.OpenFolderPanel("Path Models", "", "");
            string[] folders = Directory.GetDirectories(path_prefab);

            objectRandomizer.prefabs_paths = new List<string>();

            for (int i = 0; i < folders.Length; i++)
            {
                string current_folder = folders[i].Replace("\\", "/").Split("Assets/")[1];
                List<string> files = Directory
                    .GetFiles("Assets/" + current_folder)
                    .Where(file => allowedExtensions.Any(file.ToLower().EndsWith))
                    .ToList();
                foreach (var file in files)
                {
                    string current_file = file.Replace("\\", "/");
                    var filename = current_file.Split("/").Last();
                    var dir_ = current_file.Split("/").SkipLast(1).Last();
                    objectRandomizer.prefabs_paths.Add(current_file);
                }
            }
            objectRandomizer.uniformSampler.range = new FloatRange(
                0,
                objectRandomizer.prefabs_paths.Count
            );

            GUIUtility.ExitGUI();
        }
    }
}
