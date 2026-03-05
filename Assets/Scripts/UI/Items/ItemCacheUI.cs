using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Havengard.Items;

namespace Havengard.UI
{
    /// <summary>
    /// UI panel for managing the item cache and viewing equipped items
    /// </summary>
    public class ItemCacheUI : MonoBehaviour
    {
        [Header("Panel References")]
        [SerializeField] private GameObject panel;

        [Header("Equipped Items Section")]
        [SerializeField] private Transform equippedItemsParent; // EquippedItemsGrid
        [SerializeField] private TextMeshProUGUI equippedCountText;
        [SerializeField] private GameObject equippedItemSlotPrefab;

        [Header("Cached Items Section")]
        [SerializeField] private Transform cachedItemsParent; // ItemInventory_Grid (in scroll view)
        [SerializeField] private TextMeshProUGUI itemCountText;
        [SerializeField] private GameObject cachedItemSlotPrefab;

        [Header("Common")]
        [SerializeField] private TextMeshProUGUI celestiumText;
        [SerializeField] private ItemTooltipUI tooltip;

        [Header("Buttons")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button sortByRarityButton;
        [SerializeField] private Button sortByTimeButton;
        [SerializeField] private Button sortByLevelButton;
        [SerializeField] private Button backButton;

        [Header("Filter")]
        [SerializeField] private TMP_Dropdown rarityFilterDropdown;
        [SerializeField] private TMP_Dropdown typeFilterDropdown;

        [Header("Player Reference")]
        [SerializeField] private ItemInventory playerInventory;

        private List<ItemSlotUI> equippedItemSlots = new List<ItemSlotUI>();
        private List<ItemSlotUI> cachedItemSlots = new List<ItemSlotUI>();
        private ItemSlotUI selectedSlot;

        private void Start()
        {
            // Find player inventory if not assigned
            if (playerInventory == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerInventory = player.GetComponent<ItemInventory>();
                }
            }

            // Hook up buttons
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            if (backButton != null)
                backButton.onClick.AddListener(Hide);

            if (sortByRarityButton != null)
                sortByRarityButton.onClick.AddListener(() => SortCache(SortType.Rarity));

            if (sortByTimeButton != null)
                sortByTimeButton.onClick.AddListener(() => SortCache(SortType.Time));

            if (sortByLevelButton != null)
                sortByLevelButton.onClick.AddListener(() => SortCache(SortType.Level));

            // Subscribe to cache events
            if (ItemCache.Instance != null)
            {
                ItemCache.Instance.OnItemAddedToCache += OnCacheChanged;
                ItemCache.Instance.OnItemRemovedFromCache += OnCacheChanged;
                ItemCache.Instance.OnCacheCleared += RefreshDisplay;
            }

            // Subscribe to inventory events
            if (playerInventory != null)
            {
                playerInventory.OnInventoryChanged += RefreshEquippedItems;
            }

            // Subscribe to currency events
            if (ItemManager.Instance != null)
            {
                ItemManager.Instance.OnCelestiumChanged += UpdateCelestiumDisplay;
            }

            Hide();
        }

        private void OnDestroy()
        {
            if (ItemCache.Instance != null)
            {
                ItemCache.Instance.OnItemAddedToCache -= OnCacheChanged;
                ItemCache.Instance.OnItemRemovedFromCache -= OnCacheChanged;
                ItemCache.Instance.OnCacheCleared -= RefreshDisplay;
            }

            if (playerInventory != null)
            {
                playerInventory.OnInventoryChanged -= RefreshEquippedItems;
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
        /// Refresh the equipped items section (from player inventory)
        /// </summary>
        private void RefreshEquippedItems()
        {
            // Clear existing slots
            foreach (var slot in equippedItemSlots)
            {
                if (slot != null)
                    Destroy(slot.gameObject);
            }
            equippedItemSlots.Clear();

            if (playerInventory == null || equippedItemsParent == null) return;

            // Get items from player's inventory
            var equippedItems = playerInventory.GetAllItems();
            int maxSlots = playerInventory.MaxItems;

            // Create slots for max inventory size
            for (int i = 0; i < maxSlots; i++)
            {
                GameObject slotObj = Instantiate(
                    equippedItemSlotPrefab ?? cachedItemSlotPrefab,
                    equippedItemsParent
                );

                ItemSlotUI slot = slotObj.GetComponent<ItemSlotUI>();
                if (slot != null)
                {
                    if (i < equippedItems.Count)
                    {
                        // Show equipped item
                        slot.SetItem(equippedItems[i]);
                    }
                    else
                    {
                        // Empty slot
                        slot.ClearSlot();
                    }

                    slot.OnSlotClicked += OnEquippedSlotClicked;
                    slot.OnSlotHoverEnter += OnSlotHoverEnter;
                    slot.OnSlotHoverExit += OnSlotHoverExit;
                    equippedItemSlots.Add(slot);
                }
            }

            // Update count text
            if (equippedCountText != null)
            {
                equippedCountText.text = $"Equipped Items: {equippedItems.Count}/{maxSlots}";
            }

            Debug.Log($"[ItemCacheUI] Refreshed {equippedItems.Count} equipped items ({maxSlots} slots)");
        }

        /// <summary>
        /// Refresh the cached items section (from item cache)
        /// </summary>
        private void RefreshCachedItems()
        {
            // Clear existing cached slots
            foreach (var slot in cachedItemSlots)
            {
                if (slot != null)
                    Destroy(slot.gameObject);
            }
            cachedItemSlots.Clear();

            if (ItemCache.Instance == null || cachedItemsParent == null) return;

            // Create slots for cached items
            foreach (var item in ItemCache.Instance.CachedItems)
            {
                CreateCachedSlot(item);
            }

            // Update count
            if (itemCountText != null)
            {
                int count = ItemCache.Instance.Count;
                int max = 100;
                itemCountText.text = $"Cache: {count}/{max}";
            }

            Debug.Log($"[ItemCacheUI] Refreshed {cachedItemSlots.Count} cached items");
        }

        private void CreateCachedSlot(ItemInstance item)
        {
            if (cachedItemSlotPrefab == null || cachedItemsParent == null) return;

            GameObject slotObj = Instantiate(cachedItemSlotPrefab, cachedItemsParent);
            ItemSlotUI slot = slotObj.GetComponent<ItemSlotUI>();

            if (slot != null)
            {
                slot.SetItem(item);
                slot.OnSlotClicked += OnCachedSlotClicked;
                slot.OnSlotHoverEnter += OnSlotHoverEnter;
                slot.OnSlotHoverExit += OnSlotHoverExit;
                cachedItemSlots.Add(slot);
            }
        }

        private void OnEquippedSlotClicked(ItemSlotUI slot)
        {
            if (slot.IsEmpty) return;
            selectedSlot = slot;
            Debug.Log($"[ItemCacheUI] Equipped slot clicked: {slot.CurrentItem?.itemData.itemName}");
            ShowEquippedItemOptions(slot.CurrentItem);
        }

        private void OnCachedSlotClicked(ItemSlotUI slot)
        {
            if (slot.IsEmpty) return;
            selectedSlot = slot;
            Debug.Log($"[ItemCacheUI] Cached slot clicked: {slot.CurrentItem?.itemData.itemName}");
            ShowCachedItemOptions(slot.CurrentItem);
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

        private void ShowEquippedItemOptions(ItemInstance item)
        {
            Debug.Log($"[ItemCacheUI] Equipped item options for: {item}");
        }

        private void ShowCachedItemOptions(ItemInstance item)
        {
            Debug.Log($"[ItemCacheUI] Cached item options for: {item}");
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

        public void DisenchantSelectedItem()
        {
            if (selectedSlot == null || selectedSlot.CurrentItem == null) return;
            int celestium = ItemCache.Instance.DisenchantItem(selectedSlot.CurrentItem);
            Debug.Log($"[ItemCacheUI] Disenchanted for {celestium} Celestium");
        }
    }
}