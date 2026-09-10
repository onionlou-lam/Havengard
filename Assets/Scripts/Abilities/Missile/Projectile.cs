using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Havengard.Combat
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        [Header("Rotation Settings")]
        [Tooltip("Offset angle if projectile sprite doesn't face right (0°). E.g., if sprite faces up, use 90")]
        [SerializeField] private float spriteAngleOffset = 90f;

        [Header("Audio (Optional)")]
        [Tooltip("Looping sound that plays while the projectile is in flight (e.g. a magic hum or fire crackle). Routed through GameAudioManager so it pauses correctly and respects concurrency limits.")]
        [SerializeField] private AudioClip loopSFX;
        [SerializeField][Range(0f, 1f)] private float loopVolume = 0.6f;
        [SerializeField][Range(0f, 1f)] private float loopSpatialBlend = 1f;

        private Havengard.Audio.GameAudioManager.LoopHandle loopHandle;

        private Vector3 direction;
        private float speed;
        private float lifetime;
        private GameObject caster;
        private Action<GameObject, bool> onHit; // Added bool parameter for shouldDestroy
        private LayerMask wallLayers;
        private bool ignoreWallCollision;
        private Collider2D ownCollider;

        // Piercing functionality
        private bool isPiercing;
        private int pierceCount; // 0 = infinite piercing
        private int enemiesHit;
        private HashSet<GameObject> hitEnemies = new HashSet<GameObject>();

        private Transform homingTarget;
        private float homingStrength;
        private bool isHoming;

        private Rigidbody2D rb;
        private TrailRenderer trailRenderer;
        private SpriteRenderer spriteRenderer;
        private float spawnTime;
        private bool hasHit; // Only used for non-piercing projectiles
        private bool collisionEnabled;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            trailRenderer = GetComponent<TrailRenderer>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            ownCollider = GetComponent<Collider2D>();

            if (rb != null)
            {
                rb.gravityScale = 0f;
            }

            // Start looping flight SFX, if configured. Routed through GameAudioManager so it
            // automatically pauses/resumes with the game's pause state instead of ignoring
            // Time.timeScale, and so concurrency limiting applies if many projectiles share the clip.
            if (loopSFX != null && Havengard.Audio.GameAudioManager.Instance != null)
            {
                loopHandle = Havengard.Audio.GameAudioManager.Instance.PlayLoop(loopSFX, transform, loopVolume, loopSpatialBlend);
            }
        }

        private void StopLoopSFX()
        {
            if (loopHandle.IsValid && Havengard.Audio.GameAudioManager.Instance != null)
            {
                Havengard.Audio.GameAudioManager.Instance.StopLoop(loopHandle);
                loopHandle = Havengard.Audio.GameAudioManager.LoopHandle.Invalid;
            }
        }

        private void OnDestroy()
        {
            StopLoopSFX();
        }

        public void Initialize(
            Vector3 direction,
            float speed,
            float lifetime,
            GameObject caster,
            Action<GameObject, bool> onHit,
            LayerMask wallLayers = default,
            bool isPiercing = false,
            int pierceCount = 0,
            bool ignoreWallCollision = false)
        {
            this.direction = direction.normalized;
            this.speed = speed;
            this.lifetime = lifetime;
            this.caster = caster;
            this.onHit = onHit;
            this.wallLayers = wallLayers;
            this.isPiercing = isPiercing;
            this.pierceCount = pierceCount;
            this.ignoreWallCollision = ignoreWallCollision;

            spawnTime = Time.time;
            hasHit = false;
            collisionEnabled = false;
            enemiesHit = 0;
            hitEnemies.Clear();

            if (rb != null)
            {
                rb.linearVelocity = this.direction * this.speed;
            }

            // Rotate projectile to face direction of travel
            RotateToDirection(this.direction);

            // If fired from atop the wall, ignore collision with all registered wall colliders
            // so the shot can pass "over" the wall down to enemies below.
            if (ignoreWallCollision && ownCollider != null)
            {
                var wallColliders = WallColliderRegistry.GetAll();
                for (int i = 0; i < wallColliders.Count; i++)
                {
                    if (wallColliders[i] != null)
                        Physics2D.IgnoreCollision(ownCollider, wallColliders[i], true);
                }
            }

            // Enable collision after a short delay
            StartCoroutine(EnableCollisionAfterDelay(0.1f));
        }

        // Overload for backward compatibility with old signature
        public void Initialize(
            Vector3 direction,
            float speed,
            float lifetime,
            GameObject caster,
            Action<GameObject> onHit,
            LayerMask wallLayers = default)
        {
            // Wrap old callback to new signature
            Action<GameObject, bool> wrappedCallback = (hit, shouldDestroy) =>
            {
                onHit?.Invoke(hit);
            };

            Initialize(direction, speed, lifetime, caster, wrappedCallback, wallLayers, false, 0, false);
        }

        private IEnumerator EnableCollisionAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            collisionEnabled = true;
        }

        public void ConfigureVisuals(Color color, float trailTime = 0.5f)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = color;
            }

            if (trailRenderer != null)
            {
                trailRenderer.time = trailTime;
                trailRenderer.startColor = color;
                trailRenderer.endColor = new Color(color.r, color.g, color.b, 0f);
            }

            var particles = GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particles)
            {
                var main = ps.main;
                main.startColor = color;
            }
        }

        public void SetHomingTarget(Transform target, float strength)
        {
            homingTarget = target;
            homingStrength = strength;
            isHoming = true;
        }

        // Overload for compatibility with enemy code (strength, delay, target)
        public void EnableHoming(float strength, float delay, GameObject target)
        {
            if (target != null)
            {
                // Note: delay parameter ignored for simplicity, could be implemented with coroutine if needed
                SetHomingTarget(target.transform, strength);
            }
        }

        public void EnableHoming(Transform target, float strength)
        {
            SetHomingTarget(target, strength);
        }

        public Action<GameObject> OnImpact
        {
            get => (hit) => onHit?.Invoke(hit, true); // Default to destroying
            set => onHit = (hit, shouldDestroy) => value?.Invoke(hit);
        }

        private void Update()
        {
            if (Time.time >= spawnTime + lifetime)
            {
                Destroy(gameObject);
                return;
            }

            if (isHoming && homingTarget != null && rb != null)
            {
                Vector2 targetDirection = (homingTarget.position - transform.position).normalized;
                rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetDirection * speed, Time.deltaTime * homingStrength);

                // Update rotation for homing projectiles
                RotateToDirection(rb.linearVelocity);
            }
        }

        /// <summary>
        /// Rotate the projectile to face its direction of travel (2D)
        /// Accounts for sprite orientation offset
        /// </summary>
        private void RotateToDirection(Vector3 dir)
        {
            if (dir.sqrMagnitude > 0.001f)
            {
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                // Apply sprite offset (e.g., if sprite faces up instead of right, add 90)
                transform.rotation = Quaternion.Euler(0, 0, angle - spriteAngleOffset);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collisionEnabled) return;
            if (collision.gameObject == caster) return;

            // For non-piercing projectiles, use old behavior
            if (!isPiercing)
            {
                if (hasHit) return;
                hasHit = true;
                onHit?.Invoke(collision.gameObject, true); // Should destroy
                return;
            }

            // For piercing projectiles
            // Check if already hit this enemy
            if (hitEnemies.Contains(collision.gameObject))
            {
                return; // Skip enemies we've already hit
            }

            // Add to hit list
            hitEnemies.Add(collision.gameObject);
            enemiesHit++;

            // Determine if projectile should be destroyed after this hit
            bool shouldDestroy = false;

            // Check if we've reached pierce limit (0 = infinite)
            if (pierceCount > 0 && enemiesHit >= pierceCount)
            {
                shouldDestroy = true;
            }

            // Invoke callback with shouldDestroy flag
            onHit?.Invoke(collision.gameObject, shouldDestroy);

            // Note: The callback is responsible for calling Destroy(projectile) if shouldDestroy is true
        }

        /// <summary>
        /// Get the number of enemies hit so far
        /// </summary>
        public int GetEnemiesHit()
        {
            return enemiesHit;
        }

        /// <summary>
        /// Check if a specific enemy has been hit
        /// </summary>
        public bool HasHitEnemy(GameObject enemy)
        {
            return hitEnemies.Contains(enemy);
        }
    }
}