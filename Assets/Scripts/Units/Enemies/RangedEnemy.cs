using UnityEngine;
using Havengard.Abilities;
using Havengard.Combat;
using Havengard.HealthSystem;

namespace Havengard.Units
{
    public class RangedEnemy : UnitBase
    {
        [Header("Ranged")]
        [SerializeField] protected GameObject projectilePrefab;
        [SerializeField] protected float projectileSpeed = 10f;
        [SerializeField] protected int projectileDamage = 8;
        [SerializeField] protected float attackCooldown = 1.25f;
        [SerializeField] protected bool friendlyFire = false;

        [Header("Homing (Optional)")]
        [SerializeField] protected bool enableHoming = false;
        [SerializeField] protected float homingStrength = 3f;
        [SerializeField] protected float homingDelay = 0.1f;

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

            Vector2 dir2D = (target.transform.position - transform.position).normalized;

            // 2D-friendly rotation
            float angle = Mathf.Atan2(dir2D.y, dir2D.x) * Mathf.Rad2Deg;
            Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);

            GameObject projGO = Instantiate(projectilePrefab, transform.position, rot);

            if (projGO.TryGetComponent<Projectile>(out var proj))
            {
                proj.Initialize(dir2D, GetMyFaction(), friendlyFire, projectileDamage, projectileSpeed);
                
                // Enable homing if configured
                if (enableHoming)
                {
                    proj.EnableHoming(homingStrength, homingDelay, target);
                }
            }
            else
            {
                Debug.LogWarning($"[RangedEnemy] Projectile prefab '{projectilePrefab.name}' is missing Projectile component.");
            }

            lastAttackTime = Time.time;
        }
    }
}
