using UnityEngine;

public class RotateJoint : MonoBehaviour
{
    public float angle;
#pragma warning disable CS0436 // Type conflicts with imported type
    [InspectorButton("Rotate")]
#pragma warning restore CS0436 // Type conflicts with imported type
    public bool rotate;

    public void Rotate()
    {
        transform.Rotate(new Vector3(0, 0, Mathf.Rad2Deg * angle), Space.Self);
    }
}
