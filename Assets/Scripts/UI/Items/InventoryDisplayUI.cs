using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Havengard.Items;
using System.Collections.Generic;

namespace Havengard.UI
{
    /// <summary>
    /// Unified inventory display showing equipped items and cached items
    /// </summary>
    public class InventoryDisplayUI : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject panel;
        
        [Header("Equipped Items Section")]
        [SerializeField] private Transform equippedItemsParent;
        [SerializeField] private TextMeshProUGUI equippedCountText;
        
        [Header("Cached Items Section")]
        [SerializeField] private Transform cachedItemsParent;
        [SerializeField] private TextMeshProUGUI cachedCountText;
        [SerializeField] private TextMeshProUGUI celestiumText;
        
        [Header("Prefabs")]
        [SerializeField] private GameObject itemSlotPrefab;
        
        [Header("Buttons")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button sortByRarityButton;
        [SerializeField] private Button sortByTimeButton;
        [SerializeField] private Button sortByLevelButton;

        [Header("Tooltip")]
        [SerializeField] private ItemTooltipUI tooltip;

        private ItemInventory playerInventory;
        private List<ItemSlotUI> equippedSlots = new List<ItemSlotUI>();
        private List<ItemSlotUI> cachedSlots = new List<ItemSlotUI>();

        private void Start()
        {
            // Hook up buttons
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);
            
            if (sortByRarityButton != null)
                sortByRarityButton.onClick.AddListener(() => SortCache(SortType.Rarity));
            
            if (sortByTimeButton != null)
                sortByTimeButton.onClick.AddListener(() => SortCache(SortType.Time));
            
            if (sortByLevelButton != null)
                sortByLevelButton.onClick.AddListener(() => SortCache(SortType.Level));

            // Find player inventory
            playerInventory = FindFirstObjectByType<ItemInventory>();

            // Subscribe to events
            if (playerInventory != null)
            {
                playerInventory.OnInventoryChanged += OnEquippedInventoryChanged;
            }

            if (ItemCache.Instance != null)
            {
                ItemCache.Instance.OnItemAddedToCache += OnCacheChanged;
                ItemCache.Instance.OnItemRemovedFromCache += OnCacheChanged;
            }

            if (ItemManager.Instance != null)
            {
                ItemManager.Instance.OnCelestiumChanged += UpdateCelestiumDisplay;
            }

            Hide();
        }

        private void OnDestroy()
        {
            if (playerInventory != null)
            {
                playerInventory.OnInventoryChanged -= OnEquippedInventoryChanged;
            }

            if (ItemCache.Instance != null)
            {
                ItemCache.Instance.OnItemAddedToCache -= OnCacheChanged;
                ItemCache.Instance.OnItemRemovedFromCache -= OnCacheChanged;
            }

            if (ItemManager.Instance != null)
            {
                ItemManager.Instance.OnCelestiumChanged -= UpdateCelestiumDisplay;
            }
        }

        public void Show()
        {
            panel.SetActive(true);
            RefreshDisplay();
        }

        public void Hide()
        {
            panel.SetActive(false);
            if (tooltip != null)
                tooltip.Hide();
        }

        public void Toggle()
        {
            if (panel.activeSelf)
                Hide();
            else
                Show();
        }

        private void RefreshDisplay()
        {
            RefreshEquippedItems();
            RefreshCachedItems();
            UpdateCelestiumDisplay(ItemManager.Instance?.Celestium ?? 0);
        }

        /// <summary>
        /// Refresh equipped items grid
        /// </summary>
        private void RefreshEquippedItems()
        {
            // Clear existing slots
            foreach (var slot in equippedSlots)
            {
                if (slot != null)
                    Destroy(slot.gameObject);
            }
            equippedSlots.Clear();

            if (playerInventory == null) return;

            // Get equipped items
            var equippedItems = playerInventory.GetAllItems();
            
            foreach (var item in equippedItems)
            {
                CreateEquippedSlot(item);
            }

            // Update count text
            if (equippedCountText != null)
            {
                equippedCountText.text = $"Equipped: {equippedItems.Count}/{playerInventory.MaxItems}";
            }

            Debug.Log($"[InventoryDisplayUI] Refreshed {equippedItems.Count} equipped items");
        }

        /// <summary>
        /// Refresh cached items grid
        /// </summary>
        private void RefreshCachedItems()
        {
            // Clear existing slots
            foreach (var slot in cachedSlots)
            {
                if (slot != null)
                    Destroy(slot.gameObject);
            }
            cachedSlots.Clear();

            if (ItemCache.Instance == null) return;

            // Get cached items
            var cachedItems = ItemCache.Instance.CachedItems;
            
            foreach (var item in cachedItems)
            {
                CreateCachedSlot(item);
            }

            // Update count text
            if (cachedCountText != null)
            {
                cachedCountText.text = $"Cache: {cachedItems.Count}/100";
            }

            Debug.Log($"[InventoryDisplayUI] Refreshed {cachedItems.Count} cached items");
        }

        private void CreateEquippedSlot(ItemInstance item)
        {
            GameObject slotObj = Instantiate(itemSlotPrefab, equippedItemsParent);
            ItemSlotUI slot = slotObj.GetComponent<ItemSlotUI>();
            
            if (slot != null)
            {
                slot.SetItem(item);
                slot.OnSlotClicked += OnEquippedSlotClicked;
                slot.OnSlotHoverEnter += OnSlotHoverEnter;
                slot.OnSlotHoverExit += OnSlotHoverExit;
                equippedSlots.Add(slot);
            }
        }

        private void CreateCachedSlot(ItemInstance item)
        {
            GameObject slotObj = Instantiate(itemSlotPrefab, cachedItemsParent);
            ItemSlotUI slot = slotObj.GetComponent<ItemSlotUI>();
            
            if (slot != null)
            {
                slot.SetItem(item);
                slot.OnSlotClicked += OnCachedSlotClicked;
                slot.OnSlotHoverEnter += OnSlotHoverEnter;
                slot.OnSlotHoverExit += OnSlotHoverExit;
                cachedSlots.Add(slot);
            }
        }

        private void OnEquippedSlotClicked(ItemSlotUI slot)
        {
            Debug.Log($"[InventoryDisplayUI] Equipped item clicked: {slot.CurrentItem?.itemData.itemName}");
            // Could show options: Unequip, View Details, etc.
        }

        private void OnCachedSlotClicked(ItemSlotUI slot)
        {
            Debug.Log($"[InventoryDisplayUI] Cached item clicked: {slot.CurrentItem?.itemData.itemName}");
            // Could show options: Equip, Disenchant, etc.
        }

        private void OnSlotHoverEnter(ItemSlotUI slot)
        {
            if (tooltip != null && slot.CurrentItem != null)
            {
                tooltip.Show(slot.CurrentItem);
            }
        }

        private void OnSlotHoverExit(ItemSlotUI slot)
        {
            if (tooltip != null)
            {
                tooltip.Hide();
            }
        }

        private void OnEquippedInventoryChanged()
        {
            if (panel.activeSelf)
            {
                RefreshEquippedItems();
            }
        }

        private void OnCacheChanged(ItemInstance item)
        {
            if (panel.activeSelf)
            {
                RefreshCachedItems();
            }
        }

        private void UpdateCelestiumDisplay(int amount)
        {
            if (celestiumText != null)
            {
                celestiumText.text = $"Celestium: {amount}";
            }
        }

        private enum SortType { Rarity, Time, Level }

        private void SortCache(SortType sortType)
        {
            if (ItemCache.Instance == null) return;

            switch (sortType)
            {
                case SortType.Rarity:
                    ItemCache.Instance.SortByRarity();
                    break;
                case SortType.Time:
                    ItemCache.Instance.SortByTime();
                    break;
                case SortType.Level:
                    ItemCache.Instance.SortByLevel();
                    break;
            }

            RefreshCachedItems();
        }
    }
}