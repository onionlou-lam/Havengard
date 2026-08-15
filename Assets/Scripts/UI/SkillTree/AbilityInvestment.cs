using UnityEngine;
using System.Collections.Generic;
using Havengard.Core.Stats;

namespace Havengard.Abilities
{
    /// <summary>
    /// Tracks ability investments and their stat bonuses
    /// </summary>
    [System.Serializable]
    public class AbilityInvestment
    {
        public int investmentLevel = 0;
        public const int MAX_INVESTMENT = 20;

        [Header("Investment Bonuses (Applied per level)")]
        [Tooltip("Define which stats scale with investment")]
        public List<StatModifier> investmentBonuses = new List<StatModifier>();

        /// <summary>
        /// Get the total modifier for a stat type at current investment level
        /// </summary>
        public float GetTotalModifier(StatModifier.StatType statType)
        {
            float total = 0f;
            foreach (var bonus in investmentBonuses)
            {
                if (bonus.statType == statType)
                {
                    total += bonus.GetValueAtLevel(investmentLevel);
                }
            }
            return total;
        }

        /// <summary>
        /// Check if can invest more points
        /// </summary>
        public bool CanInvest()
        {
            return investmentLevel < MAX_INVESTMENT;
        }

        /// <summary>
        /// Add an investment level
        /// </summary>
        public bool Invest()
        {
            if (!CanInvest()) return false;
            investmentLevel++;
            return true;
        }

        /// <summary>
        /// Get preview of next level bonuses
        /// </summary>
        public string GetNextLevelPreview()
        {
            if (!CanInvest()) return "MAX";

            string preview = "";
            foreach (var bonus in investmentBonuses)
            {
                float currentValue = bonus.GetValueAtLevel(investmentLevel);
                float nextValue = bonus.GetValueAtLevel(investmentLevel + 1);
                float increase = nextValue - currentValue;

                string sign = increase > 0 ? "+" : "";
                preview += $"{bonus.statType}: {sign}{increase:F1}";

                if (bonus.modifierType == StatModifier.ModifierType.PercentAdd ||
                    bonus.modifierType == StatModifier.ModifierType.PercentMult)
                {
                    preview += "%";
                }
                preview += " ";
            }
            return preview.TrimEnd();
        }

        /// <summary>
        /// Get current level info
        /// </summary>
        public string GetCurrentLevelInfo(int level)
        {
            if (level == 0) return "No investment";

            string info = "";
            foreach (var bonus in investmentBonuses)
            {
                float value = bonus.GetValueAtLevel(level);
                string sign = value > 0 ? "+" : "";
                info += $"{bonus.statType}: {sign}{value:F1}";

                if (bonus.modifierType == StatModifier.ModifierType.PercentAdd ||
                    bonus.modifierType == StatModifier.ModifierType.PercentMult)
                {
                    info += "%";
                }
                info += " ";
            }
            return info.TrimEnd();
        }

        /// <summary>
        /// Apply investment bonuses to the ability
        /// </summary>
        public void ApplyInvestment(int level, AbilityBase ability)
        {
            if (ability == null) return;

            investmentLevel = level;

            // Apply stat modifiers based on investment level
            foreach (var bonus in investmentBonuses)
            {
                float value = bonus.GetValueAtLevel(level);
                
                switch (bonus.statType)
                {
                    case StatModifier.StatType.Damage:
                        ability.baseDamage += value;
                        break;
                    case StatModifier.StatType.Cooldown:
                        ability.baseCooldown = Mathf.Max(0.1f, ability.baseCooldown - value);
                        break;
                    case StatModifier.StatType.Range:
                        ability.range += value;
                        break;
                    case StatModifier.StatType.ResourceCost:
                        ability.resourceCost = Mathf.Max(0, ability.resourceCost - Mathf.RoundToInt(value));
                        break;
                }
            }
        }
    }
}