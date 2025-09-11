using UnityEngine;
using Havengard.HealthSystem;

namespace Havengard.Enemies
{
    public class MeleeEnemy : EnemyBase
    {
        [SerializeField] private float damage = 10f;

        public override void PerformAttack(GameObject target)
        {
            if (target.TryGetComponent<Health>(out var targetHealth))
            {
                // Pass this enemy's faction so friendly fire is prevented
                targetHealth.TakeDamage(damage, health.GetFaction());
            }
        }
    }
}
