using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Havengard.Items;

namespace Havengard.UI
{
    public class ItemTooltip : MonoBehaviour
    {
        public static ItemTooltip Instance { get; private set; }
        
        [Header("UI References")]
        [SerializeField] private GameObject tooltipPanel;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI rarityText;
        [SerializeField] private Image backgroundImage;
        
        [Header("Settings")]
        [SerializeField] private Vector2 offset = new Vector2(10, 10);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            Hide();
        }

        public void Show(ItemData itemData, int level, Vector3 position)
        {
            if (itemData == null) return;

            // Set content
            if (nameText != null)
                nameText.text = $"{itemData.itemName} (Lv.{level})";

            if (descriptionText != null)
                descriptionText.text = itemData.GetScaledDescription(level);

            if (rarityText != null)
                rarityText.text = itemData.rarity.ToString();

            if (backgroundImage != null)
                backgroundImage.color = itemData.rarityColor;

            // Position
            tooltipPanel.transform.position = position + (Vector3)offset;
            tooltipPanel.SetActive(true);
        }

        public void Hide()
        {
            if (tooltipPanel != null)
                tooltipPanel.SetActive(false);
        }
    }
}