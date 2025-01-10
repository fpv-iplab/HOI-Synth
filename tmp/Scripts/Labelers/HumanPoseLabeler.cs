using System;
using UnityEngine;
using UnityEngine.Perception.GroundTruth;
using UnityEngine.Perception.GroundTruth.DataModel;
using UnityEngine.Rendering;

public class HumanPoseLabeler : CameraLabeler
{
    public override string description => "HumanPoseLabeler";

    public override string labelerId => "HumanPoseLabeler";

    protected override bool supportsVisualization => false;
    HumanPoseAnnotationDefinition humanPoseAnnotationDefinition;

    public class HumanPoseAnnotationDefinition : AnnotationDefinition
    {
        public HumanPoseAnnotationDefinition(string id)
            : base(id) { }

        public override string description => "Human Pose";

        public override string modelType => "HumanPoseAnnotationDefinition";
    }

    [Serializable]
    class HumanPoseAnnotation : Annotation
    {
        public HumanPoseAnnotation(
            AnnotationDefinition definition,
            string sensorId,
            Vector3 pos,
            Vector3 rot
        )
            : base(definition, sensorId)
        {
            position = pos;
            rotation = rot;
        }

        public Vector3 position,
            rotation;

        public override void ToMessage(IMessageBuilder builder)
        {
            base.ToMessage(builder);
            builder.AddFloatArray("position", MessageBuilderUtils.ToFloatVector(position));
            builder.AddFloatArray("rotation", MessageBuilderUtils.ToFloatVector(rotation));
        }

        public override bool IsValid() => true;
    }

    protected override void Setup()
    {
        humanPoseAnnotationDefinition = new HumanPoseAnnotationDefinition("humanPose");
        DatasetCapture.RegisterAnnotationDefinition(humanPoseAnnotationDefinition);
    }

    protected override void OnBeginRendering(ScriptableRenderContext scriptableRenderContext)
    {
        var human = UnityEngine
            .Object.FindObjectsByType<CustomHumanTag>(FindObjectsSortMode.None)[0]
            .gameObject;
        var position = human.transform.position;
        var rotation = human.transform.rotation.eulerAngles;

        //Report using the PerceptionCamera's SensorHandle if scheduled this frame
        var sensorHandle = perceptionCamera.SensorHandle;
        if (sensorHandle.ShouldCaptureThisFrame)
        {
            var annotation = new HumanPoseAnnotation(
                humanPoseAnnotationDefinition,
                sensorHandle.Id,
                position,
                rotation
            );
            sensorHandle.ReportAnnotation(humanPoseAnnotationDefinition, annotation);
        }
    }
}
