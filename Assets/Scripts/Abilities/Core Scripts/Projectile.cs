using System;
using System.Collections.Generic;
using UnityEngine;
using Havengard.HealthSystem;
using Havengard.Units;
using Havengard.Combat;

namespace Havengard.Abilities
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        [Header("Runtime Info")]
        public Faction SourceFaction { get; private set; }
        public bool FriendlyFire { get; private set; }
        public int Damage { get; private set; }
        public float Speed { get; private set; }

        [Header("Lifetime")]
        [SerializeField] private float lifetime = 5f;

        [Header("Impact Settings")]
        [SerializeField] private GameObject impactEffectPrefab;
        [SerializeField] private GameObject impactMissEffectPrefab;
        [SerializeField] private AudioClip impactSound;
        [SerializeField] private bool destroyOnImpact = true;

        [Header("Piercing Settings")]
        [SerializeField] private bool canPierce = false;
        [SerializeField] private int maxPierces = 0;
        [SerializeField, Range(0.1f, 1f)] private float damageFalloffPerPierce = 1f;

        private Rigidbody2D rb;
        private Collider2D col;

        private Vector2 direction;
        private int pierceCount = 0;
        private bool impacted = false;

        private readonly HashSet<IHealth> alreadyHit = new HashSet<IHealth>();

        /// <summary>
        /// Fired exactly once when the projectile impacts (unit or wall).
        /// Params: impactPosition, hitCollider (null if none)
        /// </summary>
        public event Action<Vector3, Collider2D> OnImpact;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();

            // Recommended for trigger-based projectile hits
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        public void Init(Vector2 dir, Faction faction, bool allowFriendlyFire, int dmg, float projectileSpeed)
        {
            direction = dir.sqrMagnitude > 0.001f ? dir.normalized : Vector2.right;
            SourceFaction = faction;
            FriendlyFire = allowFriendlyFire;
            Damage = Mathf.Max(0, dmg);
            Speed = Mathf.Max(0f, projectileSpeed);

            // reset runtime state (important if you later pool)
            impacted = false;
            pierceCount = 0;
            alreadyHit.Clear();
            if (col != null) col.enabled = true;

            CancelInvoke();
            Invoke(nameof(KillSelf), lifetime);
        }

        private void FixedUpdate()
        {
            if (impacted) return;
            rb.MovePosition(rb.position + direction * Speed * Time.fixedDeltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (impacted) return;

            // Walls: impact but don't try to damage
            if (other.gameObject.layer == LayerMask.NameToLayer("Walls"))
            {
                Impact(other.transform.position, other);
                return;
            }

            var health = other.GetComponent<IHealth>();
            if (health == null) return;

            if (alreadyHit.Contains(health)) return;
            if (!FactionUtility.CanDamage(SourceFaction, health, FriendlyFire)) return;

            alreadyHit.Add(health);

            int damageToDeal = Mathf.RoundToInt(Damage * Mathf.Pow(damageFalloffPerPierce, pierceCount));
            damageToDeal = Mathf.Max(0, damageToDeal);

            Debug.Log($"[Projectile] Damaging {other.name} for {damageToDeal} (base={Damage}, pierce={pierceCount})");

            health.GetHealthSystem().Damage(damageToDeal);

            pierceCount++;

            SpawnImpactEffect(impactEffectPrefab, other.transform.position);
            if (impactSound != null)
                AudioSource.PlayClipAtPoint(impactSound, other.transform.position, 0.8f);

            if (!canPierce || pierceCount > maxPierces)
            {
                Impact(other.transform.position, other);
            }
        }

        private void Impact(Vector3 position, Collider2D hitCollider)
        {
            if (impacted) return;
            impacted = true;

            // stop motion
            rb.linearVelocity = Vector2.zero;

            // prevent double-trigger while splash/status runs
            if (col != null) col.enabled = false;

            OnImpact?.Invoke(position, hitCollider);

            // If we missed a unit, use miss VFX
            if (hitCollider == null || hitCollider.GetComponent<IHealth>() == null)
                SpawnImpactEffect(impactMissEffectPrefab, position);

            if (destroyOnImpact)
                Destroy(gameObject);
        }

        private void KillSelf()
        {
            if (impacted) return;
            Impact(transform.position, null);
        }

        private void SpawnImpactEffect(GameObject prefab, Vector3 position)
        {
            if (prefab == null) return;
            var effect = Instantiate(prefab, position, Quaternion.identity);
            Destroy(effect, 2f);
        }

        public void ConfigurePiercing(bool enabled, int maxPierce, float falloff)
        {
            canPierce = enabled;
            maxPierces = Mathf.Max(0, maxPierce);
            damageFalloffPerPierce = Mathf.Clamp(falloff, 0.1f, 1f);
        }

        public void ConfigureImpactEffects(GameObject hitVFX, GameObject missVFX, AudioClip hitSound)
        {
            impactEffectPrefab = hitVFX;
            impactMissEffectPrefab = missVFX;
            impactSound = hitSound;
        }
    }
}
