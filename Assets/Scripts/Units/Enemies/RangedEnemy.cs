using UnityEngine;
using Havengard.Abilities;
using Havengard.Combat;
using Havengard.Core.HealthSystem;

namespace Havengard.Units
{
    public class RangedEnemy : UnitBase
    {
        [Header("Ranged")]
        [SerializeField] protected GameObject projectilePrefab;
        [SerializeField] protected float projectileSpeed = 10f;
        [SerializeField] protected int projectileDamage = 8;
        [SerializeField] protected float projectileLifetime = 5f;
        [SerializeField] protected float attackCooldown = 1.25f;
        [SerializeField] protected bool friendlyFire = false;

        [Header("Homing (Optional)")]
        [SerializeField] protected bool enableHoming = false;
        [SerializeField] protected float homingStrength = 3f;

        protected float lastAttackTime;

        protected override void PerformAttack(GameObject target)
        {
            if (target == null) return;
            if (projectilePrefab == null)
            {
                Debug.LogWarning($"[RangedEnemy] {name} has no projectilePrefab assigned.");
                return;
            }

            if (Time.time < lastAttackTime + attackCooldown) return;

            var targetHP = target.GetComponent<IHealth>();
            if (targetHP == null) return;
            if (!FactionUtility.CanDamage(GetMyFaction(), targetHP, friendlyFire)) return;

            // Trigger attack animation
            TriggerAttackAnim();

            Vector2 dir2D = (target.transform.position - transform.position).normalized;

            // 2D-friendly rotation
            float angle = Mathf.Atan2(dir2D.y, dir2D.x) * Mathf.Rad2Deg;
            Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);

            GameObject projGO = Instantiate(projectilePrefab, transform.position, rot);

            if (projGO.TryGetComponent<Projectile>(out var proj))
            {
                // Use generic Initialize with callback
                proj.Initialize(
                    dir2D,
                    projectileSpeed,
                    projectileLifetime,
                    gameObject,
                    (hit) => OnProjectileHit(projGO, hit)
                );
                
                // Enable homing if configured
                if (enableHoming && target != null)
                {
                    proj.SetHomingTarget(target.transform, homingStrength);
                }
            }
            else
            {
                Debug.LogWarning($"[RangedEnemy] Projectile prefab '{projectilePrefab.name}' is missing Projectile component.");
            }

            lastAttackTime = Time.time;
        }

        /// <summary>
        /// Called when this enemy's projectile hits something.
        /// Override to add special effects or behaviors.
        /// </summary>
        protected virtual void OnProjectileHit(GameObject projectile, GameObject hit)
        {
            if (hit == null) return;

            var iHealth = hit.GetComponent<IHealth>();
            if (iHealth != null && FactionUtility.CanDamage(GetMyFaction(), iHealth, friendlyFire))
            {
                var health = hit.GetComponent<Health>();
                if (health != null)
                {
                    health.TakeDamage(projectileDamage, gameObject);
                }
            }

            // Destroy the projectile
            Destroy(projectile);
        }
    }
}
