using UnityEngine;

namespace Havengard.Core.Character
{
    [System.Serializable]
    public class HeroStats
    {
        [Header("Core Stats")]
        public int MaxHP = 100;
        public int MaxResource = 50;

        [Header("Combat Stats")]
        public int Attack = 10;
        public int Defense = 5;
        public float AttackSpeed = 1f;

        [Header("Critical Stats")]
        [Range(0f, 1f)]
        public float CritChance = 0.05f;
        public float CritMultiplier = 1.5f;

        [Header("Movement")]
        public float MoveSpeed = 5f;

        [Header("Regeneration")]
        [Tooltip("Health regenerated per second")]
        public float HealthRegenRate = 0f;
        [Tooltip("Resource regenerated per second")]
        public float ResourceRegenRate = 5f;
        [Tooltip("Delay before health regeneration starts after taking damage (seconds)")]
        public float HealthRegenDelay = 5f;
        [Tooltip("Delay before resource regeneration starts after spending resource (seconds)")]
        public float ResourceRegenDelay = 2f;

        [Header("Cooldown Reduction")]
        [Range(0f, 0.9f)]
        [Tooltip("Global cooldown reduction (0.2 = 20% faster cooldowns)")]
        public float CooldownReduction = 0f;

        [Header("Damage Modifiers")]
        [Tooltip("Bonus damage multiplier (0.1 = +10% damage)")]
        public float DamageMultiplier = 0f;
        [Tooltip("Lifesteal percentage (0.1 = 10% lifesteal)")]
        [Range(0f, 1f)]
        public float Lifesteal = 0f;

        [Header("Resource Generation")]
        [Tooltip("Bonus resource generation on hit (0.05 = +5% of damage as resource)")]
        [Range(0f, 1f)]
        public float ResourceGenerationPercent = 0f;
        [Tooltip("Flat resource generated on hit")]
        public int FlatResourceGeneration = 0;

        public HeroStats Clone()
        {
            return new HeroStats
            {
                MaxHP = MaxHP,
                MaxResource = MaxResource,
                Attack = Attack,
                Defense = Defense,
                AttackSpeed = AttackSpeed,
                CritChance = CritChance,
                CritMultiplier = CritMultiplier,
                MoveSpeed = MoveSpeed,
                HealthRegenRate = HealthRegenRate,
                ResourceRegenRate = ResourceRegenRate,
                HealthRegenDelay = HealthRegenDelay,
                ResourceRegenDelay = ResourceRegenDelay,
                CooldownReduction = CooldownReduction,
                DamageMultiplier = DamageMultiplier,
                Lifesteal = Lifesteal,
                ResourceGenerationPercent = ResourceGenerationPercent,
                FlatResourceGeneration = FlatResourceGeneration
            };
        }

        /// <summary>
        /// Apply modifiers from another HeroStats (for buffs/items)
        /// </summary>
        public void ApplyModifiers(HeroStats modifiers, bool isMultiplicative = false)
        {
            if (modifiers == null) return;

            if (isMultiplicative)
            {
                // Multiplicative modifiers (percentages)
                AttackSpeed *= (1f + modifiers.AttackSpeed);
                MoveSpeed *= (1f + modifiers.MoveSpeed);
                Attack = Mathf.RoundToInt(Attack * (1f + modifiers.DamageMultiplier));
            }
            else
            {
                // Additive modifiers (flat values)
                MaxHP += modifiers.MaxHP;
                MaxResource += modifiers.MaxResource;
                Attack += modifiers.Attack;
                Defense += modifiers.Defense;
                AttackSpeed += modifiers.AttackSpeed;
                CritChance = Mathf.Clamp01(CritChance + modifiers.CritChance);
                CritMultiplier += modifiers.CritMultiplier;
                MoveSpeed += modifiers.MoveSpeed;
                HealthRegenRate += modifiers.HealthRegenRate;
                ResourceRegenRate += modifiers.ResourceRegenRate;
                CooldownReduction = Mathf.Clamp01(CooldownReduction + modifiers.CooldownReduction);
                DamageMultiplier += modifiers.DamageMultiplier;
                Lifesteal = Mathf.Clamp01(Lifesteal + modifiers.Lifesteal);
                ResourceGenerationPercent = Mathf.Clamp01(ResourceGenerationPercent + modifiers.ResourceGenerationPercent);
                FlatResourceGeneration += modifiers.FlatResourceGeneration;
            }
        }

        /// <summary>
        /// Calculate attack cooldown based on attack speed
        /// </summary>
        public float GetAttackCooldown()
        {
            return AttackSpeed > 0 ? 1f / AttackSpeed : 1f;
        }

        /// <summary>
        /// Calculate if an attack critically hits
        /// </summary>
        public bool RollCriticalHit()
        {
            return Random.value <= CritChance;
        }

        /// <summary>
        /// Apply critical multiplier to damage
        /// </summary>
        public float ApplyCriticalDamage(float baseDamage)
        {
            return baseDamage * CritMultiplier;
        }
    }
}