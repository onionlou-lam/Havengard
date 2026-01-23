using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Havengard.Items
{
    /// <summary>
    /// Global cache for unassigned items collected during gameplay
    /// </summary>
    public class ItemCache : MonoBehaviour
    {
        private static ItemCache instance;
        public static ItemCache Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<ItemCache>();
                    if (instance == null)
                    {
                        GameObject obj = new GameObject("ItemCache");
                        instance = obj.AddComponent<ItemCache>();
                        DontDestroyOnLoad(obj);
                    }
                }
                return instance;
            }
        }

        [Header("Cache")]
        [SerializeField] private List<ItemInstance> cachedItems = new List<ItemInstance>();
        [SerializeField] private int maxCacheSize = 100;

        public IReadOnlyList<ItemInstance> CachedItems => cachedItems;
        public int Count => cachedItems.Count;

        public event System.Action<ItemInstance> OnItemAddedToCache;
        public event System.Action<ItemInstance> OnItemRemovedFromCache;
        public event System.Action OnCacheCleared;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Add an item to the cache
        /// </summary>
        public void AddItem(ItemInstance item)
        {
            if (cachedItems.Count >= maxCacheSize)
            {
                Debug.LogWarning($"[ItemCache] Cache is full! Max size: {maxCacheSize}");
                return;
            }

            cachedItems.Add(item);
            OnItemAddedToCache?.Invoke(item);
            Debug.Log($"[ItemCache] Added {item} to cache. Total: {cachedItems.Count}");
            Debug.Log($"[ItemCache] Item icon exists: {item.itemData.icon != null}");
        }

        /// <summary>
        /// Remove an item from cache
        /// </summary>
        public bool RemoveItem(ItemInstance item)
        {
            if (cachedItems.Remove(item))
            {
                OnItemRemovedFromCache?.Invoke(item);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Remove item by unique ID
        /// </summary>
        public bool RemoveItemByID(string uniqueID)
        {
            var item = cachedItems.FirstOrDefault(i => i.uniqueID == uniqueID);
            if (item != null)
            {
                return RemoveItem(item);
            }
            return false;
        }

        /// <summary>
        /// Disenchant an item for Celestium
        /// </summary>
        public int DisenchantItem(ItemInstance item)
        {
            if (!cachedItems.Contains(item)) return 0;

            int celestium = item.GetDisenchantValue();
            RemoveItem(item);
            
            // Award currency (integrate with your currency system)
            if (ItemManager.Instance != null)
            {
                ItemManager.Instance.AddCelestium(celestium);
            }

            Debug.Log($"[ItemCache] Disenchanted {item} for {celestium} Celestium");
            return celestium;
        }

        /// <summary>
        /// Clear all cached items
        /// </summary>
        public void ClearCache()
        {
            cachedItems.Clear();
            OnCacheCleared?.Invoke();
        }

        /// <summary>
        /// Get items by rarity
        /// </summary>
        public List<ItemInstance> GetItemsByRarity(ItemRarity rarity)
        {
            return cachedItems.Where(i => i.itemData.rarity == rarity).ToList();
        }

        /// <summary>
        /// Get items by type
        /// </summary>
        public List<ItemInstance> GetItemsByType(ItemType type)
        {
            return cachedItems.Where(i => i.itemData.itemType == type).ToList();
        }

        /// <summary>
        /// Sort cache by various criteria
        /// </summary>
        public void SortCache(System.Comparison<ItemInstance> comparison)
        {
            cachedItems.Sort(comparison);
        }

        /// <summary>
        /// Sort by rarity (descending)
        /// </summary>
        public void SortByRarity()
        {
            cachedItems.Sort((a, b) => b.itemData.rarity.CompareTo(a.itemData.rarity));
        }

        /// <summary>
        /// Sort by acquisition time (newest first)
        /// </summary>
        public void SortByTime()
        {
            cachedItems.Sort((a, b) => b.acquiredTime.CompareTo(a.acquiredTime));
        }

        /// <summary>
        /// Sort by level (highest first)
        /// </summary>
        public void SortByLevel()
        {
            cachedItems.Sort((a, b) => b.level.CompareTo(a.level));
        }
    }
}