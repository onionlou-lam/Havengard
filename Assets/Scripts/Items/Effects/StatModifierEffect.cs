using UnityEngine;
using Havengard.Heroes;
using Havengard.Character; // Correct namespace for StatsComponent

namespace Havengard.Items
{
    public enum StatType
    {
        MaxHealth,
        Attack,
        Defense,
        AttackSpeed,
        MoveSpeed,
        CritChance,
        CritMultiplier,
        MaxResource,
        ResourceRegen,
        CooldownReduction,
        LifeSteal,
        DodgeChance
    }

    [CreateAssetMenu(menuName = "Havengard/Items/Effects/Stat Modifier")]
    public class StatModifierEffect : ItemEffect
    {
        [Header("Stat Modifier")]
        public StatType statType;
        [Tooltip("Is this a percentage modifier (e.g., +10% instead of +10)?")]
        public bool isPercentage = false;

        public override void Apply(GameObject character, int level)
        {
            var stats = character.GetComponent<StatsComponent>();
            if (stats == null) return;

            float value = GetValue(level);
            var current = stats.CurrentStats;

            switch (statType)
            {
                case StatType.MaxHealth:
                    current.MaxHP += isPercentage ? (int)(current.MaxHP * value / 100f) : (int)value;
                    break;
                case StatType.Attack:
                    current.Attack += isPercentage ? (int)(current.Attack * value / 100f) : (int)value;
                    break;
                case StatType.Defense:
                    current.Defense += isPercentage ? (int)(current.Defense * value / 100f) : (int)value;
                    break;
                case StatType.AttackSpeed:
                    current.AttackSpeed += isPercentage ? current.AttackSpeed * value / 100f : value;
                    break;
                case StatType.MoveSpeed:
                    current.MoveSpeed += isPercentage ? current.MoveSpeed * value / 100f : value;
                    break;
                case StatType.CritChance:
                    current.CritChance += value / 100f;
                    break;
                case StatType.CritMultiplier:
                    current.CritMultiplier += value;
                    break;
                case StatType.MaxResource:
                    current.MaxResource += isPercentage ? (int)(current.MaxResource * value / 100f) : (int)value;
                    break;
            }
        }

        public override void Remove(GameObject character, int level)
        {
            var stats = character.GetComponent<StatsComponent>();
            if (stats == null) return;

            float value = GetValue(level);
            var current = stats.CurrentStats;

            switch (statType)
            {
                case StatType.MaxHealth:
                    current.MaxHP -= isPercentage ? (int)(current.MaxHP * value / 100f) : (int)value;
                    break;
                case StatType.Attack:
                    current.Attack -= isPercentage ? (int)(current.Attack * value / 100f) : (int)value;
                    break;
                case StatType.Defense:
                    current.Defense -= isPercentage ? (int)(current.Defense * value / 100f) : (int)value;
                    break;
                case StatType.AttackSpeed:
                    current.AttackSpeed -= isPercentage ? current.AttackSpeed * value / 100f : value;
                    break;
                case StatType.MoveSpeed:
                    current.MoveSpeed -= isPercentage ? current.MoveSpeed * value / 100f : value;
                    break;
                case StatType.CritChance:
                    current.CritChance -= value / 100f;
                    break;
                case StatType.CritMultiplier:
                    current.CritMultiplier -= value;
                    break;
                case StatType.MaxResource:
                    current.MaxResource -= isPercentage ? (int)(current.MaxResource * value / 100f) : (int)value;
                    break;
            }
        }

        public override string FormatDescription(string desc, int level)
        {
            float value = GetValue(level);
            string formatted = isPercentage ? $"{value:F1}%" : $"{value:F0}";
            return desc.Replace($"{{{statType}}}", formatted);
        }
    }
}