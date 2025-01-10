using System.Collections.Generic;
using System.Linq;
using UnityEngine.Perception.GroundTruth.DataModel;

public class InteractionMetric : Metric
{
    public List<InteractionAnnotation> m_Values;

    public override bool IsValid()
    {
        return base.IsValid() && m_Values != null;
    }

    public override T[] GetValues<T>()
    {
        return m_Values.Cast<T>().ToArray();
    }

    public InteractionMetric(
        List<InteractionAnnotation> values,
        MetricDefinition definition,
        string sensorId = default,
        string annotationId = default
    )
        : base(definition, sensorId, annotationId)
    {
        m_Values = values;
    }

    public override void ToMessage(IMessageBuilder builder)
    {
        base.ToMessage(builder);
        int idx_ann = 0;
        foreach (var ann in m_Values)
        {
            builder.AddIntArray(idx_ann.ToString(), new int[] { ann.hand, ann.activeObject });
            idx_ann++;
        }
    }
}

public struct InteractionAnnotation
{
    public InteractionAnnotation(int h, int o)
    {
        hand = h;
        activeObject = o;
    }

    public int hand;
    public int activeObject;
}
