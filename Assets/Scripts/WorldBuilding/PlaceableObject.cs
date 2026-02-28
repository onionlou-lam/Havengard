using UnityEngine;

public class PlaceableObject : MonoBehaviour
{
    [SerializeField] private Vector3 boxOffset = Vector3.zero;
    [SerializeField] private Vector3 boxSize = new Vector3(1f, 1f, 0f);
    [SerializeField] private Color gizmoColor = Color.green;

    public Vector3 getBuildingCenter()
    {
        return boxOffset;
    }

    void OnValidate()
    {
        boxSize = new Vector3(
            Mathf.Round(boxSize.x),
            Mathf.Round(boxSize.y),
            Mathf.Round(boxSize.z)
        );
    }

    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireCube(transform.position + boxOffset, boxSize);
    }
}
