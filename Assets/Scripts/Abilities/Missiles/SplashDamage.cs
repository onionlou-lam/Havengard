using UnityEngine;
using Havengard.HealthSystem;
using Havengard.Units;
using Havengard.Combat;
using Havengard.Statuses;

namespace Havengard.Abilities
{
    public class SplashDamage : MonoBehaviour
    {
        [Header("Splash Settings")]
        [SerializeField] private float radius = 2.5f;

        [Header("AOE Damage (applies to everyone EXCEPT hit target)")]
        [SerializeField] private int aoeDamage = 10;

        [Header("Optional Status Effect")]
        [SerializeField] private StatusEffectData statusEffect;
        [SerializeField] private int maxStatusStacks = 3;

        [Header("VFX/SFX")]
        [SerializeField] private GameObject explosionVFX;
        [SerializeField] private AudioClip explosionSFX;

        private GameObject caster;
        private Faction sourceFaction;
        private bool friendlyFire;

        public void Setup(
            GameObject caster,
            float radius,
            Faction faction,
            bool allowFriendlyFire,
            int aoeDamage,
            StatusEffectData statusEffect,
            int maxStacks,
            GameObject vfx,
            AudioClip sfx)
        {
            this.caster = caster;
            this.radius = radius;
            sourceFaction = faction;
            friendlyFire = allowFriendlyFire;

            this.aoeDamage = Mathf.Max(0, aoeDamage);
            this.statusEffect = statusEffect;
            maxStatusStacks = Mathf.Max(1, maxStacks);

            explosionVFX = vfx;
            explosionSFX = sfx;
        }

        /// <summary>
        /// Called by Projectile when it impacts something.
        /// hitTarget = the unit the projectile directly collided with (can be null if wall hit).
        /// </summary>
        public void HandleProjectileImpact(Vector3 impactPoint, GameObject hitTarget)
        {
            // VFX/SFX
            if (explosionVFX != null)
            {
                var fx = Instantiate(explosionVFX, impactPoint, Quaternion.identity);
                Destroy(fx, 2f);
            }

            if (explosionSFX != null)
                AudioSource.PlayClipAtPoint(explosionSFX, impactPoint, 0.8f);

            // ✅ Apply status to direct-hit target (but NOT AOE damage)
            if (hitTarget != null && statusEffect != null)
            {
                var hitHealth = hitTarget.GetComponent<IHealth>();
                if (hitHealth != null && FactionUtility.CanDamage(sourceFaction, hitHealth, friendlyFire))
                {
                    StatusEffectApplier.ApplyEffect(hitTarget, statusEffect, maxStatusStacks);
                }
            }

            // Splash scan
            Collider2D[] hits = Physics2D.OverlapCircleAll(impactPoint, radius);

            foreach (var col in hits)
            {
                if (col == null) continue;

                var health = col.GetComponent<IHealth>();
                if (health == null) continue;
                if (!FactionUtility.CanDamage(sourceFaction, health, friendlyFire)) continue;

                // ✅ Skip AOE damage on the direct hit target
                if (hitTarget != null && col.gameObject == hitTarget)
                {
                    // still allowed to have status (already applied above)
                    continue;
                }

                if (aoeDamage > 0)
                {
                    int dmg = aoeDamage;

                    // If you want mitigation here, swap to CombatCalculator.
                    // int dmg = CombatCalculator.CalculateDamage(caster, col.gameObject);

                    health.GetHealthSystem().Damage(dmg);
                }

                if (statusEffect != null)
                {
                    StatusEffectApplier.ApplyEffect(col.gameObject, statusEffect, maxStatusStacks);
                }
            }

            Debug.Log($"[SplashDamage] Impact at {impactPoint} radius={radius} aoeDamage={aoeDamage} status={(statusEffect ? statusEffect.name : "none")}");
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
#endif
    }
}
