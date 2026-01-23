using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Havengard.Items;

namespace Havengard.UI
{
    /// <summary>
    /// Displays player's equipped items on the HUD during gameplay
    /// </summary>
    public class PlayerInventoryHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ItemInventory playerInventory;
        [SerializeField] private Transform slotContainer;
        [SerializeField] private GameObject itemSlotHUDPrefab;

        [Header("Settings")]
        [SerializeField] private bool autoFindPlayer = true;
        [SerializeField] private int maxDisplaySlots = 6;

        private List<ItemSlotHUD> itemSlotHUDs = new List<ItemSlotHUD>();

        private void Awake()
        {
            if (autoFindPlayer && playerInventory == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerInventory = player.GetComponent<ItemInventory>();
                }
            }

            if (playerInventory == null)
            {
                Debug.LogWarning("[PlayerInventoryHUD] No player inventory found!");
                return;
            }

            InitializeSlots();
        }

        private void OnEnable()
        {
            if (playerInventory != null)
            {
                playerInventory.OnItemAdded += OnInventoryChanged;
                playerInventory.OnItemRemoved += OnInventoryChanged;
                playerInventory.OnItemLevelUp += OnInventoryChanged;
                playerInventory.OnInventoryChanged += RefreshAllSlots;
            }
        }

        private void OnDisable()
        {
            if (playerInventory != null)
            {
                playerInventory.OnItemAdded -= OnInventoryChanged;
                playerInventory.OnItemRemoved -= OnInventoryChanged;
                playerInventory.OnItemLevelUp -= OnInventoryChanged;
                playerInventory.OnInventoryChanged -= RefreshAllSlots;
            }
        }

        private void Start()
        {
            RefreshAllSlots();
        }

        /// <summary>
        /// Create the slot UI elements
        /// </summary>
        private void InitializeSlots()
        {
            if (slotContainer == null || itemSlotHUDPrefab == null)
            {
                Debug.LogError("[PlayerInventoryHUD] Missing slot container or prefab reference!");
                return;
            }

            // Clear existing slots
            foreach (var slot in itemSlotHUDs)
            {
                if (slot != null)
                    Destroy(slot.gameObject);
            }
            itemSlotHUDs.Clear();

            // Create slots
            for (int i = 0; i < maxDisplaySlots; i++)
            {
                GameObject slotObj = Instantiate(itemSlotHUDPrefab, slotContainer);
                ItemSlotHUD slotHUD = slotObj.GetComponent<ItemSlotHUD>();
                
                if (slotHUD != null)
                {
                    slotHUD.Initialize(i);
                    itemSlotHUDs.Add(slotHUD);
                }
                else
                {
                    Debug.LogError("[PlayerInventoryHUD] Slot prefab missing ItemSlotHUD component!");
                }
            }

            Debug.Log($"[PlayerInventoryHUD] Initialized {itemSlotHUDs.Count} slots");
        }

        /// <summary>
        /// Refresh all slot displays
        /// </summary>
        private void RefreshAllSlots()
        {
            if (playerInventory == null) return;

            var items = playerInventory.Items;

            for (int i = 0; i < itemSlotHUDs.Count; i++)
            {
                if (i < items.Count)
                {
                    // Show item
                    var slot = items[i];
                    itemSlotHUDs[i].SetItem(slot.itemData, slot.currentLevel);
                }
                else
                {
                    // Empty slot
                    itemSlotHUDs[i].ClearSlot();
                }
            }

            Debug.Log($"[PlayerInventoryHUD] Refreshed {items.Count} item slots");
        }

        private void OnInventoryChanged(ItemData data, int level)
        {
            RefreshAllSlots();
        }

        private void OnInventoryChanged(ItemData data)
        {
            RefreshAllSlots();
        }

        /// <summary>
        /// Force refresh from external call
        /// </summary>
        public void ForceRefresh()
        {
            RefreshAllSlots();
        }
    }
}