using UnityEngine;
using Havengard.HealthSystem;
using Havengard.Abilities;
using Havengard.Units;

namespace Havengard.Allies
{
    /// <summary>
    /// Ally "sub-hero". Stays near spawn but will chase/attack enemies within aggro range.
    /// Uses AbilityUser to cast abilities like the player.
    /// </summary>
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(AbilityUser))]
    public class AllyHero : MonoBehaviour
    {
        [SerializeField] private float aggroRange = 6f;
        [SerializeField] private float chaseSpeed = 3f;
        [SerializeField] private float attackRange = 1.5f;

        private Vector2 spawnPoint;
        private Rigidbody2D rb;
        private AbilityUser abilityUser;
        private Health health;

        private void Awake()
        {
            spawnPoint = transform.position;
            rb = GetComponent<Rigidbody2D>();
            abilityUser = GetComponent<AbilityUser>();
            health = GetComponent<Health>();
        }

        private void Update()
        {
            GameObject target = FindClosestEnemy();
            if (target != null)
            {
                float dist = Vector2.Distance(transform.position, target.transform.position);
                if (dist > attackRange)
                {
                    Vector2 dir = (target.transform.position - transform.position).normalized;
                    rb.linearVelocity = dir * chaseSpeed;
                }
                else
                {
                    rb.linearVelocity = Vector2.zero;
                    abilityUser.UseAbility(0, target); // basic attack
                }
            }
            else
            {
                // No enemies: return to spawn and idle
                if (Vector2.Distance(transform.position, spawnPoint) > 0.1f)
                {
                    Vector2 dir = (spawnPoint - (Vector2)transform.position).normalized;
                    rb.linearVelocity = dir * chaseSpeed;
                }
                else
                {
                    rb.linearVelocity = Vector2.zero;
                }
            }
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
