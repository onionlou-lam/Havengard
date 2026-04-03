using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Havengard.Items;
using Havengard.Resources;
using System.Collections.Generic;

namespace Havengard.UI
{
    /// <summary>
    /// UI for upgrading items using Celestium
    /// </summary>
    public class ItemUpgradeUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Transform contentParent;
        [SerializeField] private GameObject upgradeSlotPrefab;
        [SerializeField] private Button closeButton;

        [Header("Costs")]
        [SerializeField] private int baseCelestiumCost = 50;

        private ItemInventory targetInventory;
        private List<ItemUpgradeSlot> upgradeSlots = new List<ItemUpgradeSlot>();

        private void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            Hide();
        }

        public void Show(ItemInventory inventory)
        {
            if (inventory == null)
            {
                Debug.LogWarning("[ItemUpgradeUI] Cannot show: inventory is null");
                return;
            }

            targetInventory = inventory;
            panel.SetActive(true);
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

            var items = targetInventory.GetAllItems();
            foreach (var item in items)
            {
                if (item.level < item.itemData.maxLevel)
                {
                    CreateUpgradeSlot(item);
                }
            }
        }

        private void ClearSlots()
        {
            foreach (var slot in upgradeSlots)
            {
                if (slot != null) Destroy(slot.gameObject);
            }
            upgradeSlots.Clear();
        }

        private void CreateUpgradeSlot(ItemInstance item)
        {
            if (upgradeSlotPrefab == null) return;

            GameObject slotObj = Instantiate(upgradeSlotPrefab, contentParent);
            ItemUpgradeSlot slot = slotObj.GetComponent<ItemUpgradeSlot>();

            if (slot != null)
            {
                int cost = GetUpgradeCost(item);
                slot.Setup(item, cost, () => OnUpgradeClicked(item));
                upgradeSlots.Add(slot);
            }
        }

        private void OnUpgradeClicked(ItemInstance item)
        {
            int cost = GetUpgradeCost(item);

            if (CelestiumSystem.Instance.SpendCelestium(cost))
            {
                // Upgrade item level
                if (item.TryStack())
                {
                    Debug.Log($"[ItemUpgradeUI] Upgraded {item.itemData.itemName} to level {item.level}");

                    // Refresh inventory to apply new effects
                    if (targetInventory != null)
                    {
                        targetInventory.RemoveItem(item);
                        targetInventory.TryAddItem(item);
                    }

                    RefreshDisplay();
                }
            }
            else
            {
                Debug.LogWarning($"[ItemUpgradeUI] Not enough Celestium! Need {cost}, have {CelestiumSystem.Instance.Current}");
            }
        }

        private int GetUpgradeCost(ItemInstance item)
        {
            return baseCelestiumCost * item.level;
        }
    }

    /// <summary>
    /// Individual upgrade slot component
    /// </summary>
    public class ItemUpgradeSlot : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Button upgradeButton;

        public void Setup(ItemInstance item, int cost, System.Action onUpgrade)
        {
            if (iconImage != null && item.itemData.icon != null)
                iconImage.sprite = item.itemData.icon;

            if (nameText != null)
                nameText.text = item.itemData.itemName;

            if (levelText != null)
                levelText.text = $"Lv.{item.level} → {item.level + 1}";

            if (costText != null)
                costText.text = $"{cost} Celestium";

            if (upgradeButton != null)
            {
                upgradeButton.onClick.RemoveAllListeners();
                upgradeButton.onClick.AddListener(() => onUpgrade?.Invoke());

                // Disable button if not enough celestium
                bool canAfford = CelestiumSystem.Instance != null && CelestiumSystem.Instance.Current >= cost;
                upgradeButton.interactable = canAfford;
            }
        }
    }
}