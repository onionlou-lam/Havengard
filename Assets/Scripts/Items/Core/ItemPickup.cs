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

        public void Initialize(ItemData data)
        {
            _itemData = data;
            
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

            // Try to add to inventory
            var inventory = player.GetComponent<ItemInventory>();
            if (inventory != null)
            {
                bool success = inventory.AddItem(this._itemData);
                if (success)
                {
                    // Play pickup effects
                    if (this._itemData.pickupVFX != null)
                    {
                        Instantiate(this._itemData.pickupVFX, transform.position, Quaternion.identity);
                    }

                    if (this._itemData.pickupSFX != null)
                    {
                        AudioSource.PlayClipAtPoint(this._itemData.pickupSFX, transform.position);
                    }

                    Debug.Log($"[ItemPickup] Player collected: {this._itemData.itemName}");
                    Destroy(gameObject);
                }
                else
                {
                    isBeingCollected = false;
                    Debug.Log($"[ItemPickup] Inventory full or item rejected");
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, pickupRadius);
        }
    }
}