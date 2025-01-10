using System.Collections.Generic;
using UnityEngine;

public class CopyTransformHand : MonoBehaviour
{
    public GameObject Target;
    public List<Transform> Hand_t,
        Target_t;

    private void LateUpdate()
    {
        if (Target == null)
            return;

        int idx = 0;
        foreach (Transform t_reference in Target_t)
        {
            Hand_t[idx].position = t_reference.position;
            Hand_t[idx].eulerAngles = t_reference.eulerAngles;
            idx++;
        }
    }

    public void SetHandTarget(GameObject HandTarget)
    {
        Target = HandTarget;

        Hand_t = new List<Transform>();
        Target_t = new List<Transform>();

        foreach (Transform child in Target.GetComponentsInChildren<Transform>())
        {
            Transform target_t = Utils.RecursiveFindChild(transform, child.name).transform;

            if (target_t != null)
            {
                Target_t.Add(child);
                Hand_t.Add(target_t);
            }
        }
    }
}
