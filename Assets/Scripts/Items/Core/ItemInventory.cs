using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using Havengard.UI;

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

            Debug.Log($"[ItemInventory] TryAddItem called: {itemInstance.itemData.itemName} to {gameObject.name}");
            Debug.Log($"[ItemInventory] Current item count BEFORE add: {items.Count}/{maxItems}");

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
                    Debug.Log($"[ItemInventory] Current item count AFTER upgrade: {items.Count}/{maxItems}");
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
                
                Debug.Log($"[ItemInventory] Item ADDED to list. New count: {items.Count}/{maxItems}");
                
                // Apply effects
                ApplyItemEffects(itemInstance.itemData, itemInstance.level);
                
                // Fire events
                Debug.Log($"[ItemInventory] Firing OnItemAdded event. Subscribers: {OnItemAdded?.GetInvocationList().Length ?? 0}");
                OnItemAdded?.Invoke(itemInstance.itemData, itemInstance.level);
                
                Debug.Log($"[ItemInventory] Firing OnInventoryChanged event. Subscribers: {OnInventoryChanged?.GetInvocationList().Length ?? 0}");
                OnInventoryChanged?.Invoke();
                
                Debug.Log($"[ItemInventory] Added new item: {itemInstance.itemData.itemName}. Final count: {items.Count}");
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

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                // Toggle the CACHE UI (stash)
                ItemCacheUI cacheUI = FindFirstObjectByType<ItemCacheUI>();
                cacheUI?.Toggle();
            }

            // for testing
            if (Input.GetKeyDown(KeyCode.T))
            {
                Debug.Log("=== TESTING ITEM SYSTEM ===");
                Debug.Log($"This Inventory ({gameObject.name}): {items.Count}/{maxItems}");
                
                if (items.Count > 0)
                {
                    Debug.Log("Items in THIS inventory:");
                    foreach (var slot in items)
                    {
                        Debug.Log($"  - {slot.itemData.itemName} Lv.{slot.currentLevel} Icon={(slot.itemData.icon != null ? slot.itemData.icon.name : "NULL")}");
                    }
                }
                else
                {
                    Debug.LogWarning("  >>> NO ITEMS IN THIS INVENTORY! <<<");
                }

                // Find ALL inventories
                var allInventories = FindObjectsByType<ItemInventory>(FindObjectsSortMode.None);
                Debug.Log($"Found {allInventories.Length} ItemInventory components in scene");
                foreach (var inv in allInventories)
                {
                    Debug.Log($"  Inventory on: {inv.gameObject.name}, Items: {inv.items.Count}");
                    if (inv.items.Count > 0)
                    {
                        foreach (var slot in inv.items)
                        {
                            Debug.Log($"    > {slot.itemData.itemName}");
                        }
                    }
                }

                // Find and log ItemSlotUI states
                var slots = FindObjectsByType<ItemSlotUI>(FindObjectsSortMode.None);
                Debug.Log($"Found {slots.Length} ItemSlotUI components");
                foreach (var slot in slots)
                {
                    string itemName = slot.CurrentItem?.itemData.itemName ?? "null";
                    string iconName = slot.CurrentItem?.itemData.icon != null ? slot.CurrentItem.itemData.icon.name : "null";
                    string parent = slot.transform.parent != null ? slot.transform.parent.name : "NO PARENT";
                    Debug.Log($"  Slot ({slot.gameObject.name}) in [{parent}]: Empty={slot.IsEmpty}, Item={itemName}, Icon={iconName}");
                }
                
                // Check InventoryUI
                var invUI = FindFirstObjectByType<InventoryUI>();
                if (invUI != null)
                {
                    Debug.Log($"[InventoryUI] Panel Active: {invUI.gameObject.activeSelf}");
                }
                else
                {
                    Debug.LogWarning("NO InventoryUI FOUND!");
                }
            }
            
            // Press 'U' to show the InventoryUI
            if (Input.GetKeyDown(KeyCode.U))
            {
                var invUI = FindFirstObjectByType<InventoryUI>();
                if (invUI != null)
                {
                    invUI.Show(this);
                    Debug.Log("[ItemInventory] Opened InventoryUI");
                }
            }
        }
    }
}