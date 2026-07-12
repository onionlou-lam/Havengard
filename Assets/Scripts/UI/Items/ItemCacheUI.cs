using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Havengard.Items;
using System;

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
        private bool isInitialized = false;

        /// <summary>
        /// Event triggered when the user wants to close this UI and return to previous menu
        /// </summary>
        public event Action OnRequestClose;

        // Add this property to ItemCacheUI class
        public bool IsShowing => panel != null && panel.activeSelf;

        private void Awake()
        {
            Debug.Log("[ItemCacheUI] Awake called");
            // Initialize immediately if the GameObject is already active
            Initialize();
        }

        private void Start()
        {
            Debug.Log("[ItemCacheUI] Start called");
            // Ensure initialization happened
            Initialize();
        }

        private void Update()
        {
            // Close item cache with Escape
            if (Input.GetKeyDown(KeyCode.Escape) && IsShowing)
            {
                RequestClose();
            }
        }

        /// <summary>
        /// Initialize the UI (called from Awake/Start or lazily on first Show)
        /// </summary>
        private void Initialize()
        {
            if (isInitialized)
            {
                Debug.Log("[ItemCacheUI] Already initialized, skipping");
                return;
            }

            Debug.Log("[ItemCacheUI] Initializing...");

            // Find player inventory if not assigned
            if (playerInventory == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerInventory = player.GetComponent<ItemInventory>();
                    Debug.Log($"[ItemCacheUI] Found player inventory: {playerInventory != null}");
                }
            }

            // Hook up buttons - both close and back should return to pause menu
            if (closeButton != null)
                closeButton.onClick.AddListener(RequestClose);

            if (backButton != null)
                backButton.onClick.AddListener(RequestClose);

            if (sortByRarityButton != null)
                sortByRarityButton.onClick.AddListener(() => SortCache(SortType.Rarity));

            if (sortByTimeButton != null)
                sortByRarityButton.onClick.AddListener(() => SortCache(SortType.Time));

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

            // Ensure panel starts hidden
            if (panel != null)
            {
                panel.SetActive(false);
                Debug.Log("[ItemCacheUI] Panel hidden on initialization");
            }

            isInitialized = true;
            Debug.Log("[ItemCacheUI] Initialization complete");
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
            Debug.Log("[ItemCacheUI] Show called");

            // Ensure initialization has happened (lazy initialization)
            if (!isInitialized)
            {
                Debug.Log("[ItemCacheUI] Not initialized yet, initializing now...");
                Initialize();
            }

            if (panel != null)
            {
                panel.SetActive(true);
                Debug.Log("[ItemCacheUI] Panel activated");
            }
            else
            {
                Debug.LogError("[ItemCacheUI] Panel reference is null!");
            }

            RefreshDisplay();
        }

        public void Hide()
        {
            if (panel != null)
                panel.SetActive(false);

            // Tooltip now managed by TooltipManager - will auto-hide when not hovering
            Debug.Log("[ItemCacheUI] Hidden");
        }

        /// <summary>
        /// Request to close this UI - triggers OnRequestClose event for parent menu to handle
        /// </summary>
        private void RequestClose()
        {
            Debug.Log("[ItemCacheUI] Close/Back button pressed - requesting close");
            Hide();
            OnRequestClose?.Invoke();
        }

        public void Toggle()
        {
            if (panel.activeSelf)
                RequestClose();
            else
                Show();
        }

        private void RefreshDisplay()
        {
            Debug.Log("[ItemCacheUI] RefreshDisplay called");
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
                    // Tooltip is now handled automatically by ItemSlotUI via TooltipManager
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
                // Tooltip is now handled automatically by ItemSlotUI via TooltipManager
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
    }
}