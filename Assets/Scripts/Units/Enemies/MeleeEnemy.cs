using UnityEngine;
using Havengard.Abilities;

namespace Havengard.Units
{
    public class MeleeEnemy : UnitBase
    {
        [Header("Melee")]
        [SerializeField] private AbilityBase attackAbility; // assign Cleave here
        private float lastAttackTime;

        protected override void PerformAttack(GameObject target)
        {
            if (attackAbility == null) return;

            // Cooldown gate
            if (Time.time < lastAttackTime + attackAbility.baseCooldown) return;

            // CanCast gate
            if (!attackAbility.CanCast(gameObject, target)) return;

            TriggerAttackAnim();
            attackAbility.Cast(gameObject, target);

            lastAttackTime = Time.time;
        }
    }
}
