using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Havengard.Items
{
    public class ItemInventory : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int maxItems = 6;
        
        [Header("References")]
        [SerializeField] private GameObject character; // The character to apply effects to
        
        // Item slot structure: ItemData + current level
        [System.Serializable]
        public class ItemSlot
        {
            public ItemData itemData;
            public int currentLevel = 1;
            
            public ItemSlot(ItemData data, int level = 1)
            {
                itemData = data;
                currentLevel = level;
            }
        }
        
        private List<ItemSlot> items = new List<ItemSlot>();
        
        // Events
        public event Action<ItemData, int> OnItemAdded;
        public event Action<ItemData, int> OnItemLevelUp;
        public event Action<ItemData> OnItemRemoved;
        public event Action OnInventoryChanged;
        
        public List<ItemSlot> Items => items;
        public int MaxItems => maxItems;
        public bool IsFull => items.Count >= maxItems;

        private void Awake()
        {
            if (character == null)
                character = gameObject;
        }

        /// <summary>
        /// Try to add an ItemInstance to inventory
        /// </summary>
        public bool TryAddItem(ItemInstance itemInstance)
        {
            if (itemInstance == null || itemInstance.itemData == null)
            {
                Debug.LogWarning("[ItemInventory] Cannot add null item instance");
                return false;
            }

            // Check if item already exists
            ItemSlot existingSlot = FindItemSlot(itemInstance.itemData);
            
            if (existingSlot != null)
            {
                // Stack/upgrade existing item
                if (itemInstance.itemData.autoStackOnPickup && existingSlot.currentLevel < itemInstance.itemData.maxLevel)
                {
                    // Remove old level effects
                    RemoveItemEffects(existingSlot.itemData, existingSlot.currentLevel);
                    
                    // Increase level (can increase by multiple levels if itemInstance.level > 1)
                    int targetLevel = Mathf.Min(existingSlot.currentLevel + itemInstance.level, itemInstance.itemData.maxLevel);
                    existingSlot.currentLevel = targetLevel;
                    
                    // Apply new level effects
                    ApplyItemEffects(existingSlot.itemData, existingSlot.currentLevel);
                    
                    OnItemLevelUp?.Invoke(itemInstance.itemData, existingSlot.currentLevel);
                    OnInventoryChanged?.Invoke();
                    
                    Debug.Log($"[ItemInventory] {itemInstance.itemData.itemName} upgraded to level {existingSlot.currentLevel}");
                    return true;
                }
                else
                {
                    Debug.Log($"[ItemInventory] {itemInstance.itemData.itemName} is already at max level");
                    return false;
                }
            }
            else
            {
                // Add new item
                if (IsFull)
                {
                    Debug.Log($"[ItemInventory] Inventory is full!");
                    return false;
                }
                
                ItemSlot newSlot = new ItemSlot(itemInstance.itemData, itemInstance.level);
                items.Add(newSlot);
                
                // Apply effects
                ApplyItemEffects(itemInstance.itemData, itemInstance.level);
                
                OnItemAdded?.Invoke(itemInstance.itemData, itemInstance.level);
                OnInventoryChanged?.Invoke();
                
                Debug.Log($"[ItemInventory] Added new item: {itemInstance.itemData.itemName}");
                return true;
            }
        }

        /// <summary>
        /// Add item to inventory or upgrade existing (legacy method for ItemData)
        /// </summary>
        public bool AddItem(ItemData itemData)
        {
            if (itemData == null) return false;
            
            // Create ItemInstance and use TryAddItem
            ItemInstance instance = new ItemInstance(itemData, 1);
            return TryAddItem(instance);
        }

        /// <summary>
        /// Remove item from inventory by ItemInstance
        /// </summary>
        public bool RemoveItem(ItemInstance itemInstance)
        {
            if (itemInstance == null || itemInstance.itemData == null)
                return false;

            return RemoveItem(itemInstance.itemData);
        }

        /// <summary>
        /// Remove item from inventory by ItemData
        /// </summary>
        public bool RemoveItem(ItemData itemData)
        {
            ItemSlot slot = FindItemSlot(itemData);
            if (slot == null) return false;
            
            // Remove effects
            RemoveItemEffects(slot.itemData, slot.currentLevel);
            
            items.Remove(slot);
            
            OnItemRemoved?.Invoke(itemData);
            OnInventoryChanged?.Invoke();
            
            Debug.Log($"[ItemInventory] Removed item: {itemData.itemName}");
            return true;
        }

        /// <summary>
        /// Get all items as ItemInstance list
        /// </summary>
        public List<ItemInstance> GetAllItems()
        {
            return items.Select(slot => new ItemInstance(slot.itemData, slot.currentLevel)).ToList();
        }

        /// <summary>
        /// Find item slot by ItemData
        /// </summary>
        private ItemSlot FindItemSlot(ItemData itemData)
        {
            return items.Find(slot => slot.itemData == itemData);
        }

        /// <summary>
        /// Apply all effects from an item at a specific level
        /// </summary>
        private void ApplyItemEffects(ItemData itemData, int level)
        {
            if (itemData == null || itemData.effects == null) return;
            
            foreach (var effect in itemData.effects)
            {
                if (effect != null)
                {
                    effect.Apply(character, level);
                }
            }
        }

        /// <summary>
        /// Remove all effects from an item at a specific level
        /// </summary>
        private void RemoveItemEffects(ItemData itemData, int level)
        {
            if (itemData == null || itemData.effects == null) return;
            
            foreach (var effect in itemData.effects)
            {
                if (effect != null)
                {
                    effect.Remove(character, level);
                }
            }
        }

        /// <summary>
        /// Get total count of unique items
        /// </summary>
        public int GetItemCount()
        {
            return items.Count;
        }

        /// <summary>
        /// Check if inventory has a specific item
        /// </summary>
        public bool HasItem(ItemData itemData)
        {
            return FindItemSlot(itemData) != null;
        }

        /// <summary>
        /// Get item level (0 if not owned)
        /// </summary>
        public int GetItemLevel(ItemData itemData)
        {
            ItemSlot slot = FindItemSlot(itemData);
            return slot != null ? slot.currentLevel : 0;
        }

        /// <summary>
        /// Clear all items (for debug/reset)
        /// </summary>
        public void ClearInventory()
        {
            foreach (var slot in items)
            {
                RemoveItemEffects(slot.itemData, slot.currentLevel);
            }
            items.Clear();
            OnInventoryChanged?.Invoke();
        }
    }
}