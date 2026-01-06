using UnityEngine;
using Havengard.HealthSystem;
using Havengard.Units;
using Havengard.Combat;
using Havengard.Statuses;

namespace Havengard.Abilities
{
    public class FireballExplosion : MonoBehaviour
    {
        [Header("Explosion")]
        [SerializeField] private float radius = 2.5f;

        [Header("Damage")]
        [SerializeField] private int damage = 15;

        [Header("Burn (optional)")]
        [SerializeField] private StatusEffectData burnEffect;   // assign your Burn SO here (optional)
        [SerializeField] private bool applyBurn = false;

        private GameObject caster;
        private Faction casterFaction;
        private bool friendlyFire;

        private bool hasExploded;

        /// <summary>
        /// Called by MissileAbility (or any missile) when spawning projectile.
        /// </summary>
        public void Setup(GameObject caster, float explosionRadius, Faction faction, bool allowFriendly, int dmg, StatusEffectData burn = null)
        {
            this.caster = caster;
            radius = explosionRadius;
            casterFaction = faction;
            friendlyFire = allowFriendly;
            damage = dmg;

            burnEffect = burn;
            applyBurn = burnEffect != null;
        }

        /// <summary>
        /// Called by Projectile on impact.
        /// </summary>
        public void HandleProjectileImpact(Vector3 impactPoint, bool hitWasUnit)
        {
            if (hasExploded) return;
            hasExploded = true;

            Explode(impactPoint);
        }

        private void Explode(Vector3 center)
        {
            var hits = Physics2D.OverlapCircleAll(center, radius);

            foreach (var hit in hits)
            {
                // Important: use GetComponentInParent in case colliders are on child objects.
                var health = hit.GetComponentInParent<IHealth>();
                if (health == null) continue;

                if (!FactionUtility.CanDamage(casterFaction, health, friendlyFire))
                    continue;

                // Damage
                var hs = health.GetHealthSystem();
                if (hs != null)
                    hs.Damage(damage);

                // Burn / DoT
                if (applyBurn && burnEffect != null)
                    ApplyOrStackStatus(health, burnEffect);
            }

            Debug.Log($"Fireball exploded at {center}, radius={radius}, damage={damage}");
        }

        private static void ApplyOrStackStatus(IHealth targetHealth, StatusEffectData data)
        {
            var targetMono = targetHealth as MonoBehaviour;
            if (targetMono == null) return;

            // If there is already a StatusEffectInstance, use it to refresh/stack.
            var existing = targetMono.GetComponent<StatusEffectInstance>();
            if (existing != null)
            {
                existing.RefreshOrStack(data);
                return;
            }

            // Otherwise add one and apply.
            var instance = targetMono.gameObject.AddComponent<StatusEffectInstance>();
            instance.Apply(data, targetHealth);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
#endif
    }
}
