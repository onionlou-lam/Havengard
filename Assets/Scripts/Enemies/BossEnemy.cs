using UnityEngine;
using Havengard.HealthSystem;

namespace Havengard.Enemies
{
    public class BossEnemy : EnemyBase
    {
        [SerializeField] private float basicDamage = 15f;
        [SerializeField] private float specialDamage = 30f;
        [SerializeField] private float specialAbilityCooldown = 5f;

        private float lastAbilityTime;

        public override void PerformAttack(GameObject target)
        {
            if (target == null || !target.TryGetComponent<IHealth>(out var targetHealth)) return;

            if (Time.time >= lastAbilityTime + specialAbilityCooldown)
            {
                Debug.Log($"{name} uses special ability!");
                targetHealth.TakeDamage(specialDamage, health.GetFaction());
                lastAbilityTime = Time.time;
            }
            else
            {
                Debug.Log($"{name} uses basic attack!");
                targetHealth.TakeDamage(basicDamage, health.GetFaction());
            }
        }
    }
}
