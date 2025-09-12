using UnityEngine;
using Havengard.HealthSystem;

namespace Havengard.Enemies
{
    public class MeleeEnemy : EnemyBase
    {
        [SerializeField] private float damage = 10f;

        public override void PerformAttack(GameObject target)
        {
            if (target.TryGetComponent<IHealth>(out var targetHealth))
            {
                // Prevent friendly fire by passing this enemy's faction
                targetHealth.TakeDamage(damage, health.GetFaction());
            }
        }
    }
}
