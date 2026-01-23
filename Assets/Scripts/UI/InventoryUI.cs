using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Havengard.Items;
using System.Collections.Generic;

namespace Havengard.UI
{
    /// <summary>
    /// Displays character inventory UI
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Transform contentParent;
        [SerializeField] private GameObject itemSlotPrefab;
        [SerializeField] private TextMeshProUGUI characterNameText;

        [Header("Buttons")]
        [SerializeField] private Button closeButton;

        private ItemInventory targetInventory;
        private List<ItemSlotUI> itemSlots = new List<ItemSlotUI>();

        private void Start()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            // Find player inventory and subscribe to changes
            var playerInventory = FindFirstObjectByType<ItemInventory>();
            if (playerInventory != null)
            {
                playerInventory.OnInventoryChanged += OnInventoryChanged;
                targetInventory = playerInventory;
                Debug.Log("[InventoryUI] Subscribed to player inventory changes");
            }

            Hide();
        }

        private void OnDestroy()
        {
            if (targetInventory != null)
            {
                targetInventory.OnInventoryChanged -= OnInventoryChanged;
            }
        }

        private void OnInventoryChanged()
        {
            if (panel.activeSelf)
            {
                RefreshDisplay();
            }
        }

        public void Show(ItemInventory inventory)
        {
            if (inventory == null)
            {
                Debug.LogWarning("[InventoryUI] Cannot show inventory: inventory is null");
                return;
            }

            targetInventory = inventory;
            panel.SetActive(true);

            if (characterNameText != null)
                characterNameText.text = inventory.gameObject.name;

            RefreshDisplay();
        }

        public void Hide()
        {
            panel.SetActive(false);
        }

        private void RefreshDisplay()
        {
            ClearSlots();
            
            if (targetInventory == null) return;

            // Get all items from inventory
            var items = targetInventory.GetAllItems();
            
            Debug.Log($"[InventoryUI] Refreshing display with {items.Count} items");
            
            foreach (var item in items)
            {
                CreateSlot(item);
            }
        }

        private void ClearSlots()
        {
            foreach (var slot in itemSlots)
            {
                if (slot != null)
                    Destroy(slot.gameObject);
            }
            itemSlots.Clear();
        }

        private void CreateSlot(ItemInstance item)
        {
            if (itemSlotPrefab == null)
            {
                Debug.LogWarning("[InventoryUI] Item slot prefab is not assigned");
                return;
            }

            GameObject slotObj = Instantiate(itemSlotPrefab, contentParent);
            ItemSlotUI slot = slotObj.GetComponent<ItemSlotUI>();
            
            if (slot != null)
            {
                slot.SetItem(item);
                itemSlots.Add(slot);
                Debug.Log($"[InventoryUI] Created slot for {item.itemData.itemName} with icon: {item.itemData.icon != null}");
            }
            else
            {
                Debug.LogError("[InventoryUI] ItemSlotUI component not found on instantiated prefab!");
            }
        }

        public void RemoveItem(ItemInstance item)
        {
            if (targetInventory == null) return;

            targetInventory.RemoveItem(item);
            RefreshDisplay();
        }
    }
}