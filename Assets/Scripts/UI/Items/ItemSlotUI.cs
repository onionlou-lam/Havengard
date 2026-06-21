using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Havengard.Items;

namespace Havengard.UI
{
    /// <summary>
    /// UI representation of a single item slot
    /// </summary>
    public class ItemSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Image rarityBorder;
        [SerializeField] private GameObject emptyIndicator;

        [Header("Colors")]
        [SerializeField] private Color emptyColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        [SerializeField] private Color commonColor = Color.white;
        [SerializeField] private Color uncommonColor = Color.green;
        [SerializeField] private Color rareColor = Color.blue;
        [SerializeField] private Color epicColor = new Color(0.6f, 0f, 1f);
        [SerializeField] private Color legendaryColor = new Color(1f, 0.5f, 0f);

        private ItemInstance currentItem;
        private bool isEmpty = true;

        public ItemInstance CurrentItem => currentItem;
        public bool IsEmpty => isEmpty;

        public event System.Action<ItemSlotUI> OnSlotClicked;
        public event System.Action<ItemSlotUI> OnSlotHoverEnter;
        public event System.Action<ItemSlotUI> OnSlotHoverExit;

        private void Start()
        {
            UpdateVisuals();
        }

        /// <summary>
        /// Set the item for this slot
        /// </summary>
        public void SetItem(ItemInstance item)
        {
            currentItem = item;
            isEmpty = (item == null);
            UpdateVisuals();
        }

        /// <summary>
        /// Clear the slot
        /// </summary>
        public void ClearSlot()
        {
            SetItem(null);
        }

        private void UpdateVisuals()
        {
            if (isEmpty || currentItem == null)
            {
                // Empty slot
                if (iconImage != null)
                {
                    iconImage.enabled = false;
                    iconImage.sprite = null;
                }

                if (levelText != null)
                {
                    levelText.enabled = false;
                    levelText.text = "";
                }

                if (rarityBorder != null)
                {
                    rarityBorder.color = emptyColor;
                }

                if (backgroundImage != null)
                {
                    backgroundImage.color = emptyColor;
                }

                if (emptyIndicator != null)
                {
                    emptyIndicator.SetActive(true);
                }
            }
            else
            {
                // Filled slot
                if (iconImage != null)
                {
                    if (currentItem.itemData.icon != null)
                    {
                        iconImage.sprite = currentItem.itemData.icon;
                        iconImage.enabled = true;

                        // Ensure the image color is visible
                        Color imageColor = iconImage.color;
                        imageColor.a = 1f;
                        iconImage.color = imageColor;
                    }
                    else
                    {
                        Debug.LogWarning($"[ItemSlotUI] Item {currentItem.itemData.itemName} has no icon assigned!");
                        iconImage.enabled = false;
                    }
                }

                if (levelText != null)
                {
                    levelText.enabled = true;
                    levelText.text = $"Lv.{currentItem.level}";
                }

                if (rarityBorder != null)
                {
                    rarityBorder.color = GetRarityColor(currentItem.itemData.rarity);
                }

                if (backgroundImage != null)
                {
                    backgroundImage.color = GetRarityColor(currentItem.itemData.rarity) * 0.3f;
                }

                if (emptyIndicator != null)
                {
                    emptyIndicator.SetActive(false);
                }

                Debug.Log($"[ItemSlotUI] Updated visuals for {currentItem.itemData.itemName} - Icon: {currentItem.itemData.icon != null}");
            }
        }

        private Color GetRarityColor(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Common => commonColor,
                ItemRarity.Uncommon => uncommonColor,
                ItemRarity.Rare => rareColor,
                ItemRarity.Epic => epicColor,
                ItemRarity.Legendary => legendaryColor,
                _ => commonColor
            };
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnSlotHoverEnter?.Invoke(this);

            // Show tooltip via TooltipManager
            if (!isEmpty && currentItem != null && TooltipManager.Instance != null)
            {
                TooltipManager.Instance.ShowItemTooltip(currentItem);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnSlotHoverExit?.Invoke(this);

            // Hide tooltip via TooltipManager
            if (TooltipManager.Instance != null)
            {
                TooltipManager.Instance.HideItemTooltip();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnSlotClicked?.Invoke(this);
        }
    }
}