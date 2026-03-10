using UnityEngine;
using System.Collections.Generic;
using Havengard.Stats;
using Havengard.Units;
using Havengard.Statuses;

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
        public ResourceType resourceType = ResourceType.Mana;

        [Header("Damage & Effects")]
        public float baseDamage = 10f;
        public float damagePerLevel = 5f;
        public List<StatusEffectData> statusEffects;

        [Header("Lifesteal")]
        [Range(0f, 1f)]
        public float lifestealPercent = 0f;

        [Header("Targeting")]
        public float range = 10f;
        public LayerMask targetLayers;
        public bool requiresTarget = true;
        public TargetingMode targetingMode = TargetingMode.Hostile;

        [Header("Visual & Audio")]
        public GameObject castVFX;
        public GameObject impactVFX;
        public AudioClip castSFX;
        public AudioClip impactSFX;

        // Current level tracking
        protected int currentLevel = 1;

        public int CurrentLevel
        {
            get => currentLevel;
            set => currentLevel = Mathf.Clamp(value, 1, maxLevel);
        }

        /// <summary>
        /// Calculate final damage/healing with level scaling and damage type bonuses
        /// </summary>
        public float CalculateFinalDamage(GameObject caster, int level = -1)
        {
            if (level < 0) level = currentLevel;

            float damage = baseDamage + (damagePerLevel * (level - 1));

            // Apply damage type bonus from PlayerStatAllocator
            if (caster != null)
            {
                var statAllocator = caster.GetComponent<PlayerStatAllocator>();
                if (statAllocator != null)
                {
                    float bonus = statAllocator.GetDamageTypeBonus(damageType);
                    damage *= (1f + bonus);
                }

                // Apply attack stat
                var stats = caster.GetComponent<Havengard.Stats.StatsComponent>();
                if (stats != null)
                {
                    float attackBonus = stats.GetStatValue("Attack");
                    damage += attackBonus;
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

            float baseDamageValue = CalculateFinalDamage(caster, level);
            return baseDamageValue * healingRatio;
        }

        /// <summary>
        /// Get the effective damage portion (after healing is subtracted)
        /// </summary>
        public float GetDamagePortion(GameObject caster, int level = -1)
        {
            if (damageType != DamageType.Holy || !canHeal)
                return CalculateFinalDamage(caster, level);

            float baseDamageValue = CalculateFinalDamage(caster, level);
            return baseDamageValue * (1f - healingRatio);
        }

        /// <summary>
        /// Apply damage or healing to a target based on damage type and targeting
        /// </summary>
        public void ApplyDamageOrHealing(GameObject caster, GameObject target, int level = -1)
        {
            if (target == null) return;

            var targetHealth = target.GetComponent<Havengard.HealthSystem.Health>();
            if (targetHealth == null) return;

            // Holy damage with healing
            if (damageType == DamageType.Holy && canHeal)
            {
                // Check if target is ally or enemy
                bool isAlly = IsAlly(caster, target);

                if (isAlly && healingRatio > 0f)
                {
                    // Heal ally
                    float healing = CalculateHealing(caster, level);
                    targetHealth.Heal((int)healing);
                    
                    Debug.Log($"[AbilityBase] {abilityName} healed {target.name} for {healing}");
                    
                    // Spawn healing VFX
                    if (impactVFX != null)
                    {
                        GameObject vfx = Instantiate(impactVFX, target.transform.position, Quaternion.identity);
                        Destroy(vfx, 2f);
                    }
                }
                else
                {
                    // Damage enemy (reduced by healing ratio)
                    float damage = GetDamagePortion(caster, level);
                    ApplyDamageInternal(caster, target, damage);
                }
            }
            else
            {
                // Standard damage
                float damage = CalculateFinalDamage(caster, level);
                ApplyDamageInternal(caster, target, damage);
            }
        }

        private void ApplyDamageInternal(GameObject caster, GameObject target, float damage)
        {
            var targetHealth = target.GetComponent<Havengard.HealthSystem.Health>();
            if (targetHealth == null) return;

            // Apply damage
            int finalDamage = Mathf.RoundToInt(damage);
            targetHealth.Damage(finalDamage);

            // Apply status effects
            if (statusEffects != null && statusEffects.Count > 0)
            {
                foreach (var effect in statusEffects)
                {
                    if (effect != null)
                        StatusEffectApplier.ApplyEffect(target, effect);
                }
            }

            // Lifesteal
            if (lifestealPercent > 0f && caster != null)
            {
                var casterHealth = caster.GetComponent<Havengard.HealthSystem.Health>();
                if (casterHealth != null)
                {
                    int healAmount = Mathf.RoundToInt(finalDamage * lifestealPercent);
                    casterHealth.Heal(healAmount);
                }
            }

            // VFX
            if (impactVFX != null)
            {
                GameObject vfx = Instantiate(impactVFX, target.transform.position, Quaternion.identity);
                Destroy(vfx, 2f);
            }

            // SFX
            if (impactSFX != null)
            {
                AudioSource.PlayClipAtPoint(impactSFX, target.transform.position);
            }
        }

        /// <summary>
        /// Check if target is ally (same faction)
        /// </summary>
        private bool IsAlly(GameObject caster, GameObject target)
        {
            var casterUnit = caster.GetComponent<UnitBase>();
            var targetUnit = target.GetComponent<UnitBase>();

            if (casterUnit == null || targetUnit == null)
                return false;

            // Use GetMyFaction() method, ensure it's accessible (change to public or internal in UnitBase)
            return casterUnit.GetMyFaction() == targetUnit.GetMyFaction();
        }

        /// <summary>
        /// Get description with level-specific values
        /// </summary>
        public virtual string GetScaledDescription(GameObject caster, int level = -1)
        {
            if (level < 0) level = currentLevel;

            string desc = description;
            
            // Replace damage placeholder
            float damage = CalculateFinalDamage(caster, level);
            desc = desc.Replace("{damage}", damage.ToString("F0"));

            // Replace healing placeholder
            if (canHeal && damageType == DamageType.Holy)
            {
                float healing = CalculateHealing(caster, level);
                desc = desc.Replace("{healing}", healing.ToString("F0"));
            }

            // Replace level placeholder
            desc = desc.Replace("{level}", level.ToString());

            // Replace damage type
            desc = desc.Replace("{damageType}", damageType.ToString());

            return desc;
        }

        public abstract void Activate(AbilityUser user, Vector3 targetPosition, GameObject targetEnemy);
        public abstract void Deactivate(AbilityUser user);
    }

    public enum TargetingMode
    {
        Hostile,
        Friendly,
        Both,
        Self,
        Ground
    }

    public enum ResourceType
    {
        Mana,
        Energy,
        Rage,
        None
    }
}