using UnityEngine;
using System.Collections.Generic;

namespace Havengard.Items
{
    /// <summary>
    /// Component that handles dropping items when an enemy dies
    /// </summary>
    public class ItemDropper : MonoBehaviour
    {
        [Header("Drop Settings")]
        [SerializeField] private ItemDropTable dropTable;
        [SerializeField] private GameObject itemPickupPrefab;
        [SerializeField] private float dropRadius = 1f;
        [SerializeField] private bool dropOnDeath = true;

        [Header("Guaranteed Drops")]
        [SerializeField] private List<ItemData> guaranteedDrops = new List<ItemData>();

        [Header("Spacing & Collision")]
        [Tooltip("Minimum distance between dropped items")]
        [SerializeField] private float minItemSpacing = 0.5f;
        [Tooltip("Maximum attempts to find a non-overlapping position")]
        [SerializeField] private int maxSpacingAttempts = 15;
        [Tooltip("Check radius for detecting nearby items")]
        [SerializeField] private float overlapCheckRadius = 0.3f;
        [Tooltip("Layer mask for walls/obstacles to avoid")]
        [SerializeField] private LayerMask obstacleLayerMask;

        // Track all active item positions to prevent overlap
        private static List<Vector3> activeItemPositions = new List<Vector3>();

        private void Start()
        {
            // Validate configuration
            if (dropTable == null)
            {
                Debug.LogWarning($"[ItemDropper] {gameObject.name} has no drop table assigned!");
            }
            
            if (itemPickupPrefab == null)
            {
                Debug.LogWarning($"[ItemDropper] {gameObject.name} has no item pickup prefab assigned!");
            }
            else
            {
                // Verify the prefab has ItemPickup component
                var pickup = itemPickupPrefab.GetComponent<ItemPickup>();
                if (pickup == null)
                {
                    Debug.LogError($"[ItemDropper] Item pickup prefab on {gameObject.name} is missing ItemPickup component!");
                }
            }

            // Set default obstacle mask if not set
            if (obstacleLayerMask == 0)
            {
                obstacleLayerMask = LayerMask.GetMask("Walls", "Default");
            }
        }

        /// <summary>
        /// Try to drop an item at the specified position
        /// </summary>
        public void TryDropItem(Vector3 position)
        {
            if (dropTable == null)
            {
                Debug.LogWarning($"[ItemDropper] {name} has no drop table assigned");
                return;
            }

            if (itemPickupPrefab == null)
            {
                Debug.LogWarning($"[ItemDropper] {name} has no item pickup prefab assigned");
                return;
            }

            // Drop guaranteed items first
            foreach (var guaranteedItem in guaranteedDrops)
            {
                if (guaranteedItem != null)
                {
                    Debug.Log($"[ItemDropper] Dropping guaranteed item: {guaranteedItem.itemName}");
                    Vector3 safePosition = FindSafeSpawnPosition(position);
                    SpawnItemPickup(guaranteedItem, safePosition);
                }
            }

            // Try random drop from table
            ItemData droppedItem = dropTable.RollForItem();
            if (droppedItem != null)
            {
                Debug.Log($"[ItemDropper] Rolled item from table: {droppedItem.itemName}");
                Vector3 safePosition = FindSafeSpawnPosition(position);
                SpawnItemPickup(droppedItem, safePosition);
            }
            else
            {
                Debug.Log($"[ItemDropper] No item dropped from table (drop chance roll failed or no valid items)");
            }
        }

        /// <summary>
        /// Find a position that doesn't overlap with other items or obstacles
        /// </summary>
        private Vector3 FindSafeSpawnPosition(Vector3 basePosition)
        {
            Vector3 bestPosition = basePosition;
            
            // First try the base position
            if (IsPositionValid(basePosition))
            {
                return basePosition;
            }

            // Try to find a valid position in a spiral pattern
            for (int attempt = 0; attempt < maxSpacingAttempts; attempt++)
            {
                // Calculate angle and distance for spiral pattern
                float angle = attempt * 137.5f; // Golden angle for good distribution
                float distance = minItemSpacing + (attempt * 0.2f);
                
                // Add randomness to avoid perfect grid
                angle += Random.Range(-15f, 15f);
                distance += Random.Range(-0.1f, 0.1f);

                Vector2 offset = new Vector2(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * distance,
                    Mathf.Sin(angle * Mathf.Deg2Rad) * distance
                );

                Vector3 testPosition = basePosition + new Vector3(offset.x, offset.y, 0);

                if (IsPositionValid(testPosition))
                {
                    return testPosition;
                }
            }

            // If we couldn't find a valid position, return one slightly offset from base
            Vector2 fallbackOffset = Random.insideUnitCircle * dropRadius;
            return basePosition + new Vector3(fallbackOffset.x, fallbackOffset.y, 0);
        }

        /// <summary>
        /// Check if a position is valid (no overlaps with items or obstacles)
        /// </summary>
        private bool IsPositionValid(Vector3 position)
        {
            // Check for overlap with walls/obstacles
            Collider2D obstacleHit = Physics2D.OverlapCircle(position, overlapCheckRadius, obstacleLayerMask);
            if (obstacleHit != null)
            {
                return false;
            }

            // Check for overlap with other items
            foreach (Vector3 itemPos in activeItemPositions)
            {
                float distance = Vector3.Distance(position, itemPos);
                if (distance < minItemSpacing)
                {
                    return false;
                }
            }

            return true;
        }

        private void SpawnItemPickup(ItemData itemData, Vector3 position)
        {
            if (itemPickupPrefab == null)
            {
                Debug.LogWarning("[ItemDropper] No item pickup prefab assigned");
                return;
            }

            // Spawn pickup
            GameObject pickupObj = Instantiate(itemPickupPrefab, position, Quaternion.identity);
            ItemPickup pickup = pickupObj.GetComponent<ItemPickup>();
            
            if (pickup != null)
            {
                pickup.Initialize(itemData);
                
                // Register this position
                activeItemPositions.Add(position);
                
                // Add tracking component to remove position when picked up
                var tracker = pickupObj.AddComponent<ItemPositionTracker>();
                tracker.Initialize(position);
                
                Debug.Log($"[ItemDropper] Successfully spawned {itemData.itemName} at {position}");
            }
            else
            {
                Debug.LogError("[ItemDropper] Item pickup prefab missing ItemPickup component!");
                Destroy(pickupObj);
            }
        }

        /// <summary>
        /// Remove a position from the tracking list (called when item is picked up or destroyed)
        /// </summary>
        public static void UnregisterItemPosition(Vector3 position)
        {
            activeItemPositions.Remove(position);
        }

        /// <summary>
        /// Clear all tracked positions (useful for scene transitions)
        /// </summary>
        public static void ClearAllTrackedPositions()
        {
            activeItemPositions.Clear();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, dropRadius);
            
            // Draw spacing visualization
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, minItemSpacing);
        }
    }

    /// <summary>
    /// Helper component to track and unregister item positions when they're destroyed
    /// </summary>
    public class ItemPositionTracker : MonoBehaviour
    {
        private Vector3 trackedPosition;

        public void Initialize(Vector3 position)
        {
            trackedPosition = position;
        }

        private void OnDestroy()
        {
            ItemDropper.UnregisterItemPosition(trackedPosition);
        }
    }
}