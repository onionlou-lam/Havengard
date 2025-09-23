using UnityEngine;

namespace Havengard.Units
{
    /// <summary>
    /// Boss enemy: uses abilities with special cooldowns.
    /// </summary>
    public class BossEnemy : EnemyUnit
    {
        [SerializeField] private float specialCooldown = 5f;
        [SerializeField] private float basicCooldown = 1.5f;

        private float lastAttackTime;

        protected override void PerformAttack(GameObject target)
        {
            if (abilityUser == null || target == null) return;
            if (Time.time < lastAttackTime + basicCooldown) return;

            if (Time.time >= lastAttackTime + specialCooldown)
            {
                Debug.Log($"{name} uses special ability!");
                abilityUser.UseAbility(1, target); // assume index 1 = special
            }
            else
            {
                abilityUser.UseAbility(0, target); // basic ability
            }

            lastAttackTime = Time.time;
        }
    }
}
