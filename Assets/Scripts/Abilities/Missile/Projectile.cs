using System;
using System.Collections;
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
        private Action<GameObject> onHit;

        private Transform homingTarget;
        private float homingStrength;
        private bool isHoming;

        private Rigidbody2D rb;
        private TrailRenderer trailRenderer;
        private SpriteRenderer spriteRenderer;
        private float spawnTime;
        private bool hasHit;
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
            Action<GameObject> onHit)
        {
            this.direction = direction.normalized;
            this.speed = speed;
            this.lifetime = lifetime;
            this.caster = caster;
            this.onHit = onHit;

            spawnTime = Time.time;
            hasHit = false;
            collisionEnabled = false;

            if (rb != null)
            {
                rb.linearVelocity = this.direction * this.speed;
            }

            // Enable collision after a short delay
            StartCoroutine(EnableCollisionAfterDelay(0.1f));
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
            get => onHit;
            set => onHit = value;
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
            if (hasHit) return;
            if (collision.gameObject == caster) return;

            hasHit = true;
            onHit?.Invoke(collision.gameObject);

            // Note: Projectile does not destroy itself here
            // The callback (onHit) is responsible for calling Destroy(projectile)
        }
    }
}