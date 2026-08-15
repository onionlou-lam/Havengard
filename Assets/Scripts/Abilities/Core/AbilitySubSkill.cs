using UnityEngine;
using Havengard.Statuses;

namespace Havengard.Abilities
{
    /// <summary>
    /// Represents a sub-skill or modifier that enhances an ability.
    /// Examples: Multi-shot, Chain Lightning, Cooldown Reduction, Explosive Impact
    /// </summary>
    [CreateAssetMenu(menuName = "Havengard/Abilities/Sub-Skill Modifier")]
    public class AbilitySubSkill : ScriptableObject
    {
        [Header("Sub-Skill Info")]
        public string subSkillName = "New Sub-Skill";
        [TextArea(2, 3)]
        public string description;
        public Sprite icon;

        [Header("Damage Modifiers")]
        public bool modifiesDamage = false;
        [Tooltip("Damage multiplier (1.0 = no change, 1.25 = +25% damage)")]
        public float damageMultiplier = 1.0f;

        [Header("Cooldown Modifiers")]
        public bool modifiesCooldown = false;
        [Range(0f, 0.9f)]
        [Tooltip("Cooldown reduction (0.2 = -20% cooldown)")]
        public float cooldownReduction = 0f;

        [Header("Cost Modifiers")]
        public bool modifiesCost = false;
        [Range(0f, 0.9f)]
        [Tooltip("Resource cost reduction (0.15 = -15% cost)")]
        public float costReduction = 0f;

        [Header("Projectile Modifiers")]
        public bool addsProjectiles = false;
        [Tooltip("Additional projectiles to fire")]
        public int additionalProjectiles = 0;
        [Tooltip("Spread angle for multiple projectiles")]
        public float spreadAngle = 15f;

        [Header("Piercing Modifiers")]
        public bool enablesPiercing = false;
        [Tooltip("Number of enemies the projectile can pierce through (0 = infinite piercing)")]
        public int pierceCount = 1;
        [Tooltip("Damage reduction per pierce (0.1 = -10% per enemy pierced, 0 = no reduction)")]
        [Range(0f, 1f)]
        public float pierceDamageReduction = 0f;

        [Header("Chain Modifiers")]
        public bool enablesChaining = false;
        [Tooltip("Number of times ability chains to nearby enemies")]
        public int chainCount = 0;
        [Tooltip("Maximum range for chain jumps")]
        public float chainRange = 5f;
        [Tooltip("Damage reduction per chain (0.25 = -25% per jump)")]
        [Range(0f, 1f)]
        public float chainDamageReduction = 0.25f;

        [Header("AoE Modifiers")]
        public bool addsExplosion = false;
        [Tooltip("Radius of explosion effect")]
        public float explosionRadius = 5f;
        [Tooltip("Damage multiplier for explosion (0.5 = 50% of ability damage)")]
        [Range(0f, 2f)]
        public float explosionDamageMultiplier = 0.5f;

        [Header("Status Effect Modifiers")]
        public bool appliesStatusEffect = false;
        public StatusEffectData statusEffect;
        [Tooltip("Chance to apply status effect (1.0 = 100%)")]
        [Range(0f, 1f)]
        public float statusEffectChance = 1f;

        /// <summary>
        /// Get formatted description for tooltip display
        /// </summary>
        public string GetTooltipText()
        {
            string text = $"<b>{subSkillName}</b>\n";
            text += description;

            // Add modifier details
            if (modifiesDamage && damageMultiplier != 1f)
            {
                float percent = (damageMultiplier - 1f) * 100f;
                text += $"\n• Damage: {(percent >= 0 ? "+" : "")}{percent:F0}%";
            }

            if (modifiesCooldown && cooldownReduction > 0f)
            {
                text += $"\n• Cooldown: -{(cooldownReduction * 100):F0}%";
            }

            if (modifiesCost && costReduction > 0f)
            {
                text += $"\n• Cost: -{(costReduction * 100):F0}%";
            }

            if (addsProjectiles && additionalProjectiles > 0)
            {
                text += $"\n• Fires {additionalProjectiles + 1} projectiles";
            }

            if (enablesPiercing)
            {
                if (pierceCount > 0)
                    text += $"\n• Pierces {pierceCount} enemies";
                else
                    text += $"\n• Pierces all enemies";

                if (pierceDamageReduction > 0f)
                {
                    text += $" (-{(pierceDamageReduction * 100):F0}% per pierce)";
                }
            }

            if (enablesChaining && chainCount > 0)
            {
                text += $"\n• Chains {chainCount} times";
            }

            if (addsExplosion)
            {
                text += $"\n• Creates {explosionRadius:F0}m explosion";
            }

            return text;
        }
    }
}