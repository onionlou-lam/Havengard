using UnityEngine;
using Havengard.Abilities;
using Havengard.HealthSystem;
using Havengard.Combat;
using System.Linq;

namespace Havengard.Units
{
    /// <summary>
    /// NavMesh-based ranged enemy that uses projectiles for attacks.
    /// </summary>
    public class RangedEnemyNavMesh : UnitBaseNavMesh
    {
        [Header("Ranged Settings")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float projectileSpeed = 10f;
        [SerializeField] private int projectileDamage = 8;
        [SerializeField] private float attackCooldown = 1.25f;
        [SerializeField] private bool friendlyFire = false;

        private float lastAttackTime;

        protected override void PerformAttack(GameObject target)
        {
            if (Time.time < lastAttackTime + attackCooldown || target == null) return;

            var targetHealth = target.GetComponent<IHealth>();
            if (!FactionUtility.CanDamage(GetMyFaction(), targetHealth, friendlyFire)) return;

            // Play attack animation/effect if available
            var effectHandler = GetComponent<AttackEffectHandler>();
            effectHandler?.PlayAttackEffect();

            Vector3 dir = (target.transform.position - transform.position).normalized;
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, dir);

            // Spawn projectile
            GameObject proj = Instantiate(projectilePrefab, transform.position, rotation);
            if (proj.TryGetComponent<Projectile>(out var projectile))
            {
                projectile.Init(dir, GetMyFaction(), friendlyFire, projectileDamage, projectileSpeed);
            }

            lastAttackTime = Time.time;
        }

        protected override GameObject FindTarget()
        {
            var allHealths = FindObjectsOfType<MonoBehaviour>().OfType<IHealth>();
            GameObject closest = null;
            float closestDist = Mathf.Infinity;
            Faction myFaction = GetMyFaction();

            foreach (var h in allHealths)
            {
                var obj = (h as MonoBehaviour).gameObject;
                if (!FactionUtility.CanDamage(myFaction, h, friendlyFire))
                    continue;

                float dist = Vector2.Distance(transform.position, obj.transform.position);
                if (dist < closestDist && dist <= aggroRange)
                {
                    closestDist = dist;
                    closest = obj;
                }
            }

            return closest;
        }
    }
}
