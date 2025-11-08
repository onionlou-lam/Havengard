/*using UnityEngine;
using Havengard.Abilities;
using Havengard.HealthSystem;
using Havengard.Units; // for Faction

namespace Havengard.Units
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(AbilityUser))]
    public abstract class UnitBase : MonoBehaviour
    {
        protected Rigidbody2D rb;
        protected Health health;
        protected AbilityUser abilityUser;

        [Header("Unit Movement/Combat")]
        [SerializeField] protected float moveSpeed = 3f;
        [SerializeField] protected float attackRange = 1.5f;
        [SerializeField] protected float aggroRange = 6f;

        protected GameObject currentTarget;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            health = GetComponent<Health>();
            abilityUser = GetComponent<AbilityUser>();

            if (health != null)
            {
                // Optional death hook if subclasses need it
                health.OnDeath += HandleDeath;
            }
        }

        protected virtual void Update()
        {
            currentTarget = FindTarget();
            HandleMovementAndAttack();
        }

        protected abstract GameObject FindTarget();

        protected virtual void HandleMovementAndAttack()
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

        protected virtual void PerformAttack(GameObject target)
        {
            // Default behavior if using AbilityUser slot 0.
            abilityUser?.UseAbility(0, target);
        }

        protected virtual void HandleDeath()
        {
            // Default: destroy on death; subclasses can override.
            Destroy(gameObject);
        }

        /// <summary>
        /// Helper for enemies/allies to read their own faction from IHealth/Health.
        /// </summary>
        protected Faction GetMyFaction()
        {
            var ih = GetComponent<IHealth>();
            return ih != null ? ih.GetFaction() : Faction.Neutral;
        }
    }
}
*/