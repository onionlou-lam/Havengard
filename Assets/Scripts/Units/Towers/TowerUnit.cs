using UnityEngine;
using Havengard.Abilities;
using Havengard.Combat;
using Havengard.HealthSystem;

namespace Havengard.Units
{
    /// <summary>
    /// Base class for tower/turret units.
    /// Towers are stationary defensive structures that automatically attack enemies in range.
    /// </summary>
    public class TowerUnit : UnitBase
    {
        [Header("Tower Ranged Attack")]
        [SerializeField] protected GameObject projectilePrefab;
        [SerializeField] protected float projectileSpeed = 10f;
        [SerializeField] protected int projectileDamage = 8;
        [SerializeField] protected float attackCooldown = 1.25f;
        [SerializeField] protected bool friendlyFire = false;

        [Header("Tower Homing (Optional)")]
        [SerializeField] protected bool enableHoming = false;
        [SerializeField] protected float homingStrength = 3f;
        [SerializeField] protected float homingDelay = 0.1f;

        [Header("Tower Visuals")]
        [SerializeField] protected Transform turretRotationPivot; // Optional: for rotating the tower sprite toward target
        [SerializeField] protected float rotationSpeed = 5f;

        protected float lastAttackTime;

        protected override void Awake()
        {
            base.Awake();

            // Towers are stationary - disable NavMeshAgent movement
            if (agent != null)
            {
                agent.enabled = false;
            }
        }

        protected override void Update()
        {
            if (!isDead)
            {
                HandleTargeting();
                // Note: No HandleMovementAndAttack() since towers don't move
                HandleTowerAttack();
                UpdateAnimatorAndFacing();

                // Optional: Rotate tower visual toward target
                if (turretRotationPivot != null && currentTarget != null)
                {
                    RotateTurretTowardTarget();
                }
            }
        }

        protected virtual void HandleTowerAttack()
        {
            if (currentTarget == null) return;

            // Check if target is still in range
            float dist = Vector2.Distance(transform.position, currentTarget.transform.position);
            if (dist > attackRange) return;

            PerformAttack(currentTarget);
        }

        protected override void PerformAttack(GameObject target)
        {
            if (target == null) return;
            if (projectilePrefab == null)
            {
                Debug.LogWarning($"[TowerUnit] {name} has no projectilePrefab assigned.");
                return;
            }

            if (Time.time < lastAttackTime + attackCooldown) return;

            var targetHP = target.GetComponent<IHealth>();
            if (targetHP == null) return;
            if (!FactionUtility.CanDamage(GetMyFaction(), targetHP, friendlyFire)) return;

            // Trigger attack animation
            TriggerAttackAnim();

            // Calculate direction
            Vector2 dir2D = (target.transform.position - transform.position).normalized;

            // 2D-friendly rotation for projectile
            float angle = Mathf.Atan2(dir2D.y, dir2D.x) * Mathf.Rad2Deg;
            Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);

            // Spawn projectile at tower position (or turret pivot if available)
            Vector3 spawnPos = turretRotationPivot != null ? turretRotationPivot.position : transform.position;
            GameObject projGO = Instantiate(projectilePrefab, spawnPos, rot);

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
                Debug.LogWarning($"[TowerUnit] Projectile prefab '{projectilePrefab.name}' is missing Projectile component.");
            }

            lastAttackTime = Time.time;
        }

        /// <summary>
        /// Rotates the turret visual smoothly toward the current target.
        /// </summary>
        protected virtual void RotateTurretTowardTarget()
        {
            if (currentTarget == null || turretRotationPivot == null) return;

            Vector2 direction = (currentTarget.transform.position - turretRotationPivot.position).normalized;
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Smooth rotation
            Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
            turretRotationPivot.rotation = Quaternion.Lerp(
                turretRotationPivot.rotation,
                targetRotation,
                Time.deltaTime * rotationSpeed
            );
        }

        protected override void HandleMovementAndAttack()
        {
            // Towers don't move - override to prevent base movement logic
            // Intentionally empty
        }

        protected override Faction GetMyFaction()
        {
            // Towers typically belong to Player or Ally faction
            return Faction.Ally;
        }

#if UNITY_EDITOR
        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            // Draw attack range in blue for towers
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
#endif
    }
}