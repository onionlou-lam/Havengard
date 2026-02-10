using Havengard.Combat;
using Havengard.HealthSystem;
using Havengard.Units;
using Havengard.Abilities; // ADD THIS - for LifestealHandler
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
        [SerializeField] private float impactDestroyDelay = 0.05f;

        [Header("Impact SFX")]
        [SerializeField] private AudioClip hitSFX;
        [SerializeField] private AudioClip wallHitSFX;

        [Header("Trail VFX (Optional)")]
        [SerializeField] private TrailRenderer trail;
        [SerializeField] private ParticleSystem trailParticles;

        [Header("Homing Settings")]
        [SerializeField] private bool enableHoming = false;
        [SerializeField] private float homingStrength = 5f;
        [SerializeField] private float homingDelay = 0f; // Delay before homing starts

        public event Action<Vector3, Collider2D> OnImpact;

        private Rigidbody2D rb;
        private int damage;
        private Faction sourceFaction;
        private bool allowFriendlyFire;
        private bool hasHit;
        private Collider2D col;
        private SpriteRenderer sr;
        private float speed;
        private Vector2 direction;
        private GameObject casterGameObject; // ADD THIS LINE
        
        // Homing variables
        private GameObject homingTarget;
        private float launchTime;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            sr = GetComponentInChildren<SpriteRenderer>();
            rb.gravityScale = 0f;
        }

        public void Initialize(Vector2 direction, Faction faction, bool friendlyFire, int damage, float speed, GameObject caster = null)
        {
            this.damage = damage;
            this.sourceFaction = faction;
            this.allowFriendlyFire = friendlyFire;
            this.speed = speed;
            this.direction = direction.normalized;
            this.launchTime = Time.time;
            this.casterGameObject = caster; // ADD THIS LINE

            rb.linearVelocity = this.direction * speed;

            // Rotate to face direction
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            Destroy(gameObject, lifetime);
        }

        /// <summary>
        /// Enable homing behavior with optional target override
        /// </summary>
        public void EnableHoming(float strength = 5f, float delay = 0f, GameObject specificTarget = null)
        {
            enableHoming = true;
            homingStrength = strength;
            homingDelay = delay;
            homingTarget = specificTarget;
        }

        private void FixedUpdate()
        {
            if (hasHit || !enableHoming) return;

            // Check if homing delay has passed
            if (Time.time < launchTime + homingDelay) return;

            // Find or update target
            if (homingTarget == null || !homingTarget.activeInHierarchy)
            {
                homingTarget = FindNearestTarget();
            }

            if (homingTarget != null)
            {
                ApplyHoming();
            }
        }

        private GameObject FindNearestTarget()
        {
            GameObject closest = null;
            float closestDist = Mathf.Infinity;

            foreach (var unit in UnitTargetManager.ActiveUnits)
            {
                if (FactionUtility.CanDamage(sourceFaction, unit, allowFriendlyFire))
                {
                    var mb = unit as MonoBehaviour;
                    if (mb == null || !mb.gameObject.activeInHierarchy) continue;

                    float dist = Vector2.Distance(transform.position, mb.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = mb.gameObject;
                    }
                }
            }

            return closest;
        }

        private void ApplyHoming()
        {
            Vector2 targetPos = homingTarget.transform.position;
            Vector2 currentPos = transform.position;
            Vector2 desiredDirection = (targetPos - currentPos).normalized;

            // Smoothly steer towards target
            Vector2 currentVelocity = rb.linearVelocity;
            Vector2 steeringForce = (desiredDirection * speed - currentVelocity) * homingStrength;
            
            rb.linearVelocity = Vector2.ClampMagnitude(currentVelocity + steeringForce * Time.fixedDeltaTime, speed);

            // Rotate sprite to face velocity direction
            if (rb.linearVelocity.sqrMagnitude > 0.01f)
            {
                float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (hasHit) return;

            var health = collision.GetComponent<IHealth>();

            // Hit a valid target
            if (health != null && FactionUtility.CanDamage(sourceFaction, health, allowFriendlyFire))
            {
                health.GetHealthSystem().Damage(damage);
                
                // Apply lifesteal - need to find the caster
                // Projectiles need to store reference to caster GameObject
                if (casterGameObject != null)
                {
                    // You'll need to add this method to projectile or call LifestealHandler
                    LifestealHandler.ApplyLifesteal(casterGameObject, damage);
                }
                
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

            // Hide sprite immediately
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

            // Play VFX/SFX
            PlayImpactEffects(hitTarget, fxPos);

            // Notify listeners (for splash damage, etc.)
            OnImpact?.Invoke(transform.position, collider);

            // Destroy after a tiny delay
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