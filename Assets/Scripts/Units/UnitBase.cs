using UnityEngine;
using UnityEngine.AI;
using Havengard.HealthSystem;
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

        public NavMeshAgent agent; //public for behaviors to access directly as it is a component reference, similar to RigidBody etc.
        protected Health health;
        protected GameObject currentTarget;

        private float nextScanTime;
        private bool isDead;

        // Facing: +1 = right, -1 = left (default art faces RIGHT)
        private int facingSign = 1;

        private float lastHitAnimTime;

        // Flash effect
        private Color originalSpriteColor;
        private Coroutine currentFlashCoroutine;

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
                // archive note for aniamtion changes: matches your Health.cs: event Action<int> OnDamaged
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

            // Restore original color on cleanup
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
                animator.SetTrigger(AnimAttack);
        }

        private void TriggerHitAnim()
        {
            if (animator != null)
                animator.SetTrigger(AnimHit);
        }

        private void UpdateAnimatorAndFacing()
        {
            if (animator == null) return;

            // Use agent.velocity for animation (more reliable than desiredVelocity for "actually moving")
            Vector2 v = Vector2.zero;
            if (agent != null && agent.enabled && agent.isOnNavMesh)
                v = agent.velocity;

            animator.SetFloat(AnimSpeed, v.magnitude);

            if (spriteRenderer == null) return;

            // Respect flip cooldown
            if (Time.time < nextFlipAllowedTime)
                return;

            // 1) Prefer movement-based facing when moving meaningfully
            if (Mathf.Abs(v.x) >= facingDeadzone)
            {
                int desired = (v.x >= 0f) ? 1 : -1;
                ApplyFacing(desired);
                return;
            }

            // 2) If not moving horizontally, face the target (useful for melee attacks while stopped)
            if (currentTarget != null)
            {
                float dx = currentTarget.transform.position.x - transform.position.x;

                if (Mathf.Abs(dx) >= facingDeadzone)
                {
                    int desired = (dx >= 0f) ? 1 : -1;
                    ApplyFacing(desired);
                }
            }
        }

        private void ApplyFacing(int desiredFacingSign)
        {
            if (desiredFacingSign == facingSign) return;

            facingSign = desiredFacingSign;
            spriteRenderer.flipX = (facingSign == -1);

            // start cooldown only when we actually flip
            nextFlipAllowedTime = Time.time + flipCooldown;
        }

        protected void FaceTargetNow()
        {
            if (spriteRenderer == null || currentTarget == null) return;

            float dx = currentTarget.transform.position.x - transform.position.x;
            if (Mathf.Abs(dx) < facingDeadzone) return;

            int desired = (dx >= 0f) ? 1 : -1;

            // Force facing without waiting for cooldown (feels better for attacks)
            if (desired != facingSign)
            {
                facingSign = desired;
                spriteRenderer.flipX = (facingSign == -1);
                nextFlipAllowedTime = Time.time + flipCooldown;
            }
        }

        // ---------------- Damage / Death ----------------

        private void OnDamaged(int amount)
        {
            if (isDead) return;

            // Avoid spamming hit triggers on rapid ticks
            if (Time.time < lastHitAnimTime + hitAnimCooldown) return;
            lastHitAnimTime = Time.time;

            TriggerHitAnim();
            PlayDamageFlash();
        }

        private void PlayDamageFlash()
        {
            if (spriteRenderer == null) return;

            if (currentFlashCoroutine != null)
                StopCoroutine(currentFlashCoroutine);

            currentFlashCoroutine = StartCoroutine(FlashCoroutine());
        }

        private IEnumerator FlashCoroutine()
        {
            float flashInterval = flashDuration / (flashCount * 2);

            for (int i = 0; i < flashCount; i++)
            {
                // Flash to damage color
                spriteRenderer.color = damageFlashColor;
                yield return new WaitForSeconds(flashInterval);

                // Return to original
                spriteRenderer.color = originalSpriteColor;
                yield return new WaitForSeconds(flashInterval);
            }

            // Ensure we end on original color
            spriteRenderer.color = originalSpriteColor;
            currentFlashCoroutine = null;
        }

        protected virtual void HandleDeath()
        {
            if (animator != null)
            {
                animator.SetFloat("Speed", 0f);
                animator.SetBool("Dead", true);
            }
            if (isDead) return;
            isDead = true;

            if (animator != null)
                animator.SetBool(AnimDead, true);

            StopAgent();

            // Stop any ongoing flash effect on death
            if (currentFlashCoroutine != null)
            {
                StopCoroutine(currentFlashCoroutine);
                currentFlashCoroutine = null;
            }

            // Restore original color
            if (spriteRenderer != null)
                spriteRenderer.color = originalSpriteColor;
        }

        private void StopAgent()
        {
            if (agent == null) return;

            agent.isStopped = true;

            // Safety: ResetPath only if agent is active/on navmesh
            if (agent.enabled && agent.isOnNavMesh && agent.hasPath)
                agent.ResetPath();
        }

        // ---------------- Faction ----------------

        protected virtual Faction GetMyFaction()
        {
            var h = GetComponent<IHealth>();
            return h != null ? h.GetFaction() : Faction.Neutral;
        }

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
