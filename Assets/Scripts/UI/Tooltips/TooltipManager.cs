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
    /// Intelligently handles different ability types and future modifiers.
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
        [SerializeField] private Image abilityIconImage;
        [SerializeField] private TextMeshProUGUI abilityNameText;
        [SerializeField] private TextMeshProUGUI abilityTypeText;
        [SerializeField] private TextMeshProUGUI abilityDescriptionText;
        [SerializeField] private TextMeshProUGUI abilityCooldownText;
        [SerializeField] private TextMeshProUGUI abilityResourceCostText;
        [SerializeField] private TextMeshProUGUI abilityDamageText;
        [SerializeField] private TextMeshProUGUI abilityRangeText;
        [SerializeField] private TextMeshProUGUI abilityDamageTypeText;
        [SerializeField] private GameObject abilityModifiersSection;
        [SerializeField] private TextMeshProUGUI abilityModifiersText;

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
        public void ShowItemTooltip(ItemInstance item)
        {
            if (item == null || item.itemData == null)
            {
                HideItemTooltip();
                return;
            }

            HideAll();

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

            PositionTooltipTopRight(itemTooltipRect);
            itemTooltipPanel.SetActive(true);
        }

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
        /// Show ability tooltip with intelligent formatting based on ability type
        /// </summary>
        public void ShowAbilityTooltip(AbilityBase ability)
        {
            if (ability == null)
            {
                HideAbilityTooltip();
                return;
            }

            HideAll();

            // Icon
            if (abilityIconImage != null && ability.icon != null)
            {
                abilityIconImage.sprite = ability.icon;
                abilityIconImage.enabled = true;
            }

            // Name
            if (abilityNameText != null)
                abilityNameText.text = ability.abilityName;

            // Type (Channeled, Zone, Projectile, etc.)
            if (abilityTypeText != null)
                abilityTypeText.text = GetAbilityTypeLabel(ability);

            // Description
            if (abilityDescriptionText != null)
                abilityDescriptionText.text = ability.description;

            // Cooldown - MAX cooldown duration (not current remaining)
            if (abilityCooldownText != null)
            {
                if (ability.baseCooldown > 0f)
                    abilityCooldownText.text = $"Cooldown: {ability.baseCooldown:F1}s";
                else
                    abilityCooldownText.text = "Cooldown: None";
            }

            // Resource Cost
            if (abilityResourceCostText != null)
            {
                if (ability.resourceCost > 0)
                    abilityResourceCostText.text = $"Cost: {ability.resourceCost} Mana";
                else
                    abilityResourceCostText.text = "Cost: Free";
            }

            // Damage (with intelligent formatting based on ability type)
            if (abilityDamageText != null)
                abilityDamageText.text = GetDamageText(ability);

            // Range
            if (abilityRangeText != null)
            {
                if (ability.range > 0f)
                    abilityRangeText.text = $"Range: {ability.range:F0} units";
                else
                    abilityRangeText.text = "";
            }

            // Damage Type with color
            if (abilityDamageTypeText != null)
            {
                abilityDamageTypeText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(GetDamageTypeColor(ability.damageType))}>{ability.damageType}</color>";
            }

            // Modifiers Section (for future sub-skills and item modifiers)
            if (abilityModifiersSection != null && abilityModifiersText != null)
            {
                string modifiers = GetAbilityModifiers(ability);
                if (!string.IsNullOrEmpty(modifiers))
                {
                    abilityModifiersSection.SetActive(true);
                    abilityModifiersText.text = modifiers;
                }
                else
                {
                    abilityModifiersSection.SetActive(false);
                }
            }

            PositionTooltipTopRight(abilityTooltipRect);
            abilityTooltipPanel.SetActive(true);
        }

        /// <summary>
        /// Get ability type label based on inheritance
        /// </summary>
        private string GetAbilityTypeLabel(AbilityBase ability)
        {
            if (ability is ChanneledAbilityBase)
                return "[Channeled]";
            if (ability is ZoneAbility)
                return "[Zone/AoE]";
            if (ability.GetType().Name.Contains("Missile") || ability.GetType().Name.Contains("Projectile"))
                return "[Projectile]";
            if (ability.GetType().Name.Contains("Buff"))
                return "[Buff]";
            if (ability.GetType().Name.Contains("Dash") || ability.GetType().Name.Contains("Roll"))
                return "[Mobility]";

            return "[Instant]";
        }

        /// <summary>
        /// Get intelligent damage text based on ability type
        /// </summary>
        private string GetDamageText(AbilityBase ability)
        {
            // Check if this is a channeled ability (has tick damage)
            if (ability is ChanneledAbilityBase channeled)
            {
                float damage = ability.CalculateDamage(null, ability.CurrentLevel);
                float tickRate = channeled.TickRate;
                float duration = channeled.MaxChargeTime;

                if (damage > 0f && tickRate > 0f)
                {
                    // Calculate damage per tick
                    float damagePerTick = damage * tickRate;
                    // Calculate total ticks
                    int totalTicks = Mathf.FloorToInt(duration / tickRate);
                    // Calculate total damage
                    float totalDamage = damage * duration;

                    return $"Damage: {damagePerTick:F0} per {tickRate:F1}s\n" +
                           $"Total: {totalDamage:F0} over {duration:F1}s ({totalTicks} ticks)";
                }
            }

            // Check if this is a zone ability (has DoT)
            if (ability is ZoneAbility)
            {
                float damage = ability.CalculateDamage(null, ability.CurrentLevel);
                if (damage > 0f)
                {
                    return $"Damage: {damage:F0} per tick\n(Damage over Time)";
                }
            }

            // Standard instant/projectile damage
            float standardDamage = ability.CalculateDamage(null, ability.CurrentLevel);

            if (standardDamage > 0f)
            {
                // Show damage + level scaling
                if (ability.CurrentLevel > 1 && ability.damagePerLevel > 0f)
                {
                    return $"Damage: {standardDamage:F0} (+{ability.damagePerLevel:F0} per level)";
                }
                else
                {
                    return $"Damage: {standardDamage:F0}";
                }
            }

            // Check if this is a healing ability
            if (ability.canHeal && ability.healingRatio > 0f)
            {
                float healing = ability.CalculateHealing(null, ability.CurrentLevel);
                return $"Healing: {healing:F0} ({(ability.healingRatio * 100):F0}% conversion)";
            }

            // No damage (utility/movement ability)
            return "Damage: None";
        }

        /// <summary>
        /// Get modifiers text for sub-skills and item effects
        /// </summary>
        private string GetAbilityModifiers(AbilityBase ability)
        {
            string modifiers = "";

            // Lifesteal
            if (ability.lifestealPercent > 0f)
            {
                modifiers += $"• Lifesteal: {(ability.lifestealPercent * 100):F0}%\n";
            }

            // Healing ratio (for Holy damage)
            if (ability.canHeal && ability.healingRatio > 0f && ability.healingRatio < 1f)
            {
                modifiers += $"• Heals allies for {(ability.healingRatio * 100):F0}% of damage\n";
            }

            // Sub-skill modifiers
            if (ability.activeSubSkills != null && ability.activeSubSkills.Count > 0)
            {
                modifiers += "<color=cyan>[Active Sub-Skills]</color>\n";
                
                foreach (AbilitySubSkill subSkill in ability.activeSubSkills)
                {
                    if (subSkill == null) continue;

                    modifiers += $"<b>• {subSkill.subSkillName}</b>\n";

                    // Damage modifiers
                    if (subSkill.modifiesDamage && subSkill.damageMultiplier != 1f)
                    {
                        float percent = (subSkill.damageMultiplier - 1f) * 100f;
                        modifiers += $"  Damage: {(percent >= 0 ? "+" : "")}{percent:F0}%\n";
                    }

                    // Cooldown modifiers
                    if (subSkill.modifiesCooldown && subSkill.cooldownReduction > 0f)
                    {
                        modifiers += $"  Cooldown: -{(subSkill.cooldownReduction * 100):F0}%\n";
                    }

                    // Cost modifiers
                    if (subSkill.modifiesCost && subSkill.costReduction > 0f)
                    {
                        modifiers += $"  Cost: -{(subSkill.costReduction * 100):F0}%\n";
                    }

                    // Projectile modifiers
                    if (subSkill.addsProjectiles && subSkill.additionalProjectiles > 0)
                    {
                        modifiers += $"  Fires {subSkill.additionalProjectiles + 1} projectiles\n";
                    }

                    // Chain modifiers
                    if (subSkill.enablesChaining && subSkill.chainCount > 0)
                    {
                        modifiers += $"  Chains {subSkill.chainCount} times\n";
                    }

                    // AoE modifiers
                    if (subSkill.addsExplosion)
                    {
                        modifiers += $"  Creates {subSkill.explosionRadius:F0}m explosion\n";
                    }

                    // Status effects
                    if (subSkill.appliesStatusEffect && subSkill.statusEffect != null)
                    {
                        modifiers += $"  Applies: {subSkill.statusEffect.name}\n";
                    }
                }
            }

            return modifiers.TrimEnd('\n');
        }

        /// <summary>
        /// Get color for damage type
        /// </summary>
        private Color GetDamageTypeColor(Combat.DamageType damageType)
        {
            return damageType switch
            {
                Combat.DamageType.Fire => new Color(1f, 0.3f, 0f),
                Combat.DamageType.Frost => new Color(0.3f, 0.8f, 1f),
                Combat.DamageType.Lightning => new Color(1f, 1f, 0.3f),
                Combat.DamageType.Holy => new Color(1f, 0.9f, 0.3f),
                Combat.DamageType.Physical => new Color(0.8f, 0.8f, 0.8f),
                Combat.DamageType.Arcane => new Color(0.7f, 0.3f, 1f),
                _ => Color.white
            };
        }

        public void HideAbilityTooltip()
        {
            if (abilityTooltipPanel != null)
                abilityTooltipPanel.SetActive(false);
        }
        #endregion

        #region Positioning
        private void PositionTooltipTopRight(RectTransform tooltipRect)
        {
            if (tooltipRect == null || canvas == null) return;

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            if (canvasRect == null) return;

            float xPos = Screen.width + topRightOffset.x - tooltipRect.rect.width / 2;
            float yPos = Screen.height + topRightOffset.y - tooltipRect.rect.height / 2;

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