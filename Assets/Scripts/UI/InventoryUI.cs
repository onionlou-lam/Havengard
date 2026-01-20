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

            Hide();
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
            targetInventory = null;
        }

        private void RefreshDisplay()
        {
            ClearSlots();
            
            if (targetInventory == null) return;

            // Get all items from inventory
            var items = targetInventory.GetAllItems();
            
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