using UnityEngine;
using Havengard.HealthSystem;
using Havengard.Combat;

namespace Havengard.Units
{
    /// <summary>
    /// Enemy that deals direct melee damage.
    /// Respects faction rules and uses CombatCalculator for damage.
    /// </summary>
    public class MeleeEnemy : EnemyUnit
    {
        [Header("Melee Settings")]
        [SerializeField] private float attackCooldown = 1f;
        [SerializeField] private bool friendlyFire = false;

        private float lastAttackTime;

        protected override void PerformAttack(GameObject target)
        {
            if (Time.time < lastAttackTime + attackCooldown) return;
            if (target == null) return;

            if (Vector2.Distance(transform.position, target.transform.position) <= attackRange)
            {
                var targetHealth = target.GetComponent<IHealth>();
                if (FactionUtility.CanDamage(GetMyFaction(), targetHealth, friendlyFire))
                {
                    // Calculate damage using CombatCalculator
                    int finalDamage = CombatCalculator.CalculateDamage(gameObject, target);
                    targetHealth.GetHealthSystem().Damage(finalDamage);

                    lastAttackTime = Time.time;
                }
            }
        }
    }
}
