using Havengard.Combat;
using Havengard.HealthSystem;
using Havengard.Units;
using System;
using UnityEngine;

namespace Havengard.Abilities
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        [Header("Projectile Settings")]
        [SerializeField] private float lifetime = 5f;

        [Header("Impact VFX")]
        [SerializeField] private GameObject hitVFX;
        [SerializeField] private GameObject wallHitVFX;
        [SerializeField] private float impactDestroyDelay = 0.05f; // small, but visible

        [Header("Impact SFX")]
        [SerializeField] private AudioClip hitSFX;
        [SerializeField] private AudioClip wallHitSFX;

        [Header("Trail VFX (Optional)")]
        [SerializeField] private TrailRenderer trail;
        [SerializeField] private ParticleSystem trailParticles;

        public event Action<Vector3, Collider2D> OnImpact;

        private Rigidbody2D rb;
        private int damage;
        private Faction sourceFaction;
        private bool allowFriendlyFire;
        private bool hasHit;
        private Collider2D col;
        private SpriteRenderer sr;
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            sr = GetComponentInChildren<SpriteRenderer>(); // or GetComponent<SpriteRenderer>() if on root
            rb.gravityScale = 0f;
        }

        public void Initialize(Vector2 direction, Faction faction, bool friendlyFire, int damage, float speed)
        {
            this.damage = damage;
            this.sourceFaction = faction;
            this.allowFriendlyFire = friendlyFire;

            rb.linearVelocity = direction.normalized * speed;

            // Rotate to face direction
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            Destroy(gameObject, lifetime);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (hasHit) return;

            var health = collision.GetComponent<IHealth>();

            // Hit a valid target
            if (health != null && FactionUtility.CanDamage(sourceFaction, health, allowFriendlyFire))
            {
                health.GetHealthSystem().Damage(damage);
                HandleImpact(true, collision, collision.transform.position);
                return;
            }

            // Hit a wall or obstacle
            if (collision.gameObject.layer == LayerMask.NameToLayer("Obstacles") ||
                collision.gameObject.CompareTag("Wall"))
            {
                HandleImpact(false, collision, transform.position);
            }
        }
        private void HandleImpact(bool hitTarget, Collider2D collider, Vector3 fxPos)
        {
            if (hasHit) return;
            hasHit = true;

            // Stop physics immediately
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.simulated = false;
            }

            // Disable collider so we don't re-trigger
            if (col != null) col.enabled = false;

            // Hide sprite immediately (optional; if you want it to linger, remove this)
            if (sr != null) sr.enabled = false;

            // Detach trail so it can fade naturally
            if (trail != null)
            {
                trail.transform.SetParent(null, true);
                trail.autodestruct = true;
                trail.emitting = false;
            }

            if (trailParticles != null)
            {
                trailParticles.transform.SetParent(null, true);
                trailParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                Destroy(trailParticles.gameObject, 2f);
            }

            // Play VFX/SFX (your existing method already instantiates VFX separately)
            PlayImpactEffects(hitTarget, fxPos);

            // Let systems like SplashDamage react (keep this!)
            OnImpact?.Invoke(transform.position, collider);

            // Destroy after a tiny delay so audio/visual isn't cut off
            Destroy(gameObject, impactDestroyDelay);
        }

        private void PlayImpactEffects(bool hitTarget, Vector3 position)
        {
            // Spawn VFX
            GameObject vfx = hitTarget ? hitVFX : wallHitVFX;
            if (vfx != null)
            {
                GameObject fxInstance = Instantiate(vfx, position, Quaternion.identity);
                Destroy(fxInstance, 2f);
            }

            // Play SFX
            AudioClip sfx = hitTarget ? hitSFX : wallHitSFX;
            if (sfx != null)
            {
                AudioSource.PlayClipAtPoint(sfx, position);
            }
        }

    }
}