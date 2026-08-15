using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Havengard.Abilities
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        private Vector3 direction;
        private float speed;
        private float lifetime;
        private GameObject caster;
        private Action<GameObject, bool> onHit; // Added bool parameter for shouldDestroy
        private LayerMask wallLayers;

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

            if (rb != null)
            {
                rb.gravityScale = 0f;
            }
        }

        public void Initialize(
            Vector3 direction,
            float speed,
            float lifetime,
            GameObject caster,
            Action<GameObject, bool> onHit,
            LayerMask wallLayers = default,
            bool isPiercing = false,
            int pierceCount = 0)
        {
            this.direction = direction.normalized;
            this.speed = speed;
            this.lifetime = lifetime;
            this.caster = caster;
            this.onHit = onHit;
            this.wallLayers = wallLayers;
            this.isPiercing = isPiercing;
            this.pierceCount = pierceCount;

            spawnTime = Time.time;
            hasHit = false;
            collisionEnabled = false;
            enemiesHit = 0;
            hitEnemies.Clear();

            if (rb != null)
            {
                rb.linearVelocity = this.direction * this.speed;
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

            Initialize(direction, speed, lifetime, caster, wrappedCallback, wallLayers, false, 0);
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

                float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
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