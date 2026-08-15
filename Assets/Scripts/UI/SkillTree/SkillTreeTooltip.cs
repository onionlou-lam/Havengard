using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Havengard.Abilities;
using Havengard.Combat;

namespace Havengard.UI  // Changed from Havengard.UI.SkillTree to match SkillTreeNodeUI
{
    /// <summary>
    /// Dedicated tooltip for skill tree that shows detailed ability info,
    /// requirements, and unlock status. Follows mouse cursor.
    /// </summary>
    public class SkillTreeTooltip : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private GameObject tooltipPanel;
        [SerializeField] private Image abilityIcon;
        [SerializeField] private TextMeshProUGUI abilityNameText;
        [SerializeField] private TextMeshProUGUI abilityTypeText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI damageText;
        [SerializeField] private TextMeshProUGUI cooldownText;
        [SerializeField] private TextMeshProUGUI resourceCostText;
        [SerializeField] private TextMeshProUGUI requirementsText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Image statusBackground;

        [Header("Colors")]
        [SerializeField] private Color unlockedStatusColor = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color canUnlockStatusColor = new Color(0.9f, 0.9f, 0.2f);
        [SerializeField] private Color lockedStatusColor = new Color(0.8f, 0.2f, 0.2f);

        [Header("Layout")]
        [SerializeField] private Vector2 offset = new Vector2(15f, 15f);
        [SerializeField] private float padding = 10f;

        private RectTransform rectTransform;
        private Canvas parentCanvas;
        private bool isVisible = false;

        //-----------------------------------------------------

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            parentCanvas = GetComponentInParent<Canvas>();

            // Auto-find tooltip panel if not assigned
            if (tooltipPanel == null)
                tooltipPanel = gameObject;

            HideTooltip();
        }

        //-----------------------------------------------------

        private void Update()
        {
            if (isVisible)
            {
                UpdatePosition();
            }
        }

        //-----------------------------------------------------

        /// <summary>
        /// Show tooltip with full ability details and skill tree context
        /// </summary>
        public void ShowTooltip(ClassAbility classAbility, Vector2 nodeWorldPosition, bool isUnlocked, bool canUnlock)
        {
            if (classAbility == null || classAbility.ability == null)
            {
                HideTooltip();
                return;
            }

            if (tooltipPanel != null)
                tooltipPanel.SetActive(true);

            isVisible = true;

            AbilityBase ability = classAbility.ability;

            // Icon
            if (abilityIcon != null && ability.icon != null)
            {
                abilityIcon.sprite = ability.icon;
                abilityIcon.enabled = true;
            }

            // Ability Name
            if (abilityNameText != null)
            {
                abilityNameText.text = ability.abilityName;
            }

            // Ability Type
            if (abilityTypeText != null)
            {
                abilityTypeText.text = GetAbilityTypeLabel(ability);
            }

            // Description
            if (descriptionText != null)
            {
                descriptionText.text = classAbility.GetDescription();
            }

            // Damage
            if (damageText != null)
            {
                string damageInfo = GetDamageInfo(ability);
                damageText.text = damageInfo;
            }

            // Cooldown
            if (cooldownText != null)
            {
                if (ability.baseCooldown > 0f)
                    cooldownText.text = $"Cooldown: {ability.baseCooldown:F1}s";
                else
                    cooldownText.text = "Cooldown: None";
            }

            // Resource Cost
            if (resourceCostText != null)
            {
                if (ability.resourceCost > 0)
                    resourceCostText.text = $"Cost: {ability.resourceCost} Mana";
                else
                    resourceCostText.text = "Cost: Free";
            }

            // Requirements
            if (requirementsText != null)
            {
                string reqText = $"<b>Requirements:</b>\n";
                reqText += $"• Level: {classAbility.requiredLevel}\n";
                reqText += $"• Skill Points: {classAbility.skillPointCost}";

                if (classAbility.HasPrerequisites())
                {
                    reqText += "\n• <color=cyan>Prerequisites Required</color>";
                }

                if (classAbility.HasSubSkills())
                {
                    int subSkillCount = classAbility.GetSubSkillCount();
                    reqText += $"\n• <color=yellow>{subSkillCount} Sub-Skill(s) Available</color>";
                }

                requirementsText.text = reqText;
            }

            // Status
            UpdateStatus(isUnlocked, canUnlock);

            UpdatePosition();
        }

        //-----------------------------------------------------

        /// <summary>
        /// Update unlock status display
        /// </summary>
        private void UpdateStatus(bool isUnlocked, bool canUnlock)
        {
            if (statusText != null)
            {
                if (isUnlocked)
                {
                    statusText.text = "✓ UNLOCKED";
                    if (statusBackground != null)
                        statusBackground.color = unlockedStatusColor;
                }
                else if (canUnlock)
                {
                    statusText.text = "▶ CLICK TO UNLOCK";
                    if (statusBackground != null)
                        statusBackground.color = canUnlockStatusColor;
                }
                else
                {
                    statusText.text = "✖ LOCKED";
                    if (statusBackground != null)
                        statusBackground.color = lockedStatusColor;
                }
            }
        }

        //-----------------------------------------------------

        /// <summary>
        /// Get ability type label with color coding
        /// </summary>
        private string GetAbilityTypeLabel(AbilityBase ability)
        {
            if (ability is ChanneledAbilityBase)
                return "<color=#FFD700>[Channeled]</color>";
            if (ability is ZoneAbility)
                return "<color=#FF6347>[Zone/AoE]</color>";
            if (ability.GetType().Name.Contains("Missile") || ability.GetType().Name.Contains("Projectile"))
                return "<color=#87CEEB>[Projectile]</color>";
            if (ability.GetType().Name.Contains("Buff"))
                return "<color=#90EE90>[Buff]</color>";
            if (ability.GetType().Name.Contains("Dash") || ability.GetType().Name.Contains("Roll"))
                return "<color=#FFA500>[Mobility]</color>";

            return "<color=#FFFFFF>[Instant]</color>";
        }

        //-----------------------------------------------------

        /// <summary>
        /// Get detailed damage information
        /// </summary>
        private string GetDamageInfo(AbilityBase ability)
        {
            string damageInfo = "";

            // Damage type with color
            Color damageColor = GetDamageTypeColor(ability.damageType);
            string damageTypeText = $"<color=#{ColorUtility.ToHtmlStringRGB(damageColor)}>{ability.damageType}</color>";
            
            // Calculate base damage
            float damage = ability.CalculateDamage(null, ability.CurrentLevel);

            if (ability is ChanneledAbilityBase channeled)
            {
                float tickRate = channeled.TickRate;
                float duration = channeled.MaxChargeTime;
                float damagePerTick = damage * tickRate;
                float totalDamage = damage * duration;

                damageInfo = $"{damageTypeText} Damage: {damagePerTick:F0} per {tickRate:F1}s\n";
                damageInfo += $"Total: {totalDamage:F0} over {duration:F1}s";
            }
            else if (ability is ZoneAbility)
            {
                damageInfo = $"{damageTypeText} Damage: {damage:F0} per tick\n(Damage over Time)";
            }
            else
            {
                damageInfo = $"{damageTypeText} Damage: {damage:F0}";
                
                if (ability.damagePerLevel > 0f)
                {
                    damageInfo += $" (+{ability.damagePerLevel:F0}/lvl)";
                }
            }

            // Add healing info if applicable
            if (ability.canHeal && ability.healingRatio > 0f)
            {
                float healing = ability.CalculateHealing(null, ability.CurrentLevel);
                damageInfo += $"\nHealing: {healing:F0} ({(ability.healingRatio * 100):F0}%)";
            }

            // Add lifesteal
            if (ability.lifestealPercent > 0f)
            {
                damageInfo += $"\nLifesteal: {(ability.lifestealPercent * 100):F0}%";
            }

            // Add range
            if (ability.range > 0f)
            {
                damageInfo += $"\nRange: {ability.range:F0}m";
            }

            return damageInfo;
        }

        //-----------------------------------------------------

        /// <summary>
        /// Get damage type color
        /// </summary>
        private Color GetDamageTypeColor(DamageType damageType)
        {
            return damageType switch
            {
                DamageType.Fire => new Color(1f, 0.3f, 0f),
                DamageType.Frost => new Color(0.3f, 0.8f, 1f),
                DamageType.Lightning => new Color(1f, 1f, 0.3f),
                DamageType.Holy => new Color(1f, 0.9f, 0.3f),
                DamageType.Physical => new Color(0.8f, 0.8f, 0.8f),
                DamageType.Arcane => new Color(0.7f, 0.3f, 1f),
                _ => Color.white
            };
        }

        //-----------------------------------------------------

        /// <summary>
        /// Update tooltip position to follow mouse with edge clamping
        /// </summary>
        private void UpdatePosition()
        {
            if (rectTransform == null || parentCanvas == null)
                return;

            // Get mouse position in canvas space
            Vector2 mouseScreenPos = Input.mousePosition;
            RectTransform canvasRect = parentCanvas.transform as RectTransform;
            Vector2 localPoint;
            Camera canvasCamera = parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main;

            bool success = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                mouseScreenPos,
                canvasCamera,
                out localPoint
            );

            if (!success)
                return;

            // Apply offset (bottom-right of cursor)
            localPoint += offset;

            // Get tooltip dimensions
            Rect canvasBounds = canvasRect.rect;
            float tooltipWidth = rectTransform.rect.width;
            float tooltipHeight = rectTransform.rect.height;

            // Clamp to canvas bounds (with padding)
            if (localPoint.x + tooltipWidth > canvasBounds.xMax)
            {
                // Flip to left side of cursor
                localPoint.x -= tooltipWidth + (offset.x * 2);
            }
            
            if (localPoint.x < canvasBounds.xMin)
            {
                localPoint.x = canvasBounds.xMin + padding;
            }

            if (localPoint.y - tooltipHeight < canvasBounds.yMin)
            {
                // Flip to top of cursor
                localPoint.y += tooltipHeight + (offset.y * 2);
            }
            
            if (localPoint.y > canvasBounds.yMax)
            {
                localPoint.y = canvasBounds.yMax - padding;
            }

            // Set position
            rectTransform.anchoredPosition = localPoint;
        }

        //-----------------------------------------------------

        /// <summary>
        /// Hide the tooltip
        /// </summary>
        public void HideTooltip()
        {
            isVisible = false;

            if (tooltipPanel != null)
                tooltipPanel.SetActive(false);
        }
    }
}