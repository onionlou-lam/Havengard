using UnityEngine;
using UnityEngine.AI;
using NavMeshPlus.Components;
using UnityEngine.Tilemaps;

namespace Havengard.Town
{
    /// <summary>
    /// Diagnostic tool to help troubleshoot NavMesh issues.
    /// Attach this temporarily to diagnose problems.
    /// </summary>
    public class NavMeshDiagnostics : MonoBehaviour
    {
        [ContextMenu("Run Full Diagnostics")]
        public void RunDiagnostics()
        {
            Debug.Log("========== NAVMESH DIAGNOSTICS ==========");

            CheckNavMeshSurface();
            CheckColliders();
            CheckTilemaps();
            CheckPlayer();
            CheckLayers();

            Debug.Log("========== DIAGNOSTICS COMPLETE ==========");
        }

        private void CheckNavMeshSurface()
        {
            Debug.Log("\n--- NavMeshSurface Check ---");
            NavMeshSurface[] surfaces = FindObjectsOfType<NavMeshSurface>();

            if (surfaces.Length == 0)
            {
                Debug.LogError("✗ NO NavMeshSurface found in scene!");
                return;
            }

            foreach (var surface in surfaces)
            {
                Debug.Log($"✓ Found NavMeshSurface on: {surface.gameObject.name}");
                Debug.Log($"  - Rotation: {surface.transform.eulerAngles}");

                if (!Mathf.Approximately(surface.transform.eulerAngles.x, 270f))
                {
                    Debug.LogError($"  ✗ WRONG ROTATION! Should be (270, 0, 0), is {surface.transform.eulerAngles}");
                }
                else
                {
                    Debug.Log("  ✓ Rotation correct (270, 0, 0)");
                }

                Debug.Log($"  - Agent Type ID: {surface.agentTypeID}");
                Debug.Log($"  - Collect Objects: {surface.collectObjects}");
                Debug.Log($"  - Use Geometry: {surface.useGeometry}");
                Debug.Log($"  - Layer Mask: {surface.layerMask.value}");
                Debug.Log($"  - Has NavMesh Data: {surface.navMeshData != null}");

                if (surface.navMeshData != null)
                {
                    Debug.Log($"  ✓ NavMesh Data exists - Bounds: {surface.navMeshData.sourceBounds}");
                }
                else
                {
                    Debug.LogError("  ✗ No NavMesh Data! Try baking the NavMesh.");
                }
            }
        }

        private void CheckColliders()
        {
            Debug.Log("\n--- Collider2D Check ---");
            BoxCollider2D[] boxColliders = FindObjectsOfType<BoxCollider2D>();
            CompositeCollider2D[] compositeColliders = FindObjectsOfType<CompositeCollider2D>();

            Debug.Log($"Found {boxColliders.Length} BoxCollider2D");
            Debug.Log($"Found {compositeColliders.Length} CompositeCollider2D");

            if (boxColliders.Length == 0 && compositeColliders.Length == 0)
            {
                Debug.LogError("✗ NO Collider2D found! NavMesh needs colliders to bake from.");
            }
            else
            {
                Debug.Log("✓ Colliders present");

                // Sample first few colliders
                int count = Mathf.Min(5, boxColliders.Length);
                for (int i = 0; i < count; i++)
                {
                    Debug.Log($"  - {boxColliders[i].gameObject.name}: Layer={LayerMask.LayerToName(boxColliders[i].gameObject.layer)}, Trigger={boxColliders[i].isTrigger}");
                }
            }
        }

        private void CheckTilemaps()
        {
            Debug.Log("\n--- Tilemap Check ---");
            TilemapCollider2D[] tilemapColliders = FindObjectsOfType<TilemapCollider2D>();

            Debug.Log($"Found {tilemapColliders.Length} TilemapCollider2D");

            if (tilemapColliders.Length == 0)
            {
                Debug.LogWarning("⚠ No TilemapCollider2D found");
            }
            else
            {
                foreach (var tc in tilemapColliders)
                {
                    Tilemap tm = tc.GetComponent<Tilemap>();
                    Debug.Log($"  ✓ {tc.gameObject.name}:");
                    Debug.Log($"    - Layer: {LayerMask.LayerToName(tc.gameObject.layer)}");
                    Debug.Log($"    - Used By Composite: {tc.usedByComposite}");
                    if (tm != null)
                    {
                        Debug.Log($"    - Tile Count: {tm.GetUsedTilesCount()}");
                        Debug.Log($"    - Cell Bounds: {tm.cellBounds}");
                    }
                }
            }
        }

        private void CheckPlayer()
        {
            Debug.Log("\n--- Player Check ---");
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player == null)
            {
                Debug.LogError("✗ Player not found! Make sure player has 'Player' tag.");
                return;
            }

            Debug.Log($"✓ Player found: {player.name}");

            NavMeshAgent agent = player.GetComponent<NavMeshAgent>();
            if (agent == null)
            {
                Debug.LogError("  ✗ Player missing NavMeshAgent component!");
            }
            else
            {
                Debug.Log("  ✓ NavMeshAgent present");
                Debug.Log($"    - Agent Type ID: {agent.agentTypeID}");
                Debug.Log($"    - Is On NavMesh: {agent.isOnNavMesh}");
                Debug.Log($"    - Radius: {agent.radius}");
                Debug.Log($"    - Height: {agent.height}");
                Debug.Log($"    - Update Rotation: {agent.updateRotation}");
                Debug.Log($"    - Update Up Axis: {agent.updateUpAxis}");

                if (!agent.isOnNavMesh)
                {
                    Debug.LogError("  ✗ Agent is NOT on NavMesh! This will prevent movement.");
                }
            }

            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Debug.Log($"  ✓ Rigidbody2D: Type={rb.bodyType}");
                if (rb.bodyType != RigidbodyType2D.Kinematic)
                {
                    Debug.LogWarning("  ⚠ Rigidbody2D should be Kinematic when using NavMeshAgent!");
                }
            }
        }

        private void CheckLayers()
        {
            Debug.Log("\n--- Layer Check ---");
            Debug.Log("Defined Layers:");
            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(layerName))
                {
                    Debug.Log($"  Layer {i}: {layerName}");
                }
            }
        }

        [ContextMenu("Force Visualize NavMesh")]
        public void ForceVisualizeNavMesh()
        {
            NavMeshSurface surface = FindObjectOfType<NavMeshSurface>();
            if (surface == null || surface.navMeshData == null)
            {
                Debug.LogError("No NavMesh data to visualize!");
                return;
            }

            Debug.Log("NavMesh data found. Check Scene view with Gizmos enabled.");
            Debug.Log($"NavMesh Bounds: {surface.navMeshData.sourceBounds}");
            Debug.Log($"NavMesh Position: {surface.navMeshData.position}");
        }
    }
}