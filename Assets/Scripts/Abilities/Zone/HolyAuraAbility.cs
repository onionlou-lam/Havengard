using UnityEngine;
using Havengard.Combat; 
using System.Collections.Generic;
using Havengard.Units;

namespace Havengard.Abilities
{
    [CreateAssetMenu(menuName = "Havengard/Abilities/Zone/Holy Aura")]
    public class HolyAuraAbility : ZoneAbility
    {
        [Header("Holy Aura Settings")]
        [SerializeField] private float healTickRate = 1f;
        [SerializeField] private float damageToUndeadMultiplier = 1.5f;
        
        [Header("Buff Effects")]
        [SerializeField] private float defenseBonus = 5f;
        [SerializeField] private float attackBonus = 3f;

        private void OnValidate()
        {
            // Ensure this is Holy type with healing enabled
            damageType = DamageType.Holy;
            canHeal = true;
            healingRatio = 0.7f; // 70% healing, 30% damage to enemies
        }

        public override void Activate(AbilityUser user, Vector3 targetPosition, GameObject targetEnemy)
        {
            base.Activate(user, targetPosition, targetEnemy);
            Debug.Log($"[HolyAuraAbility] Created Holy Aura - Healing Ratio: {healingRatio}");
        }

        protected void OnZoneTick(GameObject affectedTarget, AbilityUser user)
        {
            if (affectedTarget == null || user == null) return;

            bool isAlly = IsAllyTarget(user.gameObject, affectedTarget);

            if (isAlly)
            {
                // Heal allies
                float healing = CalculateHealing(user.gameObject, CurrentLevel);
                var targetHealth = affectedTarget.GetComponent<Havengard.Core.HealthSystem.Health>();
                if (targetHealth != null)
                {
                    targetHealth.Heal((int)healing);
                    Debug.Log($"[HolyAuraAbility] Healed {affectedTarget.name} for {healing}");
                }

                // Apply buffs to allies
                ApplyHolyBuffs(affectedTarget);
            }
            else
            {
                // Damage enemies (reduced by healing ratio)
                float damage = GetDamagePortion(user.gameObject, CurrentLevel);
                
                // Extra damage to undead (if you have enemy types)
                if (IsUndead(affectedTarget))
                {
                    damage *= damageToUndeadMultiplier;
                }

                var targetHealth = affectedTarget.GetComponent<Havengard.Core.HealthSystem.Health>();
                if (targetHealth != null)
                {
                    targetHealth.Damage((int)damage);
                    Debug.Log($"[HolyAuraAbility] Damaged {affectedTarget.name} for {damage}");
                }
            }

            // Spawn heal/damage VFX
            if (impactVFX != null)
            {
                GameObject vfx = Instantiate(impactVFX, affectedTarget.transform.position, Quaternion.identity);
                Destroy(vfx, 1f);
            }
        }

        private void ApplyHolyBuffs(GameObject target)
        {
            var stats = target.GetComponent<Havengard.Core.Character.StatsComponent>();
            if (stats != null)
            {
                // Apply temporary buffs (you might need to implement temporary modifiers)
                // For now, this is a placeholder
                Debug.Log($"[HolyAuraAbility] Applied holy buffs to {target.name}");
            }
        }

        private bool IsAllyTarget(GameObject caster, GameObject target)
        {
            var casterUnit = caster.GetComponent<UnitBase>();
            var targetUnit = target.GetComponent<UnitBase>();

            if (casterUnit == null || targetUnit == null)
                return false;

            // Use GetMyFaction() instead of .faction property
            return casterUnit.GetMyFaction() == targetUnit.GetMyFaction();
        }

        private bool IsUndead(GameObject target)
        {
            // Add undead detection logic here
            // Could check for a specific tag, component, or enemy type
            return target.CompareTag("Undead");
        }
    }
}