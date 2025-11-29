/*using Havengard.Abilities;
using Havengard.Combat;
using Havengard.HealthSystem;
using Havengard.Statuses;
using Havengard.Units;
using UnityEngine;
using UnityEngine.AI;

namespace Havengard.Allies
{
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(AbilityUser))]
    public class AllyHero : MonoBehaviour
    {
        [SerializeField] private float aggroRange = 6f;
        [SerializeField] private float attackRange = 1.5f;

        private Vector2 spawnPoint;
        private NavMeshAgent agent;
        private AbilityUser abilityUser;
        private Health health;
        private AttackEffectHandler attackEffects;

        private void Awake()
        {
            spawnPoint = transform.position;
            agent = GetComponent<NavMeshAgent>();
            abilityUser = GetComponent<AbilityUser>();
            health = GetComponent<Health>();
            attackEffects = GetComponent<AttackEffectHandler>();

            agent.updateRotation = false;
            agent.updateUpAxis = false;
        }

        private void Update()
        {
            var effect = GetComponent<StatusEffectInstance>();
            if (effect != null)
            {
                if (effect.IsStunned() || effect.IsRooted())
                {
                    agent.ResetPath();
                    return;
                }
                if (effect.IsSilenced())
                {
                    MoveToEnemiesOnly();
                    return;
                }
            }

            GameObject target = FindClosestEnemy();
            if (target != null)
            {
                float dist = Vector2.Distance(transform.position, target.transform.position);
                if (dist > attackRange)
                {
                    agent.SetDestination(target.transform.position);
                }
                else
                {
                    agent.ResetPath();

                    attackEffects?.PlayAttackEffect();
                    abilityUser.UseAbility(0, target);
                    attackEffects?.PlayImpactEffect(target.transform.position);
                }
            }
            else
            {
                ReturnToSpawn();
            }
        }

        private void MoveToEnemiesOnly()
        {
            GameObject target = FindClosestEnemy();
            if (target != null)
                agent.SetDestination(target.transform.position);
            else
                agent.ResetPath();
        }

        private void ReturnToSpawn()
        {
            if (Vector2.Distance(transform.position, spawnPoint) > 0.5f)
                agent.SetDestination(spawnPoint);
            else
                agent.ResetPath();
        }

        private GameObject FindClosestEnemy()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, aggroRange);
            GameObject closest = null;
            float closestDist = Mathf.Infinity;

            foreach (var hit in hits)
            {
                if (hit.TryGetComponent<Health>(out var h))
                {
                    if (h.GetFaction() == Faction.Enemy)
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
            return closest;
        }
    }
}
*/