using UnityEngine;

namespace Havengard.Waves
{
    /// <summary>
    /// Visualizes spawn zones in the editor
    /// </summary>
    public class SpawnZoneVisualizer : MonoBehaviour
    {
        [Header("Visualization")]
        [SerializeField] private Color spawnZoneColor = new Color(1f, 0.5f, 0f, 0.3f);
        [SerializeField] private Color spawnZoneOutlineColor = new Color(1f, 0.5f, 0f, 1f);
        [SerializeField] private float spawnRadius = 2f;
        [SerializeField] private bool showLabel = true;
        [SerializeField] private bool showArrow = true;

        [Header("Settings")]
        [Tooltip("Direction enemies should face when spawned (optional)")]
        [SerializeField] private Vector3 spawnDirection = Vector3.right;

        private void OnDrawGizmos()
        {
            DrawSpawnZone(false);
        }

        private void OnDrawGizmosSelected()
        {
            DrawSpawnZone(true);
        }

        private void DrawSpawnZone(bool selected)
        {
            // Draw spawn circle
            Gizmos.color = selected ? spawnZoneOutlineColor : spawnZoneColor;

            // Filled circle
            DrawCircle(transform.position, spawnRadius, 32);

            // Outline circle
            Gizmos.color = spawnZoneOutlineColor;
            DrawCircleOutline(transform.position, spawnRadius, 32);

            // Draw arrow showing spawn direction
            if (showArrow)
            {
                Vector3 arrowStart = transform.position;
                Vector3 arrowEnd = arrowStart + spawnDirection.normalized * spawnRadius;
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(arrowStart, arrowEnd);

                // Arrow head
                Vector3 right = Vector3.Cross(spawnDirection.normalized, Vector3.forward).normalized * 0.3f;
                Gizmos.DrawLine(arrowEnd, arrowEnd - spawnDirection.normalized * 0.5f + right);
                Gizmos.DrawLine(arrowEnd, arrowEnd - spawnDirection.normalized * 0.5f - right);
            }

            // Draw label
            if (showLabel && selected)
            {
#if UNITY_EDITOR
                UnityEditor.Handles.Label(
                    transform.position + Vector3.up * (spawnRadius + 0.5f),
                    $"Spawn Zone: {gameObject.name}",
                    new GUIStyle()
                    {
                        normal = new GUIStyleState() { textColor = Color.white },
                        fontSize = 12,
                        fontStyle = FontStyle.Bold
                    }
                );
#endif
            }
        }

        private void DrawCircle(Vector3 center, float radius, int segments)
        {
            float angleStep = 360f / segments;
            for (int i = 0; i < segments; i++)
            {
                float angle1 = i * angleStep * Mathf.Deg2Rad;
                float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;

                Vector3 p1 = center + new Vector3(Mathf.Cos(angle1), Mathf.Sin(angle1), 0) * radius;
                Vector3 p2 = center + new Vector3(Mathf.Cos(angle2), Mathf.Sin(angle2), 0) * radius;
                Vector3 p3 = center;

                // Draw triangle to fill
                Gizmos.DrawLine(p1, p2);
                Gizmos.DrawLine(p2, p3);
                Gizmos.DrawLine(p3, p1);
            }
        }

        private void DrawCircleOutline(Vector3 center, float radius, int segments)
        {
            float angleStep = 360f / segments;
            for (int i = 0; i < segments; i++)
            {
                float angle1 = i * angleStep * Mathf.Deg2Rad;
                float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;

                Vector3 p1 = center + new Vector3(Mathf.Cos(angle1), Mathf.Sin(angle1), 0) * radius;
                Vector3 p2 = center + new Vector3(Mathf.Cos(angle2), Mathf.Sin(angle2), 0) * radius;

                Gizmos.DrawLine(p1, p2);
            }
        }

        public Vector3 SpawnDirection => spawnDirection.normalized;
        public float SpawnRadius => spawnRadius;
    }
}