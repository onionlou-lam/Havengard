using Havengard.Abilities;
using Havengard.Combat;
using Havengard.Enemies;
using Havengard.HealthSystem;
using Havengard.Statuses;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

namespace Havengard.Units
{
    public class RangedEnemy : EnemyBase
    {
        [Header("Ranged Attack")]
        [SerializeField] protected GameObject projectilePrefab;   // ← was private
        [SerializeField] protected float projectileSpeed = 10f;    // ← was private
        [SerializeField] protected int projectileDamage = 8;
        [SerializeField] protected bool friendlyFire = false;

        public override void PerformAttack(GameObject target)
        {
            if (!IsAttackReady() || target == null) return;
            if (GetComponent<StatusEffectInstance>()?.IsSilenced() == true) return;

            attackEffects?.PlayAttackEffect();

            Vector3 dir = (target.transform.position - transform.position).normalized;
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, dir);

            GameObject proj = Instantiate(projectilePrefab, transform.position, rotation);
            var projectile = proj.GetComponent<Projectile>();

            if (projectile != null)
            {
                projectile.Init(dir, faction, friendlyFire, projectileDamage, projectileSpeed);
                projectile.ConfigureImpactEffects(null, null, null);
            }

            Debug.Log($"{name} fired projectile at {target.name}");
            ResetAttackCooldown();
        }
    }
}
