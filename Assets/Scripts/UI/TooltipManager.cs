using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Havengard.Items;
using Havengard.Abilities;

namespace Havengard.UI
{
    /// <summary>
    /// Centralized tooltip manager for all game objects.
    /// Displays tooltips at top-right of screen with proper positioning.
    /// </summary>
    public class TooltipManager : MonoBehaviour
    {
        public static TooltipManager Instance { get; private set; }

        [Header("Tooltip Panels")]
        [SerializeField] private GameObject itemTooltipPanel;
        [SerializeField] private GameObject abilityTooltipPanel;

        [Header("Item Tooltip References")]
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI itemLevelText;
        [SerializeField] private TextMeshProUGUI itemDescriptionText;
        [SerializeField] private TextMeshProUGUI itemRarityText;
        [SerializeField] private Image itemBackgroundImage;
        [SerializeField] private Image itemRarityBorder;

        [Header("Ability Tooltip References")]
        [SerializeField] private TextMeshProUGUI abilityNameText;
        [SerializeField] private TextMeshProUGUI abilityDescriptionText;
        [SerializeField] private TextMeshProUGUI abilityCooldownText;
        [SerializeField] private TextMeshProUGUI abilityResourceCostText;
        [SerializeField] private TextMeshProUGUI abilityDamageText;
        [SerializeField] private Image abilityIconImage;

        [Header("Position Settings")]
        [SerializeField] private Vector2 topRightOffset = new Vector2(-20f, -20f);
        [SerializeField] private float padding = 20f;

        private RectTransform itemTooltipRect;
        private RectTransform abilityTooltipRect;
        private Canvas canvas;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            itemTooltipRect = itemTooltipPanel?.GetComponent<RectTransform>();
            abilityTooltipRect = abilityTooltipPanel?.GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();

            HideAll();
        }

        #region Item Tooltips
        /// <summary>
        /// Show item tooltip at top-right of screen
        /// </summary>
        public void ShowItemTooltip(ItemInstance item)
        {
            if (item == null || item.itemData == null)
            {
                HideItemTooltip();
                return;
            }

            HideAll();

            // Set content
            if (itemNameText != null)
                itemNameText.text = item.itemData.itemName;

            if (itemLevelText != null)
                itemLevelText.text = $"Level {item.level}";

            if (itemDescriptionText != null)
                itemDescriptionText.text = item.itemData.GetScaledDescription(item.level);

            if (itemRarityText != null)
                itemRarityText.text = item.itemData.rarity.ToString();

            if (itemBackgroundImage != null)
                itemBackgroundImage.color = item.itemData.rarityColor * 0.3f;

            if (itemRarityBorder != null)
                itemRarityBorder.color = item.itemData.rarityColor;

            // Position at top-right
            PositionTooltipTopRight(itemTooltipRect);

            itemTooltipPanel.SetActive(true);
        }

        /// <summary>
        /// Show item tooltip for ItemData (without level)
        /// </summary>
        public void ShowItemTooltip(ItemData itemData, int level)
        {
            if (itemData == null)
            {
                HideItemTooltip();
                return;
            }

            ShowItemTooltip(new ItemInstance(itemData, level));
        }

        public void HideItemTooltip()
        {
            if (itemTooltipPanel != null)
                itemTooltipPanel.SetActive(false);
        }
        #endregion

        #region Ability Tooltips
        /// <summary>
        /// Show ability tooltip at top-right of screen
        /// </summary>
        public void ShowAbilityTooltip(AbilityBase ability)
        {
            if (ability == null)
            {
                HideAbilityTooltip();
                return;
            }

            HideAll();

            // Set content
            if (abilityNameText != null)
                abilityNameText.text = ability.abilityName;

            if (abilityDescriptionText != null)
                abilityDescriptionText.text = ability.description;

            if (abilityCooldownText != null)
                abilityCooldownText.text = $"Cooldown: {ability.baseCooldown}s";

            if (abilityResourceCostText != null)
                abilityResourceCostText.text = $"Cost: {ability.resourceCost}";

            if (abilityDamageText != null)
            {
                float damage = ability.baseDamage + (ability.damagePerLevel * (ability.CurrentLevel - 1));
                abilityDamageText.text = $"Damage: {damage:F0} ({ability.damageType})";
            }

            if (abilityIconImage != null && ability.icon != null)
            {
                abilityIconImage.sprite = ability.icon;
                abilityIconImage.enabled = true;
            }

            // Position at top-right
            PositionTooltipTopRight(abilityTooltipRect);

            abilityTooltipPanel.SetActive(true);
        }

        public void HideAbilityTooltip()
        {
            if (abilityTooltipPanel != null)
                abilityTooltipPanel.SetActive(false);
        }
        #endregion

        #region Positioning
        /// <summary>
        /// Position tooltip at top-right of screen with padding
        /// </summary>
        private void PositionTooltipTopRight(RectTransform tooltipRect)
        {
            if (tooltipRect == null || canvas == null) return;

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            if (canvasRect == null) return;

            // Calculate top-right position
            float xPos = Screen.width + topRightOffset.x - tooltipRect.rect.width / 2;
            float yPos = Screen.height + topRightOffset.y - tooltipRect.rect.height / 2;

            // Clamp to ensure it stays on screen
            xPos = Mathf.Clamp(xPos,
                tooltipRect.rect.width / 2 + padding,
                Screen.width - tooltipRect.rect.width / 2 - padding);

            yPos = Mathf.Clamp(yPos,
                tooltipRect.rect.height / 2 + padding,
                Screen.height - tooltipRect.rect.height / 2 - padding);

            tooltipRect.position = new Vector2(xPos, yPos);
        }
        #endregion

        #region General
        public void HideAll()
        {
            HideItemTooltip();
            HideAbilityTooltip();
        }
        #endregion
    }
}