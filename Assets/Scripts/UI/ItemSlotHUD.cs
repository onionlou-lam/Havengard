using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Havengard.Items;

namespace Havengard.UI
{
    /// <summary>
    /// Individual item slot for the player HUD (simpler than cache UI version)
    /// </summary>
    public class ItemSlotHUD : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image rarityBorder;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private GameObject emptyIndicator;

        [Header("Colors")]
        [SerializeField] private Color emptyColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);

        private ItemData currentItem;
        private int currentLevel;
        private int slotIndex;

        public void Initialize(int index)
        {
            slotIndex = index;
            ClearSlot();
        }

        public void SetItem(ItemData itemData, int level)
        {
            currentItem = itemData;
            currentLevel = level;

            if (itemData == null)
            {
                ClearSlot();
                return;
            }

            // Set icon
            if (iconImage != null)
            {
                if (itemData.icon != null)
                {
                    iconImage.sprite = itemData.icon;
                    iconImage.enabled = true;
                    
                    // Ensure visible
                    Color color = iconImage.color;
                    color.a = 1f;
                    iconImage.color = color;
                }
                else
                {
                    iconImage.enabled = false;
                    Debug.LogWarning($"[ItemSlotHUD] Item {itemData.itemName} has no icon!");
                }
            }

            // Set rarity colors
            if (backgroundImage != null)
            {
                backgroundImage.color = itemData.rarityColor * 0.3f;
            }

            if (rarityBorder != null)
            {
                rarityBorder.color = itemData.rarityColor;
            }

            // Set level text
            if (levelText != null)
            {
                levelText.enabled = true;
                levelText.text = level > 1 ? $"{level}" : "";
            }

            // Hide empty indicator
            if (emptyIndicator != null)
            {
                emptyIndicator.SetActive(false);
            }

            Debug.Log($"[ItemSlotHUD {slotIndex}] Set item: {itemData.itemName} Lv.{level}");
        }

        public void ClearSlot()
        {
            currentItem = null;
            currentLevel = 0;

            if (iconImage != null)
            {
                iconImage.enabled = false;
                iconImage.sprite = null;
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = emptyColor;
            }

            if (rarityBorder != null)
            {
                rarityBorder.color = emptyColor;
            }

            if (levelText != null)
            {
                levelText.enabled = false;
                levelText.text = "";
            }

            if (emptyIndicator != null)
            {
                emptyIndicator.SetActive(true);
            }
        }
    }
}