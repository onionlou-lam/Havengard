using UnityEngine;
using UnityEngine.AI;
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

        [Header("Targeting")]
        [Tooltip("Layers this unit can target (Player, Ally, Gate, etc.). If empty, will search all layers.")]
        [SerializeField] private LayerMask targetLayers = ~0; // default: everything

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
        /// Finds the closest valid enemy using Physics2D overlap + faction filtering.
        /// Subclasses can override this if they need custom targeting, but usually don't need to.
        /// </summary>
        protected virtual GameObject FindTarget()
        {
            GameObject closest = null;
            float closestDist = Mathf.Infinity;
            Faction myFaction = GetMyFaction();

            // 2D physics scan in a circle around this unit
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, aggroRange, targetLayers);

            foreach (var hit in hits)
            {
                if (hit == null) continue;

                var healthTarget = hit.GetComponent<IHealth>();
                if (healthTarget == null) continue;

                // Skip if same faction or otherwise not a valid damage target
                if (!FactionUtility.CanDamage(myFaction, healthTarget, false))
                    continue;

                float dist = Vector2.Distance(transform.position, hit.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = hit.gameObject;
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
                agent.isStopped = true;
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
        /// Returns this unit's faction from its IHealth, or Neutral if none.
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
