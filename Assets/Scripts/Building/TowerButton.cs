using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Havengard.Resources;

namespace Havengard.Building
{
    /// <summary>
    /// Individual tower button in the selection panel
    /// Handles click, hover, and affordability display
    /// </summary>
    public class TowerButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI Elements")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Image affordabilityOverlay;

        [Header("Colors")]
        [SerializeField] private Color affordableColor = Color.white;
        [SerializeField] private Color unaffordableColor = new Color(1f, 0.3f, 0.3f);

        private TowerBuildData towerData;
        private TowerSelectionPanel parentPanel;
        private TowerTooltip tooltip;
        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
            if (button == null)
                button = gameObject.AddComponent<Button>();

            // Auto-find UI elements if not assigned
            if (iconImage == null)
                iconImage = transform.Find("Icon")?.GetComponent<Image>();

            if (nameText == null)
                nameText = GetComponentInChildren<TextMeshProUGUI>();

            if (costText == null)
            {
                var texts = GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length > 1)
                    costText = texts[1];
            }
        }

        public void Initialize(TowerBuildData data, TowerSelectionPanel panel, TowerTooltip towerTooltip)
        {
            towerData = data;
            parentPanel = panel;
            tooltip = towerTooltip;

            UpdateDisplay();

            // Setup button click
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClicked);
        }

        private void Update()
        {
            // Update affordability display every frame
            UpdateAffordability();
        }

        private void UpdateDisplay()
        {
            if (towerData == null)
                return;

            // Set icon
            if (iconImage != null && towerData.icon != null)
            {
                iconImage.sprite = towerData.icon;
                iconImage.enabled = true;
            }

            // Set name
            if (nameText != null)
            {
                nameText.text = towerData.displayName;
            }

            // Set cost
            var levelData = towerData.GetLevelData(0);
            if (costText != null && levelData != null)
            {
                costText.text = $"{levelData.buildCost}g";
            }
        }

        private void UpdateAffordability()
        {
            if (towerData == null)
                return;

            var levelData = towerData.GetLevelData(0);
            if (levelData == null)
                return;

            bool canAfford = true;

            if (GoldSystem.Instance != null)
            {
                canAfford = GoldSystem.Instance.Current >= levelData.buildCost;
            }

            // Update visual feedback
            Color targetColor = canAfford ? affordableColor : unaffordableColor;

            if (iconImage != null)
            {
                iconImage.color = targetColor;
            }

            if (costText != null)
            {
                costText.color = canAfford ? Color.white : Color.red;
            }

            // Update overlay if present
            if (affordabilityOverlay != null)
            {
                affordabilityOverlay.enabled = !canAfford;
            }

            // Disable button if can't afford
            if (button != null)
            {
                button.interactable = canAfford;
            }
        }

        private void OnClicked()
        {
            if (parentPanel != null && towerData != null)
            {
                parentPanel.OnTowerSelected(towerData);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (tooltip != null && towerData != null)
            {
                tooltip.ShowTooltip(towerData);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (tooltip != null)
            {
                tooltip.HideTooltip();
            }
        }
    }
}