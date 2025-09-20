using UnityEngine;
using Havengard.HealthSystem;
using Havengard.Abilities;
using Havengard.Units;

namespace Havengard.Enemies
{
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class EnemyBase : MonoBehaviour, IEnemy
    {
        protected Health health;
        protected AbilityUser abilityUser;
        protected Rigidbody2D rb;

        [Header("Targeting")]
        [SerializeField] protected float aggroRange = 6f;
        [SerializeField] protected float attackRange = 1.5f;
        [SerializeField] protected float moveSpeed = 2f;
        [SerializeField] protected Transform gateTarget;

        private GameObject currentTarget;

        protected virtual void Awake()
        {
            health = GetComponent<Health>();
            abilityUser = GetComponent<AbilityUser>();
            rb = GetComponent<Rigidbody2D>();

            if (health != null)
                health.OnDeath += HandleDeath;
        }

        protected virtual void Update()
        {
            currentTarget = FindTarget();
            HandleMovementAndAttack();
        }

        protected GameObject FindTarget()
        {
            // Check for nearby Player/Allies
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, aggroRange);
            GameObject closest = null;
            float closestDist = Mathf.Infinity;

            foreach (var hit in hits)
            {
                if (hit.TryGetComponent<Health>(out var h))
                {
                    if (h.GetFaction() == Faction.Player)
                    {
                        float d = Vector2.Distance(transform.position, hit.transform.position);
                        if (d < closestDist)
                        {
                            closestDist = d;
                            closest = hit.gameObject;
                        }
                    }
                }
            }

            // Fall back to Gate
            if (closest == null && gateTarget != null) return gateTarget.gameObject;
            return closest;
        }

        private void HandleMovementAndAttack()
        {
            if (currentTarget == null)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            float dist = Vector2.Distance(transform.position, currentTarget.transform.position);

            if (dist > attackRange)
            {
                Vector2 dir = (currentTarget.transform.position - transform.position).normalized;
                rb.linearVelocity = dir * moveSpeed;
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
                PerformAttack(currentTarget);
            }
        }

        public abstract void PerformAttack(GameObject target);

        protected virtual void HandleDeath()
        {
            OnDeath();
            Destroy(gameObject);
        }

        public virtual void OnDeath()
        {
            Debug.Log($"{name} has died.");
        }
    }
}
