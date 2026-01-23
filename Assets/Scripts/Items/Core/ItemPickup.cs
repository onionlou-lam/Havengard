using UnityEngine;
using System.Collections;

namespace Havengard.Items
{
    [RequireComponent(typeof(Collider2D))]
    public class ItemPickup : MonoBehaviour
    {
        [Header("Item Data")]
        [SerializeField] private ItemData _itemData;
        
        [Header("Visuals")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float _bobSpeed = 2f;
        [SerializeField] private float _bobHeight = 0.3f;
        [SerializeField] private float rotationSpeed = 90f;
        
        [Header("Pickup")]
        [SerializeField] private float pickupRadius = 1.5f;
        [SerializeField] private float magnetSpeed = 5f;
        [SerializeField] private LayerMask playerLayer;
        
        [Header("Lifetime")]
        [SerializeField] private float lifetime = 30f;
        [SerializeField] private float fadeStartTime = 25f;
        
        private Vector3 startPosition;
        private float spawnTime;
        private Transform playerTransform;
        private bool isBeingCollected = false;
        private int itemLevel = 1; // Store the level

        public void Initialize(ItemData data, int level = 1)
        {
            _itemData = data;
            itemLevel = level;
            
            if (spriteRenderer != null && data != null && data.icon != null)
            {
                spriteRenderer.sprite = data.icon;
                spriteRenderer.color = data.rarityColor;
            }
            
            // Play spawn VFX
            if (data != null && data.pickupVFX != null)
            {
                Instantiate(data.pickupVFX, transform.position, Quaternion.identity);
            }
        }

        private void Start()
        {
            startPosition = transform.position;
            spawnTime = Time.time;
            
            // Setup collider
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
            
            // Auto-destroy after lifetime
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            // Bobbing animation
            float newY = startPosition.y + Mathf.Sin(Time.time * _bobSpeed) * _bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            
            // Rotation
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
            
            // Fade near end of lifetime
            float age = Time.time - spawnTime;
            if (age > fadeStartTime && spriteRenderer != null)
            {
                float fadeProgress = (age - fadeStartTime) / (lifetime - fadeStartTime);
                Color color = spriteRenderer.color;
                color.a = 1f - fadeProgress;
                spriteRenderer.color = color;
            }
            
            // Magnet towards player
            if (!isBeingCollected && playerTransform == null)
            {
                FindNearbyPlayer();
            }
            
            if (playerTransform != null && !isBeingCollected)
            {
                float distance = Vector3.Distance(transform.position, playerTransform.position);
                if (distance <= pickupRadius)
                {
                    // Move towards player
                    transform.position = Vector3.MoveTowards(
                        transform.position, 
                        playerTransform.position, 
                        magnetSpeed * Time.deltaTime
                    );
                }
                else
                {
                    playerTransform = null; // Lost player
                }
            }
        }

        private void FindNearbyPlayer()
        {
            // Find player within pickup radius
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pickupRadius, playerLayer);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    playerTransform = hit.transform;
                    break;
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isBeingCollected) return;
            
            // Check if it's the player
            if (other.CompareTag("Player"))
            {
                CollectItem(other.gameObject);
            }
        }

        private void CollectItem(GameObject player)
        {
            if (this._itemData == null) return;

            isBeingCollected = true;

            // Create ItemInstance with level
            ItemInstance newItem = new ItemInstance(this._itemData, itemLevel);

            // Try to add to player's inventory first
            var inventory = player.GetComponent<ItemInventory>();
            bool addedToInventory = false;
            
            if (inventory != null)
            {
                addedToInventory = inventory.TryAddItem(newItem);
            }

            // If couldn't add to inventory, add to cache
            if (!addedToInventory)
            {
                if (ItemCache.Instance != null)
                {
                    ItemCache.Instance.AddItem(newItem);
                    Debug.Log($"[ItemPickup] Item sent to cache: {this._itemData.itemName}");
                }
                else
                {
                    Debug.LogWarning("[ItemPickup] ItemCache.Instance is null! Item will be lost.");
                }
            }

            // Play pickup effects
            if (this._itemData.pickupVFX != null)
            {
                Instantiate(this._itemData.pickupVFX, transform.position, Quaternion.identity);
            }

            if (this._itemData.pickupSFX != null)
            {
                AudioSource.PlayClipAtPoint(this._itemData.pickupSFX, transform.position);
            }

            // Notify ItemManager
            if (ItemManager.Instance != null)
            {
                ItemManager.Instance.OnItemCollected(newItem);
            }

            Debug.Log($"[ItemPickup] Player collected: {this._itemData.itemName} (AddedToInventory: {addedToInventory})");
            Destroy(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, pickupRadius);
        }
    }
}