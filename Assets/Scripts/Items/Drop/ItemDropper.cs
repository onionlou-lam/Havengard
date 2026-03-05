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
        }

        /// <summary>
        /// Try to drop an item at the specified position
        /// </summary>
        public void TryDropItem(Vector3 position)
        {
            //Debug.Log($"[ItemDropper] TryDropItem called on {gameObject.name} at position {position}");
            
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
                    SpawnItemPickup(guaranteedItem, position);
                }
            }

            // Try random drop from table
            ItemData droppedItem = dropTable.RollForItem();
            if (droppedItem != null)
            {
                Debug.Log($"[ItemDropper] Rolled item from table: {droppedItem.itemName}");
                SpawnItemPickup(droppedItem, position);
            }
            else
            {
                Debug.Log($"[ItemDropper] No item dropped from table (drop chance roll failed or no valid items)");
            }
        }

        private void SpawnItemPickup(ItemData itemData, Vector3 position)
        {
            if (itemPickupPrefab == null)
            {
                Debug.LogWarning("[ItemDropper] No item pickup prefab assigned");
                return;
            }

            // Add random offset within radius
            Vector2 randomOffset = Random.insideUnitCircle * dropRadius;
            Vector3 spawnPosition = position + new Vector3(randomOffset.x, randomOffset.y, 0);

            // Spawn pickup
            GameObject pickupObj = Instantiate(itemPickupPrefab, spawnPosition, Quaternion.identity);
            ItemPickup pickup = pickupObj.GetComponent<ItemPickup>();
            
            if (pickup != null)
            {
                pickup.Initialize(itemData);
                Debug.Log($"[ItemDropper] Successfully spawned {itemData.itemName} at {spawnPosition}");
            }
            else
            {
                Debug.LogError("[ItemDropper] Item pickup prefab missing ItemPickup component!");
                Destroy(pickupObj);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, dropRadius);
        }
    }
}