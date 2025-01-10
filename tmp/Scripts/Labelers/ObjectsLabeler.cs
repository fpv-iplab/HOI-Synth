using System;
using UnityEngine;
using UnityEngine.Perception.GroundTruth;
using UnityEngine.Perception.GroundTruth.DataModel;
using UnityEngine.Rendering;

public class ObjectsLabeler : CameraLabeler
{
    AnnotationDefinition objectAnnotationDefinition;

    public override string description => "ObjectNameLabeler";
    public override string labelerId => "ObjectNameLabeler";

    protected override bool supportsVisualization => false;

    public class ObjectAnnotationDefinition : AnnotationDefinition
    {
        public ObjectAnnotationDefinition(string id)
            : base(id) { }

        public override string description => "Object name";

        public override string modelType => "ObjectAnnotationDefinition";
    }

    [Serializable]
    class ObjectNameAnnotation : Annotation
    {
        public ObjectNameAnnotation(
            AnnotationDefinition definition,
            string sensorId,
            string objectN
        )
            : base(definition, sensorId)
        {
            objectName = objectN;
        }

        public string objectName;

        public override void ToMessage(IMessageBuilder builder)
        {
            base.ToMessage(builder);
            builder.AddString("objectName", objectName);
        }

        public override bool IsValid() => true;
    }

    protected override void Setup()
    {
        objectAnnotationDefinition = new ObjectAnnotationDefinition("object_name");
        DatasetCapture.RegisterAnnotationDefinition(objectAnnotationDefinition);
    }

    protected override void OnBeginRendering(ScriptableRenderContext scriptableRenderContext)
    {
        var objectName = GameObject.Find("ObjectContainer").transform.GetChild(0).name;
        //Report using the PerceptionCamera's SensorHandle if scheduled this frame
        var sensorHandle = perceptionCamera.SensorHandle;
        if (sensorHandle.ShouldCaptureThisFrame)
        {
            var annotation = new ObjectNameAnnotation(
                objectAnnotationDefinition,
                sensorHandle.Id,
                objectName
            );
            sensorHandle.ReportAnnotation(objectAnnotationDefinition, annotation);
        }
    }
}
