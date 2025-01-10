using System;
using UnityEngine;
using UnityEngine.Perception.GroundTruth;
using UnityEngine.Perception.GroundTruth.DataModel;
using UnityEngine.Rendering;

public class EnvironmentLabeler : CameraLabeler
{
    AnnotationDefinition environmentAnnotationDefinition;

    public override string description => "EnvironmentNameLabeler";
    public override string labelerId => "EnvironmentNameLabeler";

    protected override bool supportsVisualization => false;

    public class EnvironmentAnnotationDefinition : AnnotationDefinition
    {
        public EnvironmentAnnotationDefinition(string id)
            : base(id) { }

        public override string description => "Environment name";

        public override string modelType => "EnvironmentAnnotationDefinition";
    }

    [Serializable]
    class EnvironmentNameAnnotation : Annotation
    {
        public EnvironmentNameAnnotation(
            AnnotationDefinition definition,
            string sensorId,
            string environmentN
        )
            : base(definition, sensorId)
        {
            environmentName = environmentN;
        }

        public string environmentName;

        public override void ToMessage(IMessageBuilder builder)
        {
            base.ToMessage(builder);
            builder.AddString("environmentName", environmentName);
        }

        public override bool IsValid() => true;
    }

    protected override void Setup()
    {
        environmentAnnotationDefinition = new EnvironmentAnnotationDefinition("environment_name");
        DatasetCapture.RegisterAnnotationDefinition(environmentAnnotationDefinition);
    }

    protected override void OnBeginRendering(ScriptableRenderContext scriptableRenderContext)
    {
        var environmentName = GameObject.Find("EnvironmentContainer").transform.GetChild(0).name;
        //Report using the PerceptionCamera's SensorHandle if scheduled this frame
        var sensorHandle = perceptionCamera.SensorHandle;
        if (sensorHandle.ShouldCaptureThisFrame)
        {
            var annotation = new EnvironmentNameAnnotation(
                environmentAnnotationDefinition,
                sensorHandle.Id,
                environmentName
            );
            sensorHandle.ReportAnnotation(environmentAnnotationDefinition, annotation);
        }
    }
}
