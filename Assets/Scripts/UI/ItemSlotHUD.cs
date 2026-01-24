using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Havengard.Items;
using UnityEngine.EventSystems;
using System.Collections;

namespace Havengard.UI
{
    /// <summary>
    /// Individual item slot for the player HUD (simpler than cache UI version)
    /// </summary>
    public class ItemSlotHUD : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image rarityBorder;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private GameObject emptyIndicator;

        [Header("Colors")]
        [SerializeField] private Color emptyColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);

        [Header("Animation")]
        [SerializeField] private bool enablePulseAnimation = true;
        [SerializeField] private float pulseScale = 1.2f;
        [SerializeField] private float pulseDuration = 0.2f;

        private ItemData currentItem;
        private int currentLevel;
        private int slotIndex;
        private Coroutine pulseCoroutine;

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

            // Pulse animation when item is set
            if (enablePulseAnimation && itemData != null)
            {
                if (pulseCoroutine != null)
                {
                    StopCoroutine(pulseCoroutine);
                }
                pulseCoroutine = StartCoroutine(PulseAnimation());
            }

            Debug.Log($"[ItemSlotHUD {slotIndex}] Set item: {itemData.itemName} Lv.{level}");
        }

        private IEnumerator PulseAnimation()
        {
            Vector3 originalScale = Vector3.one;
            Vector3 targetScale = Vector3.one * pulseScale;
            float elapsed = 0f;

            // Scale up
            while (elapsed < pulseDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / pulseDuration;
                transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
                yield return null;
            }

            elapsed = 0f;

            // Scale back down
            while (elapsed < pulseDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / pulseDuration;
                transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
                yield return null;
            }

            transform.localScale = originalScale;
            pulseCoroutine = null;
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

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (currentItem != null)
            {
                var tooltip = FindObjectOfType<ItemTooltipUI>();
                if (tooltip != null)
                {
                    var instance = new ItemInstance(currentItem, currentLevel);
                    tooltip.Show(instance);
                }
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            var tooltip = FindObjectOfType<ItemTooltipUI>();
            if (tooltip != null)
            {
                tooltip.Hide();
            }
        }
    }
}