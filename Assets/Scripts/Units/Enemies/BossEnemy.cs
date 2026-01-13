using UnityEngine;
using Havengard.Abilities;
using Havengard.HealthSystem;
using Havengard.Combat;

namespace Havengard.Units
{
    /// <summary>
    /// Boss variant of the RangedEnemy with higher damage and unique visuals.
    /// </summary>
    public class BossEnemy : RangedEnemy
    {
        [Header("Boss Settings")]
        [SerializeField] private float bossAttackCooldown = 1.0f;
        [SerializeField] private int bossProjectileDamage = 25;

        protected override void PerformAttack(GameObject target)
        {
            if (Time.time < lastAttackTime + bossAttackCooldown || target == null) return;

            var targetHealth = target.GetComponent<IHealth>();
            if (!FactionUtility.CanDamage(GetMyFaction(), targetHealth, false)) return;

            var effectHandler = GetComponent<AttackEffectHandler>();
            effectHandler?.PlayAttackEffect();

            Vector3 dir = (target.transform.position - transform.position).normalized;
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, dir);

            GameObject proj = Instantiate(projectilePrefab, transform.position, rotation);
            if (proj.TryGetComponent<Projectile>(out var projectile))
            {
                // VFX/SFX are configured on the prefab itself
                projectile.Initialize(dir, GetMyFaction(), false, bossProjectileDamage, projectileSpeed);
            }

            lastAttackTime = Time.time;
        }

        protected override void HandleDeath()
        {
            Debug.Log($"💀 Boss {name} defeated!");
            base.HandleDeath();
        }
    }
}