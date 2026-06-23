using UnityEngine;
using System.Collections.Generic;
using Havengard.Combat;

namespace Havengard.Abilities
{
    public abstract class AbilityBase : ScriptableObject
    {
        [Header("Basic Info")]
        public string abilityName = "New Ability";
        [TextArea(3, 5)]
        public string description;
        public Sprite icon;
        public int maxLevel = 5;

        [Header("Damage Configuration")]
        public DamageType damageType = DamageType.Physical;
        [Tooltip("If true, this ability can heal allies when damageType is Holy")]
        public bool canHeal = false;
        [Range(0f, 1f)]
        [Tooltip("For Holy damage: percentage that heals instead of damages (0 = pure damage, 1 = pure heal)")]
        public float healingRatio = 0f;

        [Header("Cooldown & Resource")]
        public float baseCooldown = 1f;
        public int resourceCost = 10;

        [Header("Damage & Effects")]
        public float baseDamage = 10f;
        public float damagePerLevel = 5f;

        [Header("Lifesteal")]
        [Range(0f, 1f)]
        public float lifestealPercent = 0f;

        [Header("Targeting")]
        public float range = 10f;
        public LayerMask targetLayers;
        public bool requiresTarget = true;

        [Header("Visual & Audio")]
        public GameObject castVFX;
        public GameObject impactVFX;
        public AudioClip castSFX;
        public AudioClip impactSFX;

        [Header("Sub-Skills / Modifiers (Future)")]
        [Tooltip("Active sub-skills that modify this ability's behavior")]
        public List<AbilitySubSkill> activeSubSkills = new List<AbilitySubSkill>();

        protected int currentLevel = 1;

        public int CurrentLevel
        {
            get => currentLevel;
            set => currentLevel = Mathf.Clamp(value, 1, maxLevel);
        }

        /// <summary>
        /// Calculate final damage with level scaling and damage type bonuses
        /// </summary>
        public virtual float CalculateDamage(GameObject caster = null, int level = -1)
        {
            if (level < 0) level = currentLevel;

            float damage = baseDamage + (damagePerLevel * (level - 1));

            // Apply damage type bonus from PlayerStatAllocator (if exists)
            if (caster != null)
            {
                var statAllocator = caster.GetComponent<Havengard.Stats.PlayerStatAllocator>();
                if (statAllocator != null)
                {
                    float bonus = statAllocator.GetDamageTypeBonus(damageType);
                    damage *= (1f + bonus);
                }
            }

            // Apply sub-skill modifiers
            foreach (var subSkill in activeSubSkills)
            {
                if (subSkill != null && subSkill.modifiesDamage)
                {
                    damage *= subSkill.damageMultiplier;
                }
            }

            return damage;
        }

        /// <summary>
        /// Calculate healing amount (for Holy abilities)
        /// </summary>
        public float CalculateHealing(GameObject caster, int level = -1)
        {
            if (damageType != DamageType.Holy || !canHeal)
                return 0f;

            float baseDamageValue = CalculateDamage(caster, level);
            return baseDamageValue * healingRatio;
        }

        /// <summary>
        /// Get effective cooldown with modifiers applied
        /// </summary>
        public virtual float GetEffectiveCooldown()
        {
            float cooldown = baseCooldown;

            // Apply sub-skill cooldown reduction
            foreach (var subSkill in activeSubSkills)
            {
                if (subSkill != null && subSkill.modifiesCooldown)
                {
                    cooldown *= (1f - subSkill.cooldownReduction);
                }
            }

            return Mathf.Max(0.1f, cooldown); // Minimum 0.1s cooldown
        }

        /// <summary>
        /// Get effective resource cost with modifiers applied
        /// </summary>
        public virtual int GetEffectiveResourceCost()
        {
            float cost = resourceCost;

            // Apply sub-skill cost reduction
            foreach (var subSkill in activeSubSkills)
            {
                if (subSkill != null && subSkill.modifiesCost)
                {
                    cost *= (1f - subSkill.costReduction);
                }
            }

            return Mathf.Max(0, Mathf.RoundToInt(cost));
        }

        private Color GetDamageTypeColor()
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

        public abstract void Activate(AbilityUser user, Vector3 targetPosition, GameObject targetEnemy);
        public abstract void Deactivate(AbilityUser user);
    }
}