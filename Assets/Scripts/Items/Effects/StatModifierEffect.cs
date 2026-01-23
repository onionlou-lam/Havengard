using UnityEngine;
using Havengard.Character;

namespace Havengard.Items
{
    [CreateAssetMenu(menuName = "Havengard/Items/Effects/Stat Modifier")]
    public class StatModifierEffect : ItemEffect
    {
        public enum StatType
        {
            MaxHP,
            Attack,
            Defense,
            MaxResource,
            AttackSpeed,
            MoveSpeed,
            CritChance,
            CritMultiplier
        }

        [Header("Stat Modification")]
        public StatType statType;
        public float baseValue = 10f;
        public float perLevelValue = 5f;

        public override void Apply(GameObject target, int level)
        {
            if (target == null)
            {
                Debug.LogWarning("[StatModifierEffect] Target is null");
                return;
            }

            StatsComponent stats = target.GetComponent<StatsComponent>();
            if (stats == null || stats.CurrentStats == null)
            {
                Debug.LogWarning($"[StatModifierEffect] {target.name} has no StatsComponent or CurrentStats");
                return;
            }

            float totalValue = baseValue + (perLevelValue * (level - 1));
            
            Debug.Log($"[StatModifierEffect] Applying {statType} +{totalValue} to {target.name} (Level {level})");

            switch (statType)
            {
                case StatType.MaxHP:
                    stats.CurrentStats.MaxHP += Mathf.RoundToInt(totalValue);
                    // Update health system
                    var health = target.GetComponent<Havengard.HealthSystem.Health>();
                    if (health != null)
                    {
                        health.SetMaxHealthFromStats(refill: false);
                    }
                    break;
                    
                case StatType.Attack:
                    stats.CurrentStats.Attack += Mathf.RoundToInt(totalValue);
                    break;
                    
                case StatType.Defense:
                    stats.CurrentStats.Defense += Mathf.RoundToInt(totalValue);
                    break;
                    
                case StatType.MaxResource:
                    stats.CurrentStats.MaxResource += Mathf.RoundToInt(totalValue);
                    break;
                    
                case StatType.AttackSpeed:
                    stats.CurrentStats.AttackSpeed += totalValue;
                    break;
                    
                case StatType.MoveSpeed:
                    stats.CurrentStats.MoveSpeed += totalValue;
                    break;
                    
                case StatType.CritChance:
                    stats.CurrentStats.CritChance += totalValue;
                    break;
                    
                case StatType.CritMultiplier:
                    stats.CurrentStats.CritMultiplier += totalValue;
                    break;
            }

            Debug.Log($"[StatModifierEffect] New {statType}: {GetCurrentStatValue(stats, statType)}");
        }

        public override void Remove(GameObject target, int level)
        {
            if (target == null) return;

            StatsComponent stats = target.GetComponent<StatsComponent>();
            if (stats == null || stats.CurrentStats == null) return;

            float totalValue = baseValue + (perLevelValue * (level - 1));
            
            Debug.Log($"[StatModifierEffect] Removing {statType} -{totalValue} from {target.name}");

            switch (statType)
            {
                case StatType.MaxHP:
                    stats.CurrentStats.MaxHP -= Mathf.RoundToInt(totalValue);
                    stats.CurrentStats.MaxHP = Mathf.Max(1, stats.CurrentStats.MaxHP);
                    var health = target.GetComponent<Havengard.HealthSystem.Health>();
                    if (health != null)
                    {
                        health.SetMaxHealthFromStats(refill: false);
                    }
                    break;
                    
                case StatType.Attack:
                    stats.CurrentStats.Attack -= Mathf.RoundToInt(totalValue);
                    stats.CurrentStats.Attack = Mathf.Max(0, stats.CurrentStats.Attack);
                    break;
                    
                case StatType.Defense:
                    stats.CurrentStats.Defense -= Mathf.RoundToInt(totalValue);
                    stats.CurrentStats.Defense = Mathf.Max(0, stats.CurrentStats.Defense);
                    break;
                    
                case StatType.MaxResource:
                    stats.CurrentStats.MaxResource -= Mathf.RoundToInt(totalValue);
                    stats.CurrentStats.MaxResource = Mathf.Max(1, stats.CurrentStats.MaxResource);
                    break;
                    
                case StatType.AttackSpeed:
                    stats.CurrentStats.AttackSpeed -= totalValue;
                    break;
                    
                case StatType.MoveSpeed:
                    stats.CurrentStats.MoveSpeed -= totalValue;
                    break;
                    
                case StatType.CritChance:
                    stats.CurrentStats.CritChance -= totalValue;
                    break;
                    
                case StatType.CritMultiplier:
                    stats.CurrentStats.CritMultiplier -= totalValue;
                    break;
            }
        }

        public override float GetValue(int level)
        {
            return baseValue + (perLevelValue * (level - 1));
        }

        public override string FormatDescription(string description, int level)
        {
            float value = GetValue(level);
            return description.Replace("{value}", value.ToString("F1"));
        }

        private float GetCurrentStatValue(StatsComponent stats, StatType type)
        {
            return type switch
            {
                StatType.MaxHP => stats.CurrentStats.MaxHP,
                StatType.Attack => stats.CurrentStats.Attack,
                StatType.Defense => stats.CurrentStats.Defense,
                StatType.MaxResource => stats.CurrentStats.MaxResource,
                StatType.AttackSpeed => stats.CurrentStats.AttackSpeed,
                StatType.MoveSpeed => stats.CurrentStats.MoveSpeed,
                StatType.CritChance => stats.CurrentStats.CritChance,
                StatType.CritMultiplier => stats.CurrentStats.CritMultiplier,
                _ => 0
            };
        }
    }
}