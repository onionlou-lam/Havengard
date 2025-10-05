using UnityEngine;
using UnityEngine.AI;
using Havengard.Abilities;
using Havengard.HealthSystem;

namespace Havengard.Units
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(AbilityUser))]
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class UnitBaseNavMesh : MonoBehaviour
    {
        protected Rigidbody2D rb;
        protected NavMeshAgent agent;
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
            agent = GetComponent<NavMeshAgent>();

            // Keep Rigidbody2D only for collisions
            rb.isKinematic = true;
            rb.simulated = false;

            // Setup NavMeshAgent for 2D
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            agent.speed = moveSpeed;
            agent.stoppingDistance = attackRange * 0.9f;

            if (health != null)
                health.OnDeath += HandleDeath;
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
                agent.ResetPath();
                return;
            }

            float dist = Vector2.Distance(transform.position, currentTarget.transform.position);

            if (dist > attackRange)
            {
                agent.SetDestination(currentTarget.transform.position);
            }
            else
            {
                agent.ResetPath();
                PerformAttack(currentTarget);
            }
        }

        protected virtual void PerformAttack(GameObject target)
        {
            abilityUser?.UseAbility(0, target);
        }

        protected virtual void HandleDeath()
        {
            Destroy(gameObject);
        }

        protected Faction GetMyFaction()
        {
            var ih = GetComponent<IHealth>();
            return ih != null ? ih.GetFaction() : Faction.Neutral;
        }
    }
}
