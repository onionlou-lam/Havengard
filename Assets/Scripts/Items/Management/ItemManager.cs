using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Havengard.Items
{
    public class ItemManager : MonoBehaviour
    {
        public static ItemManager Instance { get; private set; }

        private int celestium = 0;

        public int Celestium => celestium;

        public event Action<int> OnCelestiumChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void AddCelestium(int amount)
        {
            celestium += amount;
            OnCelestiumChanged?.Invoke(celestium);
            Debug.Log($"[ItemManager] Awarded {amount} Celestium. Total: {celestium}");
        }

        public bool SpendCelestium(int amount)
        {
            if (celestium < amount)
            {
                Debug.LogWarning($"[ItemManager] Not enough Celestium! Need {amount}, have {celestium}");
                return false;
            }

            celestium -= amount;
            OnCelestiumChanged?.Invoke(celestium);
            Debug.Log($"[ItemManager] Spent {amount} Celestium. Remaining: {celestium}");
            return true;
        }

        public void OnItemCollected(ItemInstance item)
        {
            Debug.Log($"[ItemManager] Item collected: {item}");
        }

        /// <summary>
        /// Get all game objects that have ItemInventory components
        /// </summary>
        public List<GameObject> GetAllCharactersWithInventories()
        {
            var inventories = FindObjectsByType<ItemInventory>(FindObjectsSortMode.None);
            return inventories.Select(inv => inv.gameObject).ToList();
        }

        /// <summary>
        /// Assign an item from cache to a character's inventory
        /// </summary>
        public bool AssignItemToCharacter(ItemInstance item, GameObject character)
        {
            if (item == null || character == null)
            {
                Debug.LogWarning("[ItemManager] Cannot assign item: null item or character");
                return false;
            }

            var inventory = character.GetComponent<ItemInventory>();
            if (inventory == null)
            {
                Debug.LogWarning($"[ItemManager] {character.name} has no ItemInventory component");
                return false;
            }

            // Try to add to inventory
            bool success = inventory.TryAddItem(item);
            if (success)
            {
                // Remove from cache
                ItemCache.Instance?.RemoveItem(item);
                Debug.Log($"[ItemManager] Assigned {item} to {character.name}");
            }

            return success;
        }
    }
}