using UnityEngine;
using UnityEngine.AI;
using Havengard.Core.HealthSystem;
using Havengard.Combat;
using System.Collections;

namespace Havengard.Units
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Health))]
    [DisallowMultipleComponent]
    public abstract class UnitBase : MonoBehaviour
    {
        [Header("General Settings")]
        [SerializeField] protected float aggroRange = 8f;
        [SerializeField] protected float attackRange = 2f;
        [SerializeField] protected float moveSpeed = 3.5f;
        [SerializeField] protected float retargetInterval = 0.5f;

        [Header("Targeting")]
        [Tooltip("Layers this unit can target (Player, Ally, Gate, etc.).")]
        [SerializeField] private LayerMask targetLayers = ~0;
        
        [Tooltip("If true, will target DefaultTarget objects when no enemies in range")]
        [SerializeField] private bool targetDefaultObjectives = true;

        [Header("Animation + Facing")]
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [Tooltip("Minimum time between left/right flips (seconds). Prevents jitter when target hovers around your X line.")]
        [SerializeField] private float flipCooldown = 0.15f;

        private float nextFlipAllowedTime = 0f;
        [Tooltip("Deadzone for horizontal facing updates. Prevents jitter when moving mostly up/down.")]
        [SerializeField] private float facingDeadzone = 0.12f;

        [Tooltip("Minimum time between Hit triggers to avoid spam on rapid damage ticks.")]
        [SerializeField] private float hitAnimCooldown = 0.12f;

        [Header("Damage Flash Effect")]
        [SerializeField] private Color damageFlashColor = new Color(1f, 0.2f, 0.2f, 1f);
        [SerializeField] private float flashDuration = 0.2f;
        [SerializeField] private int flashCount = 2;

        // Animator parameter hashes (must match Animator parameter names exactly)
        private static readonly int AnimSpeed = Animator.StringToHash("Speed");
        private static readonly int AnimAttack = Animator.StringToHash("Attack");
        private static readonly int AnimHit = Animator.StringToHash("Hit");
        private static readonly int AnimDead = Animator.StringToHash("Dead");
        private static readonly int AnimAttackIndex = Animator.StringToHash("AttackIndex");

        public NavMeshAgent agent;
        protected Health health;
        protected GameObject currentTarget;

        private float nextScanTime;
        protected bool isDead;

        // Facing: +1 = right, -1 = left (default art faces RIGHT)
        private int facingSign = 1;

        private float lastHitAnimTime;

        // Flash effect
        private Color originalSpriteColor;
        private Coroutine currentFlashCoroutine;

        [Header("Unit Identity")]
        public string unitName = "Unit";
        public Faction faction = Faction.Neutral;

        [Header("Stats")]
        [SerializeField] protected int maxHealth = 100;

        protected virtual void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            health = GetComponent<Health>();

            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (spriteRenderer != null)
                originalSpriteColor = spriteRenderer.color;

            // NavMeshAgent 2D settings
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            agent.speed = moveSpeed;

            if (health != null)
            {
                health.OnDamaged += OnDamaged;
                health.OnDeath += HandleDeath;
            }
        }

        protected virtual void OnDestroy()
        {
            if (health != null)
            {
                health.OnDamaged -= OnDamaged;
                health.OnDeath -= HandleDeath;
            }

            if (spriteRenderer != null)
                spriteRenderer.color = originalSpriteColor;
        }

        protected virtual void Update()
        {
            if (isDead) return;

            HandleTargeting();
            HandleMovementAndAttack();
            UpdateAnimatorAndFacing();
        }

        // ---------------- Targeting ----------------

        protected virtual void HandleTargeting()
        {
            if (Time.time < nextScanTime) return;
            nextScanTime = Time.time + retargetInterval;

            currentTarget = FindTarget();
        }

        protected virtual GameObject FindTarget()
        {
            GameObject closest = null;
            float closestDist = Mathf.Infinity;
            Faction myFaction = GetMyFaction();

            // First pass: Look for enemies in aggro range
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, aggroRange, targetLayers);

            foreach (var hit in hits)
            {
                if (hit == null) continue;

                var healthTarget = hit.GetComponent<IHealth>();
                if (healthTarget == null) continue;

                if (!FactionUtility.CanDamage(myFaction, healthTarget, false))
                    continue;

                float dist = Vector2.Distance(transform.position, hit.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = hit.gameObject;
                }
            }

            // Second pass: If no enemies found and we target objectives, find default targets
            if (closest == null && targetDefaultObjectives)
            {
                DefaultTarget[] defaultTargets = FindObjectsByType<DefaultTarget>(FindObjectsSortMode.None);
                
                DefaultTarget highestPriority = null;
                float shortestDistToHighPriority = Mathf.Infinity;

                foreach (var defTarget in defaultTargets)
                {
                    if (defTarget == null) continue;

                    var healthTarget = defTarget.GetComponent<IHealth>();
                    if (healthTarget == null) continue;

                    // Check if we can damage this target
                    if (!FactionUtility.CanDamage(myFaction, healthTarget, false))
                        continue;

                    // If this target always has priority, use it
                    if (defTarget.AlwaysPriority)
                    {
                        return defTarget.gameObject;
                    }

                    // Track highest priority target
                    if (highestPriority == null || defTarget.TargetPriority > highestPriority.TargetPriority)
                    {
                        highestPriority = defTarget;
                        shortestDistToHighPriority = Vector2.Distance(transform.position, defTarget.transform.position);
                    }
                    else if (defTarget.TargetPriority == highestPriority.TargetPriority)
                    {
                        // Same priority, choose closest
                        float dist = Vector2.Distance(transform.position, defTarget.transform.position);
                        if (dist < shortestDistToHighPriority)
                        {
                            highestPriority = defTarget;
                            shortestDistToHighPriority = dist;
                        }
                    }
                }

                if (highestPriority != null)
                {
                    closest = highestPriority.gameObject;
                }
            }

            return closest;
        }

        // ---------------- Movement / Attack ----------------

        protected virtual void HandleMovementAndAttack()
        {
            if (agent == null) return;

            if (currentTarget == null)
            {
                StopAgent();
                return;
            }

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

        protected abstract void PerformAttack(GameObject target);

        // ---------------- Animation + Facing ----------------

        protected void TriggerAttackAnim()
        {
            if (animator != null)
            {
                int attackIndex = Random.Range(0, 2);
                animator.SetInteger(AnimAttackIndex, attackIndex);
                animator.SetTrigger(AnimAttack);
            }
        }

        private void TriggerHitAnim()
        {
            if (animator != null && Time.time >= lastHitAnimTime + hitAnimCooldown)
            {
                animator.SetTrigger(AnimHit);
                lastHitAnimTime = Time.time;
            }
        }

        protected virtual void UpdateAnimatorAndFacing() // Changed from private to protected virtual
        {
            if (animator == null) return;

            float speed = agent != null ? agent.velocity.magnitude : 0f;
            animator.SetFloat(AnimSpeed, speed);

            if (currentTarget != null)
            {
                Vector2 dirToTarget = (currentTarget.transform.position - transform.position).normalized;
                UpdateFacing(dirToTarget);
            }
            else if (speed > 0.1f)
            {
                Vector2 moveDir = agent.velocity.normalized;
                UpdateFacing(moveDir);
            }
        }

        private void UpdateFacing(Vector2 direction)
        {
            if (spriteRenderer == null) return;
            if (Mathf.Abs(direction.x) < facingDeadzone) return;
            if (Time.time < nextFlipAllowedTime) return;

            int desiredSign = direction.x > 0 ? 1 : -1;
            if (desiredSign != facingSign)
            {
                facingSign = desiredSign;
                spriteRenderer.flipX = (facingSign < 0);
                nextFlipAllowedTime = Time.time + flipCooldown;
            }
        }

        // ---------------- Health / Damage ----------------

        protected virtual void OnDamaged(int damage)
        {
            TriggerHitAnim();
            StartDamageFlash();
        }

        protected virtual void HandleDeath()
        {
            if (isDead) return;
            isDead = true;

            StopAgent();

            if (animator != null)
                animator.SetTrigger(AnimDead);

            StartCoroutine(DestroyAfterDelay(2f));
        }

        private IEnumerator DestroyAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            Destroy(gameObject);
        }

        private void StartDamageFlash()
        {
            if (spriteRenderer == null) return;

            if (currentFlashCoroutine != null)
                StopCoroutine(currentFlashCoroutine);

            currentFlashCoroutine = StartCoroutine(DamageFlashRoutine());
        }

        private IEnumerator DamageFlashRoutine()
        {
            float flashInterval = flashDuration / (flashCount * 2);

            for (int i = 0; i < flashCount; i++)
            {
                spriteRenderer.color = damageFlashColor;
                yield return new WaitForSeconds(flashInterval);
                spriteRenderer.color = originalSpriteColor;
                yield return new WaitForSeconds(flashInterval);
            }

            currentFlashCoroutine = null;
        }

        // ---------------- Utility ----------------

        protected void StopAgent()
        {
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }
        }

        public virtual Faction GetMyFaction() // Changed from protected to public virtual
        {
            return health != null ? health.GetFaction() : Faction.Neutral;
        }

        protected virtual void OnDrawGizmosSelected() // Changed from private to protected virtual
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, aggroRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
