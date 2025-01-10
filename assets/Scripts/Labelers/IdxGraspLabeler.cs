using System;
using UnityEngine;
using UnityEngine.Perception.GroundTruth;
using UnityEngine.Perception.GroundTruth.DataModel;
using UnityEngine.Rendering;

public class IdxGraspLabeler : CameraLabeler
{
    AnnotationDefinition idxGraspAnnotationDefinition;

    public override string description => "IdxGraspLabeler";
    public override string labelerId => "IdxGraspLabeler";

    protected override bool supportsVisualization => false;

    public class IdxGraspAnnotationDefinition : AnnotationDefinition
    {
        public IdxGraspAnnotationDefinition(string id)
            : base(id) { }

        public override string description => "IdxGrasp";

        public override string modelType => "IdxGraspAnnotationDefinition";
    }

    [Serializable]
    class IdxGraspAnnotation : Annotation
    {
        public IdxGraspAnnotation(AnnotationDefinition definition, string sensorId, int idxGraspC)
            : base(definition, sensorId)
        {
            idxGrasp = idxGraspC - 1;
        }

        public int idxGrasp;

        public override void ToMessage(IMessageBuilder builder)
        {
            base.ToMessage(builder);
            builder.AddInt("idxGrasp", idxGrasp);
        }

        public override bool IsValid() => true;
    }

    protected override void Setup()
    {
        idxGraspAnnotationDefinition = new IdxGraspAnnotationDefinition("idxGrasp");
        DatasetCapture.RegisterAnnotationDefinition(idxGraspAnnotationDefinition);
    }

    protected override void OnBeginRendering(ScriptableRenderContext scriptableRenderContext)
    {
        var idxGrasp = GameObject
            .Find("Scenario")
            .GetComponent<SceneGraspManager>()
            .current_grasp_idx;
        //Report using the PerceptionCamera's SensorHandle if scheduled this frame
        var sensorHandle = perceptionCamera.SensorHandle;
        if (sensorHandle.ShouldCaptureThisFrame)
        {
            var annotation = new IdxGraspAnnotation(
                idxGraspAnnotationDefinition,
                sensorHandle.Id,
                idxGrasp
            );
            sensorHandle.ReportAnnotation(idxGraspAnnotationDefinition, annotation);
        }
    }
}
