using Havengard.Abilities;
using Havengard.Combat;
using Havengard.HealthSystem;
using Havengard.Statuses;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Havengard.Units
{
    /// <summary>
    /// Hero ally that can use abilities and moves via NavMeshAgent.
    /// </summary>
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(AbilityUser))]
    public class AllyHeroNavMesh : UnitBaseNavMesh
    {
        [Header("Hero Settings")]
        [SerializeField] private float attackCooldown = 1f;

        private float lastAttackTime;
        private Vector2 spawnPoint;

        protected override void Awake()
        {
            base.Awake();
            spawnPoint = transform.position;
        }

        protected override void PerformAttack(GameObject target)
        {
            if (Time.time < lastAttackTime + attackCooldown || target == null) return;

            var status = GetComponent<StatusEffectInstance>();
            if (status != null && (status.IsStunned() || status.IsSilenced()))
                return;

            abilityUser?.UseAbility(0, target);
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
                if (!FactionUtility.CanDamage(myFaction, h, false))
                    continue;

                float dist = Vector2.Distance(transform.position, obj.transform.position);
                if (dist < closestDist && dist <= aggroRange)
                {
                    closestDist = dist;
                    closest = obj;
                }
            }

            // Return to spawn if idle
            if (closest == null && Vector2.Distance(transform.position, spawnPoint) > 0.5f)
                agent.SetDestination(spawnPoint);

            return closest;
        }
    }
}
