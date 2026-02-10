using UnityEngine;
using System.Collections;
using Havengard.UI; // Add this

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
        private int itemLevel = 1;

        public void Initialize(ItemData data, int level = 1)
        {
            _itemData = data;
            itemLevel = level;
            
            if (spriteRenderer != null && data != null && data.icon != null)
            {
                spriteRenderer.sprite = data.icon;
                spriteRenderer.color = data.rarityColor;
            }
            
            if (data != null && data.pickupVFX != null)
            {
                GameObject vfx = Instantiate(data.pickupVFX, transform.position, Quaternion.identity);
                
                // Auto-destroy VFX after particle system completes
                var ps = vfx.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    Destroy(vfx, ps.main.duration + ps.main.startLifetime.constantMax);
                }
                else
                {
                    Destroy(vfx, 2f);
                }
            }
        }

        private void Start()
        {
            startPosition = transform.position;
            spawnTime = Time.time;
            
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
            
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * _bobSpeed) * _bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
            
            float age = Time.time - spawnTime;
            if (age > fadeStartTime && spriteRenderer != null)
            {
                float fadeProgress = (age - fadeStartTime) / (lifetime - fadeStartTime);
                Color color = spriteRenderer.color;
                color.a = 1f - fadeProgress;
                spriteRenderer.color = color;
            }
            
            if (!isBeingCollected && playerTransform == null)
            {
                FindNearbyPlayer();
            }
            
            if (playerTransform != null && !isBeingCollected)
            {
                float distance = Vector3.Distance(transform.position, playerTransform.position);
                if (distance <= pickupRadius)
                {
                    transform.position = Vector3.MoveTowards(
                        transform.position, 
                        playerTransform.position, 
                        magnetSpeed * Time.deltaTime
                    );
                }
                else
                {
                    playerTransform = null;
                }
            }
        }

        private void FindNearbyPlayer()
        {
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
            
            if (other.CompareTag("Player"))
            {
                CollectItem(other.gameObject);
            }
        }

        private void CollectItem(GameObject player)
        {
            if (this._itemData == null) return;

            isBeingCollected = true;

            ItemInstance newItem = new ItemInstance(this._itemData, itemLevel);

            var inventory = player.GetComponent<ItemInventory>();
            bool addedToInventory = false;
            
            if (inventory != null)
            {
                addedToInventory = inventory.TryAddItem(newItem);
            }

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

            // Spawn floating pickup text
            Debug.Log($"[ItemPickup] Attempting to spawn pickup number. Spawner exists: {ItemPickupNumberSpawner.Instance != null}");
            if (ItemPickupNumberSpawner.Instance != null)
            {
                ItemPickupNumberSpawner.Instance.SpawnPickupNumber(this._itemData, transform.position, itemLevel);
            }
            else
            {
                Debug.LogWarning("[ItemPickup] ItemPickupNumberSpawner.Instance is null!");
            }

            if (this._itemData.pickupVFX != null)
            {
                GameObject vfx = Instantiate(this._itemData.pickupVFX, transform.position, Quaternion.identity);
                
                // Auto-destroy VFX after particle system completes
                var ps = vfx.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    Destroy(vfx, ps.main.duration + ps.main.startLifetime.constantMax);
                }
                else
                {
                    // Fallback if no ParticleSystem component found
                    Destroy(vfx, 2f);
                }
            }

            if (this._itemData.pickupSFX != null)
            {
                AudioSource.PlayClipAtPoint(this._itemData.pickupSFX, transform.position);
            }

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