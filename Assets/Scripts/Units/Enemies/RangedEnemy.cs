using UnityEngine;
using Havengard.Abilities; // Projectile script
using Havengard.Combat;    // FactionUtility (projectile uses its own check on hit)

namespace Havengard.Units
{
    /// <summary>
    /// Enemy that attacks from range using projectiles.
    /// Respects faction rules (no friendly fire unless enabled).
    /// </summary>
    public class RangedEnemy : EnemyUnit
    {
        [Header("Ranged Settings")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float projectileSpeed = 10f;
        [SerializeField] private int damage = 8;
        [SerializeField] private float attackCooldown = 1.5f;
        [SerializeField] private bool friendlyFire = false;

        private float lastAttackTime;

        protected override void PerformAttack(GameObject target)
        {
            if (Time.time < lastAttackTime + attackCooldown) return;
            if (target == null) return;

            if (Vector2.Distance(transform.position, target.transform.position) <= attackRange)
            {
                ShootProjectile(target);
                lastAttackTime = Time.time;
            }
        }

        private void ShootProjectile(GameObject target)
        {
            if (projectilePrefab == null) return;

            Vector3 dir = (target.transform.position - transform.position).normalized;
            GameObject projGO = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

            var proj = projGO.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.sourceFaction = GetMyFaction();
                proj.friendlyFire = friendlyFire;
                proj.damage = damage;
                proj.speed = projectileSpeed;
            }

            // Rotate projectile to face its travel direction
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            projGO.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
}
