using Havengard.Combat;
using Havengard.HealthSystem;
using System.Linq;
using UnityEngine;

namespace Havengard.Units
{
    /// <summary>
    /// NavMesh-based enemy that uses pathfinding for movement and
    /// deals direct melee damage. Works with NavMeshSurface2D and
    /// NavMeshAgent2D (from NavMesh Plus).
    /// </summary>
    public class MeleeEnemyNavMesh : UnitBaseNavMesh
    {
        [Header("Melee Settings")]
        [SerializeField] private float attackCooldown = 1f;
        [SerializeField] private bool friendlyFire = false;

        private float lastAttackTime;

        protected override void PerformAttack(GameObject target)
        {
            if (Time.time < lastAttackTime + attackCooldown) return;
            if (target == null) return;

            if (Vector2.Distance(transform.position, target.transform.position) <= attackRange)
            {
                var targetHealth = target.GetComponent<IHealth>();
                if (FactionUtility.CanDamage(GetMyFaction(), targetHealth, friendlyFire))
                {
                    // Calculate and apply damage using CombatCalculator
                    int finalDamage = CombatCalculator.CalculateDamage(gameObject, target);
                    targetHealth.GetHealthSystem().Damage(finalDamage);

                    lastAttackTime = Time.time;
                }
            }
        }

        /// <summary>
        /// Automatically searches for the closest valid target within aggro range.
        /// </summary>
        protected override GameObject FindTarget()
        {
            // Find all potential targets
            var allHealths = FindObjectsOfType<MonoBehaviour>().OfType<IHealth>();

            GameObject closestTarget = null;
            float closestDist = Mathf.Infinity;
            Faction myFaction = GetMyFaction();

            foreach (var h in allHealths)
            {
                GameObject obj = (h as MonoBehaviour).gameObject;

                if (!FactionUtility.CanDamage(myFaction, h, friendlyFire))
                    continue;

                float dist = Vector2.Distance(transform.position, obj.transform.position);
                if (dist < closestDist && dist <= aggroRange)
                {
                    closestDist = dist;
                    closestTarget = obj;
                }
            }

            return closestTarget;
        }
    }
}
