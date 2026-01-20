using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Havengard.Items;

namespace Havengard.UI
{
    /// <summary>
    /// Displays detailed item information on hover
    /// </summary>
    public class ItemTooltipUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject tooltipPanel;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI itemLevelText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI rarityText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image rarityBorder;

        [Header("Settings")]
        [SerializeField] private Vector2 offset = new Vector2(10, -10);
        [SerializeField] private float padding = 20f;

        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            Hide();
        }

        public void Show(ItemInstance item)
        {
            if (item == null || item.itemData == null)
            {
                Hide();
                return;
            }

            // Set content
            if (itemNameText != null)
                itemNameText.text = item.itemData.itemName;

            if (itemLevelText != null)
                itemLevelText.text = $"Level {item.level}";

            if (descriptionText != null)
                descriptionText.text = item.itemData.GetScaledDescription(item.level);

            if (rarityText != null)
                rarityText.text = item.itemData.rarity.ToString();

            if (backgroundImage != null)
                backgroundImage.color = item.itemData.rarityColor * 0.3f;

            if (rarityBorder != null)
                rarityBorder.color = item.itemData.rarityColor;

            // Position tooltip near mouse
            UpdatePosition();

            tooltipPanel.SetActive(true);
        }

        public void Hide()
        {
            if (tooltipPanel != null)
                tooltipPanel.SetActive(false);
        }

        private void Update()
        {
            if (tooltipPanel.activeSelf)
            {
                UpdatePosition();
            }
        }

        private void UpdatePosition()
        {
            if (rectTransform == null) return;

            Vector2 mousePosition = Input.mousePosition;
            
            // Add offset
            Vector2 targetPosition = mousePosition + offset;

            // Clamp to screen bounds
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                RectTransform canvasRect = canvas.GetComponent<RectTransform>();
                
                // Ensure tooltip stays within screen bounds
                float halfWidth = rectTransform.rect.width / 2;
                float halfHeight = rectTransform.rect.height / 2;

                targetPosition.x = Mathf.Clamp(targetPosition.x, halfWidth + padding, Screen.width - halfWidth - padding);
                targetPosition.y = Mathf.Clamp(targetPosition.y, halfHeight + padding, Screen.height - halfHeight - padding);
            }

            rectTransform.position = targetPosition;
        }
    }
}