using UnityEngine;
using Havengard.HealthSystem;
using Havengard.Combat;   // FactionUtility

namespace Havengard.Units
{
    /// <summary>
    /// Enemy that deals direct melee damage.
    /// Respects faction rules (no friendly fire unless enabled).
    /// </summary>
    public class MeleeEnemy : EnemyUnit
    {
        [Header("Melee Settings")]
        [SerializeField] private int damage = 10;
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
                    targetHealth.GetHealthSystem().Damage(damage);
                    lastAttackTime = Time.time;
                }
            }
        }
    }
}
