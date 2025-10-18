using UnityEngine;
using UnityEngine.AI;
using Havengard.HealthSystem;
using Havengard.Abilities;
using Havengard.Units;
using Havengard.Statuses;
using Havengard.Combat;

namespace Havengard.Enemies
{
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class EnemyBase : MonoBehaviour, IEnemy
    {
        protected Health health;
        protected AbilityUser abilityUser;
        protected AttackEffectHandler attackEffects;
        protected NavMeshAgent agent;

        [Header("Base Enemy Settings")]
        [SerializeField] protected Faction faction = Faction.Enemy;
        [SerializeField] protected float aggroRange = 6f;
        [SerializeField] protected float attackRange = 1.5f;
        [SerializeField] protected float attackCooldown = 1.5f;

        protected GameObject currentTarget;
        protected float lastAttackTime;

        [Header("Target Priorities")]
        [SerializeField] protected Transform gateTarget;

        protected virtual void Awake()
        {
            health = GetComponent<Health>();
            abilityUser = GetComponent<AbilityUser>();
            attackEffects = GetComponent<AttackEffectHandler>();
            agent = GetComponent<NavMeshAgent>();

            agent.updateRotation = false;
            agent.updateUpAxis = false;
            agent.speed = 2f;

            if (health != null)
                health.OnDeath += HandleDeath;
        }

        protected virtual void Update()
        {
            var effect = GetComponent<StatusEffectInstance>();
            if (effect != null)
            {
                if (effect.IsStunned() || effect.IsRooted())
                {
                    StopMovement();
                    return;
                }
            }

            currentTarget = FindTarget();
            HandleMovementAndAttack(effect);
        }

        protected virtual void HandleMovementAndAttack(StatusEffectInstance effect)
        {
            if (currentTarget == null)
            {
                StopMovement();
                return;
            }

            float distance = Vector2.Distance(transform.position, currentTarget.transform.position);

            if (distance > attackRange)
            {
                MoveTowardsTarget(currentTarget.transform.position);
            }
            else
            {
                StopMovement();

                if (effect == null || !effect.IsSilenced())
                    PerformAttack(currentTarget);
            }
        }

        protected void MoveTowardsTarget(Vector3 targetPos)
        {
            if (agent != null && agent.isActiveAndEnabled)
                agent.SetDestination(targetPos);
        }

        protected void StopMovement()
        {
            if (agent != null && agent.isActiveAndEnabled)
                agent.ResetPath();
        }

        protected bool IsAttackReady() => Time.time >= lastAttackTime + attackCooldown;
        protected void ResetAttackCooldown() => lastAttackTime = Time.time;

        protected GameObject FindTarget()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, aggroRange);
            GameObject closest = null;
            float closestDist = Mathf.Infinity;

            foreach (var hit in hits)
            {
                if (hit.TryGetComponent<Health>(out var h))
                {
                    if (h.GetFaction() == Faction.Player || h.GetFaction() == Faction.Ally)
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

            if (closest == null && gateTarget != null)
                return gateTarget.gameObject;

            return closest;
        }

        public abstract void PerformAttack(GameObject target);

        protected virtual void HandleDeath()
        {
            StopMovement();
            OnDeath();
            Destroy(gameObject);
        }

        public virtual void OnDeath()
        {
            Debug.Log($"{name} has died.");
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, aggroRange);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
#endif
    }
}
