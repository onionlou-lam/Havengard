using UnityEngine;

public class FixedCameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 10f, -10f);
    public Vector3 fixedRotation = new Vector3(35f, 10f, 0f);

    void LateUpdate()
    {
        if (target == null) return;

        transform.position = target.position + offset;
        transform.rotation = Quaternion.Euler(fixedRotation);
    }
}