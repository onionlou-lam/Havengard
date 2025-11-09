using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;
using Havengard.Abilities;
using Havengard.HealthSystem;
using Havengard.Combat;

namespace Havengard.Units
{
    /// <summary>
    /// Base class for all NavMesh-driven units (Player, Allies, Enemies, Bosses).
    /// Handles movement, targeting, attacking, and faction logic.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Health))]
    [DisallowMultipleComponent]
    public abstract class UnitBase : MonoBehaviour
    {
        [Header("General Settings")]
        [SerializeField] protected float aggroRange = 8f;
        [SerializeField] protected float attackRange = 2f;
        [SerializeField] protected float moveSpeed = 3.5f;
        [SerializeField] protected float retargetInterval = 0.5f; // seconds between scans

        protected NavMeshAgent agent;
        protected Health health;
        protected AbilityUser abilityUser;
        protected GameObject currentTarget;

        private float nextScanTime = 0f;
        private bool isDead;

        // --------------------------------------------------------------------
        #region Unity Lifecycle
        // --------------------------------------------------------------------

        protected virtual void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            agent.speed = moveSpeed;

            health = GetComponent<Health>();
            abilityUser = GetComponent<AbilityUser>();

            if (health != null)
                health.OnDeath += HandleDeath;
        }

        protected virtual void OnDestroy()
        {
            if (health != null)
                health.OnDeath -= HandleDeath;
        }

        protected virtual void Update()
        {
            if (isDead) return;

            HandleTargeting();
            HandleMovementAndAttack();
        }
        #endregion

        // --------------------------------------------------------------------
        #region Targeting & AI Logic
        // --------------------------------------------------------------------

        /// <summary>
        /// Caches target scanning to run only at intervals.
        /// </summary>
        protected virtual void HandleTargeting()
        {
            if (Time.time < nextScanTime) return;
            nextScanTime = Time.time + retargetInterval;

            currentTarget = FindTarget();
        }

        /// <summary>
        /// Finds the closest valid enemy using the shared UnitTargetManager.
        /// Subclasses can override this if they need custom targeting logic.
        /// </summary>
        protected virtual GameObject FindTarget()
        {
            GameObject closest = null;
            float closestDist = Mathf.Infinity;
            Faction myFaction = GetMyFaction();

            foreach (var h in UnitTargetManager.RegisteredUnits)
            {
                if (h == null) continue;
                if (!FactionUtility.CanDamage(myFaction, h, false))
                    continue;

                var obj = (h as MonoBehaviour).gameObject;
                float dist = Vector2.Distance(transform.position, obj.transform.position);
                if (dist < closestDist && dist <= aggroRange)
                {
                    closest = obj;
                    closestDist = dist;
                }
            }

            return closest;
        }

        /// <summary>
        /// Handles movement and attack range checking via NavMesh.
        /// </summary>
        protected virtual void HandleMovementAndAttack()
        {
            if (currentTarget == null)
            {
                agent.ResetPath();
                return;
            }

            float dist = Vector2.Distance(transform.position, currentTarget.transform.position);

            if (dist > attackRange)
            {
                // Move toward target
                agent.isStopped = false;
                agent.SetDestination(currentTarget.transform.position);
            }
            else
            {
                // Stop and attack
                agent.isStopped = true;
                PerformAttack(currentTarget);
            }
        }

        #endregion

        // --------------------------------------------------------------------
        #region Combat & Faction
        // --------------------------------------------------------------------

        /// <summary>
        /// Core attack behavior. Must be implemented by subclasses.
        /// </summary>
        protected abstract void PerformAttack(GameObject target);

        /// <summary>
        /// Returns this unit's faction.
        /// </summary>
        protected virtual Faction GetMyFaction()
        {
            var h = GetComponent<IHealth>();
            return h != null ? h.GetFaction() : Faction.Neutral;
        }

        /// <summary>
        /// Called when health reaches 0.
        /// </summary>
        protected virtual void HandleDeath()
        {
            isDead = true;
            agent.isStopped = true;
            agent.ResetPath();
        }

        #endregion

#if UNITY_EDITOR
        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, aggroRange);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
#endif
    }
}
