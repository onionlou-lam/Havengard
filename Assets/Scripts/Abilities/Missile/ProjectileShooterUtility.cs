using UnityEngine;
using UnityEngine.AI;
using Havengard.Core.HealthSystem;
using Havengard.Abilities;
using Havengard.Units;
using Havengard.Combat;

namespace Havengard.Combat
{
    /// <summary>
    /// Modular helper class for firing projectiles safely in 2D.
    /// Can be attached to any ranged unit (Player, Ally, or Enemy).
    /// </summary>
    [RequireComponent(typeof(Transform))]
    public class ProjectileShooterUtility : MonoBehaviour
    {
        [Header("General Settings")]
        [SerializeField] private GameObject projectilePrefab; // Prefab should have Projectile component with VFX/SFX configured
        [SerializeField] private float projectileSpeed = 10f;
        [SerializeField] private int projectileDamage = 15;
        [SerializeField] private float projectileLifetime = 5f;
        [SerializeField] private float spawnOffset = 0.5f;
        [SerializeField] private bool friendlyFire = false;
        [SerializeField] private LayerMask wallMask;
        [SerializeField] private LayerMask friendlyMask;

        [Header("Audio (Fire Sound)")]
        [SerializeField] private AudioClip fireSFX;

        [Header("NavMesh Repositioning")]
        [SerializeField] private bool allowReposition = true;
        [SerializeField] private float repositionRadius = 2.5f;
        [SerializeField] private float repositionCooldown = 1f;

        private float lastRepositionTime;
        private Faction faction;
        private NavMeshAgent agent;
        private AttackEffectHandler attackEffects;

        private void Awake()
        {
            faction = GetComponent<IHealth>()?.GetFaction() ?? Faction.Neutral;
            agent = GetComponent<NavMeshAgent>();
            attackEffects = GetComponent<AttackEffectHandler>();

            if (wallMask == 0)
                wallMask = LayerMask.GetMask("Walls");

            if (friendlyMask == 0)
                friendlyMask = LayerMask.GetMask("Player", "Ally");
        }

        /// <summary>
        /// Attempts to fire a projectile toward the given target GameObject.
        /// Performs line-of-sight and friendly-fire checks automatically.
        /// </summary>
        public bool TryShootAt(GameObject target)
        {
            if (projectilePrefab == null || target == null)
                return false;

            Vector3 dir3D = (target.transform.position - transform.position).normalized;
            Vector2 dir = new Vector2(dir3D.x, dir3D.y);

            // 1. Check for walls
            if (Physics2D.Raycast(transform.position, dir, 3f, wallMask))
            {
                Debug.DrawRay(transform.position, dir * 3f, Color.red, 0.5f);
                if (allowReposition) TryReposition(target);
                return false;
            }

            // 2. Check for allies in front
            if (!friendlyFire && Physics2D.Raycast(transform.position, dir, 3f, friendlyMask))
            {
                Debug.DrawRay(transform.position, dir * 3f, Color.yellow, 0.5f);
                return false;
            }

            // 3. Calculate safe spawn point
            Vector3 spawnPos = transform.position + (Vector3)(dir * spawnOffset);
            if (Physics2D.Raycast(transform.position, dir, spawnOffset, wallMask))
            {
                Debug.Log($"{name}: blocked close shot by wall.");
                return false;
            }

            // 4. Play visual/sound feedback
            attackEffects?.PlayAttackEffect();
            if (fireSFX != null)
                AudioSource.PlayClipAtPoint(fireSFX, transform.position, 0.8f);

            // 5. Spawn and initialize projectile
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, dir);
            GameObject projGO = Instantiate(projectilePrefab, spawnPos, rotation);
            var proj = projGO.GetComponent<Projectile>();
            if (proj != null)
            {
                // Use generic Initialize with callback
                proj.Initialize(
                    dir,
                    projectileSpeed,
                    projectileLifetime,
                    gameObject,
                    (hit) => OnProjectileHit(projGO, hit)
                );
            }

            return true;
        }

        /// <summary>
        /// Called when our projectile hits something.
        /// </summary>
        protected virtual void OnProjectileHit(GameObject projectile, GameObject hit)
        {
            if (hit == null) return;

            var iHealth = hit.GetComponent<IHealth>();
            if (iHealth != null && FactionUtility.CanDamage(faction, iHealth, friendlyFire))
            {
                // Get the Health component to call TakeDamage
                var health = hit.GetComponent<Health>();
                if (health != null)
                {
                    health.TakeDamage(projectileDamage, gameObject);
                }
            }

            // Destroy the projectile
            Destroy(projectile);
        }

        /// <summary>
        /// Optional repositioning logic to regain line-of-sight. 
        /// </summary>
        private void TryReposition(GameObject target)
        {
            if (agent == null) return;
            if (Time.time < lastRepositionTime + repositionCooldown) return;
            lastRepositionTime = Time.time;

            Vector2 offset = Random.insideUnitCircle.normalized * repositionRadius;
            Vector3 newPos = target.transform.position + new Vector3(offset.x, offset.y, 0f);

            if (NavMesh.SamplePosition(newPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                Debug.DrawLine(transform.position, hit.position, Color.cyan, 0.5f);
                Debug.Log($"{name} repositioning for clearer shot.");
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, repositionRadius);
        }
#endif
    }
}