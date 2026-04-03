using Havengard.Combat;
using Havengard.Core.HealthSystem;
using Havengard.Statuses;
using Havengard.Units;
using UnityEngine;

namespace Havengard.Abilities
{
    public class SplashDamage : MonoBehaviour
    {
        [Header("Splash Settings")]
        [SerializeField] private float radius = 2.5f;
        [SerializeField] private int aoeDamage = 10;

        [Header("VFX/SFX")]
        [SerializeField] private GameObject explosionVFX;
        [SerializeField] private AudioClip explosionSFX;

        private GameObject caster;
        private Faction sourceFaction;
        private bool friendlyFire;
        private StatusEffectData statusEffect;
        private int maxStatusStacks;

        public void Initialize(
            GameObject caster,
            float radius,
            Faction faction,
            bool allowFriendlyFire,
            int aoeDamage,
            StatusEffectData statusEffect = null,
            int maxStacks = 1)
        {
            this.caster = caster;
            this.radius = radius;
            this.sourceFaction = faction;
            this.friendlyFire = allowFriendlyFire;
            this.aoeDamage = Mathf.Max(0, aoeDamage);
            this.statusEffect = statusEffect;
            this.maxStatusStacks = Mathf.Max(1, maxStacks);
        }

        /// <summary>
        /// Called by Projectile when it impacts something. 
        /// hitTarget = the unit the projectile directly collided with (can be null if wall hit).
        /// </summary>
        public void HandleProjectileImpact(Vector3 impactPoint, GameObject hitTarget)
        {
            // Play explosion VFX
            if (explosionVFX != null)
            {
                GameObject fx = Instantiate(explosionVFX, impactPoint, Quaternion.identity);
                Destroy(fx, 2f);
            }

            // Play explosion SFX
            if (explosionSFX != null)
            {
                AudioSource.PlayClipAtPoint(explosionSFX, impactPoint, 0.8f);
            }

            // Apply status to direct-hit target (but NOT AOE damage)
            if (hitTarget != null && statusEffect != null)
            {
                var hitHealth = hitTarget.GetComponent<IHealth>();
                if (hitHealth != null && FactionUtility.CanDamage(sourceFaction, hitHealth, friendlyFire))
                {
                    var applier = hitTarget.GetComponent<StatusEffectApplier>();
                    if (applier != null)
                    {
                        for (int i = 0; i < maxStatusStacks; i++)
                        {
                            applier.ApplyStatusEffect(statusEffect, caster);
                        }
                    }
                }
            }

            // Apply splash damage in radius
            if (radius > 0f)
            {
                Collider2D[] hits = Physics2D.OverlapCircleAll(impactPoint, radius);

                foreach (var col in hits)
                {
                    if (col == null) continue;

                    var health = col.GetComponent<IHealth>();
                    if (health == null) continue;
                    if (!FactionUtility.CanDamage(sourceFaction, health, friendlyFire)) continue;

                    // Skip AOE damage on the direct hit target (they already took projectile damage)
                    if (hitTarget != null && col.gameObject == hitTarget)
                    {
                        continue; // Status already applied above
                    }

                    // Apply AOE damage
                    if (aoeDamage > 0)
                    {
                        health.GetHealthSystem().Damage(aoeDamage);
                    }

                    // Apply status effect
                    if (statusEffect != null)
                    {
                        var applier = col.GetComponent<StatusEffectApplier>();
                        if (applier != null)
                        {
                            for (int i = 0; i < maxStatusStacks; i++)
                            {
                                applier.ApplyStatusEffect(statusEffect, caster);
                            }
                        }
                    }
                }
            }
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