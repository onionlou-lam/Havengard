using UnityEngine;
using Havengard.Abilities;

namespace Havengard.Items
{
    [CreateAssetMenu(menuName = "Havengard/Items/Effects/Ability Modifier")]
    public class AbilityModifierEffect : ItemEffect
    {
        [Header("Ability Modification")]
        [Tooltip("Leave empty to affect all abilities")]
        public AbilityBase targetAbility;
        
        public enum ModifierType
        {
            CooldownReduction,
            DamageIncrease,
            RangeIncrease,
            CostReduction
        }
        
        public ModifierType modifierType;
        public float baseValue = 0.1f; // 10% by default
        public float perLevelValue = 0.05f; // +5% per level

        [Header("Display")]
        [Tooltip("Short name for floating text (e.g., 'CDR', 'Dmg+', 'Range+'). Leave empty to use default.")]
        public string shortDisplayName;

        public override void Apply(GameObject target, int level)
        {
            if (target == null) return;

            var abilityUser = target.GetComponent<AbilityUser>();
            if (abilityUser == null)
            {
                Debug.LogWarning($"[AbilityModifierEffect] {target.name} has no AbilityUser component");
                return;
            }

            float totalValue = baseValue + (perLevelValue * (level - 1));

            // Apply to specific ability or all abilities
            if (targetAbility != null)
            {
                ApplyToAbility(abilityUser, targetAbility, totalValue, level);
            }
            else
            {
                // Apply to all abilities
                for (int i = 0; ; i++)
                {
                    var ability = abilityUser.GetAbility(i);
                    if (ability == null)
                        break;
                    ApplyToAbility(abilityUser, ability, totalValue, level);
                }
            }

            Debug.Log($"[AbilityModifierEffect] Applied {modifierType} +{totalValue} to {target.name}");
        }

        private void ApplyToAbility(AbilityUser abilityUser, AbilityBase ability, float value, int level)
        {
            if (ability == null) return;

            switch (modifierType)
            {
                case ModifierType.CooldownReduction:
                    // Implement cooldown reduction (you'll need to add this to AbilityBase)
                    // ability.baseCooldown *= (1f - value);
                    break;
                    
                case ModifierType.DamageIncrease:
                    // Implement damage increase
                    break;
                    
                case ModifierType.RangeIncrease:
                    // Implement range increase
                    break;
                    
                case ModifierType.CostReduction:
                    // Implement cost reduction
                    break;
            }
        }

        public override void Remove(GameObject target, int level)
        {
            // Implement removal logic (reverse of Apply)
        }

        public override float GetValue(int level)
        {
            return baseValue + (perLevelValue * (level - 1));
        }

        public override string FormatDescription(string description, int level)
        {
            float value = GetValue(level) * 100f; // Convert to percentage
            return description.Replace("{value}", value.ToString("F0") + "%");
        }

        /// <summary>
        /// Get the display name for floating text
        /// </summary>
        public string GetShortDisplayName()
        {
            if (!string.IsNullOrEmpty(shortDisplayName))
            {
                return shortDisplayName;
            }

            // Default short names
            return modifierType switch
            {
                ModifierType.CooldownReduction => "CDR",
                ModifierType.DamageIncrease => "Ability Dmg",
                ModifierType.RangeIncrease => "Range",
                ModifierType.CostReduction => "Cost Red",
                _ => modifierType.ToString()
            };
        }

        /// <summary>
        /// Get formatted ability bonus text for floating display
        /// </summary>
        public string GetAbilityBonusText(int level)
        {
            float value = GetValue(level) * 100f; // Convert to percentage
            string displayName = GetShortDisplayName();

            // Use the public property AbilityName instead of the private field abilityName
            string abilityName = targetAbility != null ? targetAbility.AbilityName : "All Abilities";

            return $"+{value:F0}% {displayName}";
        }
    }
}