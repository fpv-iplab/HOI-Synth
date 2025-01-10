using UnityEngine;
using UnityEngine.Perception.GroundTruth;
using UnityEngine.Perception.Randomization.Scenarios;

public class ScannerEnvironmentsManager : SceneManager
{
    //private
    CustomScenario customScenario;
    public bool rotateHuman = false,
        rotate_object = false;
    public int numberOfRotations = 10;

    bool nextIt = false;

    public int currentNumber;

    void Start()
    {
        currentNumber = numberOfRotations;
        customScenario = (CustomScenario)ScenarioBase.activeScenario;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (
            customScenario.AllCustomRandomizersReady()
            && customScenario.scenarioState == CustomScenario.ScenarioState.Idle
        )
        {
            if (currentNumber > 0)
            {
                foreach (PerceptionCamera perceptionCamera in customScenario.perceptionCameras)
                    perceptionCamera.RequestCapture();
                if (rotateHuman)
                {
                    var human = UnityEngine
                        .Object.FindObjectsByType<CustomHumanTag>(FindObjectsSortMode.None)[0]
                        .gameObject;
                    human.transform.Rotate(Vector3.up * 360 / numberOfRotations);
                }
                if (rotate_object && currentNumber != numberOfRotations)
                {
                    var object_ = GameObject
                        .Find("ObjectContainer")
                        .transform.GetChild(0)
                        .gameObject;
                    object_.transform.Rotate(new Vector3(-60, -60, -60));
                }

                currentNumber--;
            }
            else
            {
                customScenario.NextIteraction();
                currentNumber = numberOfRotations;
            }
        }
    }

    public override void Reset() { }
}
