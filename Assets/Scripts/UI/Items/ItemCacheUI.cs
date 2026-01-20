using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Havengard.Items;
using System.Collections.Generic;

namespace Havengard.UI
{
    /// <summary>
    /// UI panel for managing the item cache
    /// </summary>
    public class ItemCacheUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Transform contentParent;
        [SerializeField] private GameObject itemSlotPrefab;
        [SerializeField] private TextMeshProUGUI celestiumText;
        [SerializeField] private TextMeshProUGUI itemCountText;
        [SerializeField] private ItemTooltipUI tooltip;

        [Header("Buttons")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button sortByRarityButton;
        [SerializeField] private Button sortByTimeButton;
        [SerializeField] private Button sortByLevelButton;

        [Header("Filter")]
        [SerializeField] private TMP_Dropdown rarityFilterDropdown;
        [SerializeField] private TMP_Dropdown typeFilterDropdown;

        private List<ItemSlotUI> itemSlots = new List<ItemSlotUI>();
        private ItemSlotUI selectedSlot;

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

            // Subscribe to events
            if (ItemCache.Instance != null)
            {
                ItemCache.Instance.OnItemAddedToCache += OnItemAddedToCache;
                ItemCache.Instance.OnItemRemovedFromCache += OnItemRemovedFromCache;
            }

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
                ItemCache.Instance.OnItemAddedToCache -= OnItemAddedToCache;
                ItemCache.Instance.OnItemRemovedFromCache -= OnItemRemovedFromCache;
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
            ClearSlots();
            PopulateSlots();
            UpdateCelestiumDisplay(ItemManager.Instance?.Celestium ?? 0);
            UpdateItemCountDisplay();
        }

        private void ClearSlots()
        {
            foreach (var slot in itemSlots)
            {
                Destroy(slot.gameObject);
            }
            itemSlots.Clear();
        }

        private void PopulateSlots()
        {
            if (ItemCache.Instance == null) return;

            foreach (var item in ItemCache.Instance.CachedItems)
            {
                CreateSlot(item);
            }
        }

        private void CreateSlot(ItemInstance item)
        {
            GameObject slotObj = Instantiate(itemSlotPrefab, contentParent);
            ItemSlotUI slot = slotObj.GetComponent<ItemSlotUI>();
            
            if (slot != null)
            {
                slot.SetItem(item);
                slot.OnSlotClicked += OnSlotClicked;
                slot.OnSlotHoverEnter += OnSlotHoverEnter;
                slot.OnSlotHoverExit += OnSlotHoverExit;
                itemSlots.Add(slot);
            }
        }

        private void OnSlotClicked(ItemSlotUI slot)
        {
            selectedSlot = slot;
            // Open assignment menu or show options
            ShowItemOptions(slot.CurrentItem);
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

        private void ShowItemOptions(ItemInstance item)
        {
            // Show context menu: Assign, Disenchant, Upgrade
            // This would open another UI panel
            Debug.Log($"[ItemCacheUI] Selected: {item}");
        }

        private void OnItemAddedToCache(ItemInstance item)
        {
            if (panel.activeSelf)
            {
                CreateSlot(item);
                UpdateItemCountDisplay();
            }
        }

        private void OnItemRemovedFromCache(ItemInstance item)
        {
            if (panel.activeSelf)
            {
                RefreshDisplay();
            }
        }

        private void UpdateCelestiumDisplay(int amount)
        {
            if (celestiumText != null)
            {
                celestiumText.text = $"Celestium: {amount}";
            }
        }

        private void UpdateItemCountDisplay()
        {
            if (itemCountText != null)
            {
                int count = ItemCache.Instance?.Count ?? 0;
                itemCountText.text = $"Items: {count}";
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

            RefreshDisplay();
        }

        /// <summary>
        /// Disenchant selected item
        /// </summary>
        public void DisenchantSelectedItem()
        {
            if (selectedSlot == null || selectedSlot.CurrentItem == null) return;

            int celestium = ItemCache.Instance.DisenchantItem(selectedSlot.CurrentItem);
            Debug.Log($"[ItemCacheUI] Disenchanted for {celestium} Celestium");
        }
    }
}