using UnityEngine;
using UnityEngine.Perception.Randomization.Parameters;
using UnityEngine.Perception.Randomization.Scenarios;

public class CustomCameraController : MonoBehaviour
{
    public Transform Head;
    public Vector3 positionOffset;
    public Vector3 targetOffsetPos = new Vector3(0, -0.25f, 1f);
    Vector3 tmpOffset = Vector3.zero;
    Transform targetLookObject;
    GameObject tmpTarget;

    public bool lookAtTarget = false;
    public bool smooth = false;

    Vector3 RelaxGaze;
    Vector3 velocity = Vector3.zero;
    CustomScenario customScenario;
    public Vector3Parameter gazeRange;

    private void Awake()
    {
        targetLookObject = new GameObject("Camera Target").transform;
        if (Head)
            RelaxGaze = Head.TransformPoint(targetOffsetPos + tmpOffset);
    }

    void Start()
    {
        customScenario = (CustomScenario)ScenarioBase.activeScenario;
        customScenario.newIteraction.AddListener(RandomizeGazePose);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (Head)
        {
            transform.position = Head.position;
            transform.Translate(positionOffset, Head);

            if (!lookAtTarget)
            {
                RelaxGaze = Head.TransformPoint(targetOffsetPos + tmpOffset);
                targetLookObject.position = smooth
                    ? Vector3.SmoothDamp(targetLookObject.position, RelaxGaze, ref velocity, 0.3f)
                    : RelaxGaze;
            }
            else
            {
                targetLookObject.transform.position = tmpTarget.transform.position;
            }
            transform.LookAt(targetLookObject);
        }
    }

    [ContextMenu("Set Camera Position In Editor")]
    public void SetCameraPositionInEditor()
    {
        RelaxGaze = Head.TransformPoint(0, -0.25f, 1f);
        transform.position = Head.position;
        transform.Translate(positionOffset, Head);
        transform.LookAt(RelaxGaze);
    }

    public void SetTarget(GameObject target)
    {
        lookAtTarget = true;
        tmpTarget = target;
    }

    public void RandomizeGazePose() => tmpOffset = gazeRange.Sample();
}
