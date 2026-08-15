using UnityEngine;
using System;

namespace Havengard.Core.Stats
{
    /// <summary>
    /// Unified stat modifier system for abilities, items, buffs, and investments
    /// </summary>
    [Serializable]
    public class StatModifier
    {
        public enum ModifierType
        {
            Flat,           // Adds a flat amount (+10 damage)
            PercentAdd,     // Adds percentage (10% more damage, stacks additively)
            PercentMult     // Multiplies percentage (10% more damage, stacks multiplicatively)
        }

        public enum StatType
        {
            // Ability Stats
            Damage,
            Cooldown,
            ResourceCost,
            Range,
            Duration,
            TickRate,
            ProjectileCount,
            ChainCount,

            // Combat Stats
            CritChance,
            CritDamage,
            Lifesteal,

            // Character Stats (from StatsComponent)
            Health,
            MaxHealth,
            Defense,
            AttackSpeed,
            MoveSpeed,
            DamageMultiplier,
            CooldownReduction
        }

        [Header("Modifier Configuration")]
        public StatType statType;
        public ModifierType modifierType;
        public float value;

        [Header("Scaling")]
        [Tooltip("Value added per investment level")]
        public float valuePerLevel = 0f;

        [Header("Source Info")]
        [Tooltip("What granted this modifier (Item name, Ability name, etc.)")]
        public string source = "";

        public StatModifier(StatType stat, ModifierType type, float val, string src = "")
        {
            statType = stat;
            modifierType = type;
            value = val;
            source = src;
        }

        /// <summary>
        /// Calculate modifier value at a specific level
        /// </summary>
        public float GetValueAtLevel(int level)
        {
            return value + (valuePerLevel * Mathf.Max(0, level - 1));
        }

        /// <summary>
        /// Apply this modifier to a base value
        /// </summary>
        public float Apply(float baseValue, int level = 1)
        {
            float modValue = GetValueAtLevel(level);

            return modifierType switch
            {
                ModifierType.Flat => baseValue + modValue,
                ModifierType.PercentAdd => baseValue * (1f + modValue),
                ModifierType.PercentMult => baseValue * (1f + modValue),
                _ => baseValue
            };
        }
    }
}