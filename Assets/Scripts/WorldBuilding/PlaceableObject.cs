using UnityEngine;

public class PlaceableObject : MonoBehaviour
{
    [SerializeField] private Vector3 boxOffset = Vector3.zero;
    [SerializeField] private Vector3 inputBoxSize = new Vector3(1f, 1f, 0f);
    [SerializeField] private Color gizmoColor = Color.green;
    [Tooltip("Set to the grid cell size")] public float cellSize = .32f;
    [SerializeField, Tooltip("Debug only field")] private Vector3 boxSize;

    public Vector3 getBuildingCenter()
    {
        return boxOffset;
    }

    void OnValidate()
    {
        inputBoxSize.x = Mathf.Round(inputBoxSize.x);
        inputBoxSize.y = Mathf.Round(inputBoxSize.y);
        inputBoxSize.z = Mathf.Round(inputBoxSize.z);
        boxSize = new Vector3(
            cellSize * Mathf.Round(inputBoxSize.x),
            cellSize * Mathf.Round(inputBoxSize.y),
            cellSize * Mathf.Round(inputBoxSize.z)
        );
    }

    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireCube(transform.position + boxOffset, boxSize);
    }
}
