using UnityEngine;
using Havengard.Abilities;
using Havengard.HealthSystem;
using Havengard.Combat;

namespace Havengard.Units
{
    /// <summary>
    /// Basic ally unit with configurable AI behavior.
    /// No progression tracking (EXP/Items). Can be spawned before waves start.
    /// </summary>
    public class AllyUnit : UnitBase, IAlly
    {
        [Header("Ally Unit Settings")]
        [SerializeField] private AllyBehaviorMode behaviorMode = AllyBehaviorMode.Default;
        [SerializeField] private Transform playerTransform; // Set for Follow behavior
        [SerializeField] private AbilityBase attackAbility; // Optional ability-based attack

        [Header("Basic Attack (if no ability)")]
        [SerializeField] private int baseDamage = 10;
        [SerializeField] private float attackCooldown = 1f;

        private AllyBehavior currentBehavior;
        private float lastAttackTime;

        protected override void Awake()
        {
            base.Awake();
            InitializeBehavior();
        }

        private void InitializeBehavior()
        {
            // Remove any existing behavior
            if (currentBehavior != null)
            {
                Destroy(currentBehavior);
            }

            // Create appropriate behavior based on mode
            switch (behaviorMode)
            {
                case AllyBehaviorMode.Default:
                    currentBehavior = gameObject.AddComponent<DefaultBehavior>();
                    break;

                case AllyBehaviorMode.Stationary:
                    currentBehavior = gameObject.AddComponent<StationaryBehavior>();
                    break;

                case AllyBehaviorMode.Follow:
                    currentBehavior = gameObject.AddComponent<FollowBehavior>();
                    if (playerTransform == null)
                    {
                        // Try to find player automatically
                        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
                    }
                    currentBehavior.SetFollowTarget(playerTransform);
                    break;
            }

            currentBehavior.Initialize(this);
        }

        /// <summary>
        /// Change behavior at runtime (IAlly implementation).
        /// </summary>
        public void SetBehavior(AllyBehaviorMode newMode, Transform followTarget = null)
        {
            behaviorMode = newMode;
            if (followTarget != null)
            {
                playerTransform = followTarget;
            }
            InitializeBehavior();
        }

        /// <summary>
        /// Get current behavior mode (IAlly implementation).
        /// </summary>
        public AllyBehaviorMode GetBehaviorMode()
        {
            return behaviorMode;
        }

        protected override GameObject FindTarget()
        {
            // Check if behavior has custom targeting
            GameObject behaviorTarget = currentBehavior?.FindBehaviorTarget();
            if (behaviorTarget != null)
            {
                return behaviorTarget;
            }

            // Use default UnitBase targeting
            return base.FindTarget();
        }

        protected override void HandleMovementAndAttack()
        {
            if (agent == null) return;

            if (currentTarget == null)
            {
                // Let behavior handle what to do when idle
                currentBehavior?.OnNoTarget();
                return;
            }

            // Standard combat movement (same as UnitBase)
            float dist = Vector2.Distance(transform.position, currentTarget.transform.position);

            if (dist > attackRange)
            {
                agent.isStopped = false;
                agent.SetDestination(currentTarget.transform.position);
            }
            else
            {
                agent.isStopped = true;
                PerformAttack(currentTarget);
            }
        }

        protected override void PerformAttack(GameObject target)
        {
            if (target == null) return;

            // Cooldown check
            if (Time.time < lastAttackTime + attackCooldown) return;

            // Use ability if available
            if (attackAbility != null)
            {
                if (attackAbility.CanCast(gameObject, target))
                {
                    TriggerAttackAnim();
                    attackAbility.Cast(gameObject, target);
                    lastAttackTime = Time.time;
                }
                return;
            }

            // Otherwise use basic attack
            var h = target.GetComponent<IHealth>();
            if (h != null && FactionUtility.CanDamage(GetMyFaction(), h, false))
            {
                TriggerAttackAnim();
                h.GetHealthSystem().Damage(baseDamage);
                lastAttackTime = Time.time;
            }
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private float debugSpeed;
        [SerializeField] private bool debugIsMoving;

        protected override void Update()
        {
            base.Update();
            
            // Update debug values
            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                debugSpeed = agent.velocity.magnitude;
                debugIsMoving = !agent.isStopped;
            }
        }
#endif
    }
}
