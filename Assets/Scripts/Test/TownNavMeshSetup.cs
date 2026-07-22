using UnityEngine;
using NavMeshPlus.Components;
using NavMeshPlus.Extensions;

namespace Havengard.Town
{
    /// <summary>
    /// NavMesh setup for town scene - compatible with Navigation Surface component
    /// </summary>
    public class TownNavMeshSetup : MonoBehaviour
    {
        [Header("Manual Bake Control")]
        [SerializeField]
        [Tooltip("Click to bake the NavMesh manually")]
        private bool manualBake = false;

        private void OnValidate()
        {
            if (manualBake)
            {
                manualBake = false;
                BakeNavMesh();
            }
        }

        [ContextMenu("Bake NavMesh")]
        public void BakeNavMesh()
        {
            // Find the NavMeshSurface component (works with "Navigation Surface" display name too)
            NavMeshSurface surface = GetComponent<NavMeshSurface>();
            
            if (surface == null)
            {
                // Try finding it in children or by interface
                surface = GetComponentInChildren<NavMeshSurface>();
            }

            if (surface == null)
            {
                Debug.LogError("[TownNavMeshSetup] Cannot find NavMeshSurface component! Make sure this GameObject has a NavMesh Surface component attached.");
                return;
            }

            Debug.Log("[TownNavMeshSetup] ========== BAKING NAVMESH ==========");
            Debug.Log($"  Mode: {surface.collectObjects}");
            Debug.Log($"  Geometry: {surface.useGeometry}");
            Debug.Log($"  Layers: {surface.layerMask.value}");

            // Clear and rebuild
            surface.RemoveData();
            surface.BuildNavMesh();
            
            // Verify
            if (surface.navMeshData != null)
            {
                Bounds bounds = surface.navMeshData.sourceBounds;
                
                if (bounds.size.magnitude > 0.01f)
                {
                    Debug.Log($"[TownNavMeshSetup] ✓✓✓ SUCCESS!");
                    Debug.Log($"  Bounds Center: {bounds.center}");
                    Debug.Log($"  Bounds Size: {bounds.size}");
                    Debug.Log($"  Extents: {bounds.extents}");
                    
                    // Check if NavMesh is too thin in Y
                    if (bounds.extents.y < 0.1f)
                    {
                        Debug.LogWarning($"[TownNavMeshSetup] ⚠ NavMesh is very thin in Y axis (extents.y = {bounds.extents.y})");
                        Debug.LogWarning("  This might cause 'Not on NavMesh' issues");
                        Debug.LogWarning("  Try switching Use Geometry to 'Render Meshes' instead of 'Physics Colliders'");
                    }
                }
                else
                {
                    Debug.LogError("[TownNavMeshSetup] ✗ FAILED - NavMesh has zero size!");
                }
            }
            else
            {
                Debug.LogError("[TownNavMeshSetup] ✗ FAILED - No NavMesh data generated!");
            }
        }

        [ContextMenu("Clear NavMesh")]
        public void ClearNavMesh()
        {
            NavMeshSurface surface = GetComponent<NavMeshSurface>();
            if (surface != null)
            {
                surface.RemoveData();
                Debug.Log("[TownNavMeshSetup] NavMesh cleared");
            }
        }

        [ContextMenu("Fix Player Position")]
        public void FixPlayerPosition()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogError("[TownNavMeshSetup] No player found with 'Player' tag!");
                return;
            }

            UnityEngine.AI.NavMeshAgent agent = player.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent == null)
            {
                Debug.LogError("[TownNavMeshSetup] Player has no NavMeshAgent!");
                return;
            }

            // Try to warp player to a valid NavMesh position
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(player.transform.position, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
            {
                player.transform.position = hit.position;
                Debug.Log($"[TownNavMeshSetup] ✓ Moved player to valid NavMesh position: {hit.position}");
                Debug.Log($"  Player is now on NavMesh: {agent.isOnNavMesh}");
            }
            else
            {
                Debug.LogError("[TownNavMeshSetup] ✗ Could not find valid NavMesh position near player!");
                Debug.LogError("  The NavMesh might not exist or player is too far from it");
            }
        }
    }
}