using System;
using System.Collections.Generic;
using UnityEngine;
using Havengard.HealthSystem;
using Havengard.Units;
using Havengard.Combat;

namespace Havengard.Abilities
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    [DisallowMultipleComponent]
    public class Projectile : MonoBehaviour
    {
        [Header("Runtime Info")]
        public Faction SourceFaction { get; private set; }
        public bool FriendlyFire { get; private set; }
        public int Damage { get; private set; }
        public float Speed { get; private set; }

        [Header("Movement")]
        [SerializeField] private float lifetime = 5f;

        [Header("Collision")]
        [SerializeField] private string wallsLayerName = "Walls";
        [SerializeField] private string wallsTag = "Wall";

        [Header("Impact Settings")]
        [SerializeField] private GameObject impactHitVFX;   // hit a unit
        [SerializeField] private GameObject impactWallVFX;  // hit a wall
        [SerializeField] private AudioClip impactSound;
        [SerializeField] private float impactSoundVolume = 0.8f;
        [SerializeField] private bool destroyOnImpact = true;

        [Header("Piercing Settings")]
        [SerializeField] private bool canPierce = false;
        [SerializeField] private int maxPierces = 0;
        [SerializeField, Range(0.1f, 1f)] private float damageFalloffPerPierce = 1f;

        /// <summary>
        /// (impactPoint, hitWasUnit)
        /// Subscribe with SetImpactListener/AddImpactListener.
        /// </summary>
        public event Action<Vector3, bool> OnImpacted;

        private Vector2 direction;
        private Rigidbody2D rb;
        private Collider2D col;
        private bool hasImpacted;

        private int pierceCount = 0;
        private readonly HashSet<IHealth> alreadyHit = new HashSet<IHealth>();
        private int wallsLayer = -1;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();

            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.freezeRotation = true;

            // Trigger-based hits (works with Tilemap/Composite colliders as long as ONE side has a Rigidbody2D)
            col.isTrigger = true;

            wallsLayer = !string.IsNullOrWhiteSpace(wallsLayerName)
                ? LayerMask.NameToLayer(wallsLayerName)
                : -1;
        }

        private void OnEnable()
        {
            // If something reuses/enables this projectile again (pooling or scene toggles),
            // make sure it is in a valid state.
            if (hasImpacted || !col.enabled)
                ResetForReuse();
        }

        /// <summary>
        /// Call this when reusing projectiles (pooling) OR if you ever see later projectiles with disabled collider.
        /// </summary>
        public void ResetForReuse()
        {
            hasImpacted = false;
            pierceCount = 0;
            alreadyHit.Clear();

            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            if (col != null)
                col.enabled = true;
        }

        public void Init(Vector2 dir, Faction faction, bool allowFriendlyFire, int dmg, float projectileSpeed)
        {
            ResetForReuse();

            direction = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.right;
            SourceFaction = faction;
            FriendlyFire = allowFriendlyFire;
            Damage = dmg;
            Speed = projectileSpeed;

            if (lifetime > 0f)
                Destroy(gameObject, lifetime);
        }

        private void FixedUpdate()
        {
            if (hasImpacted) return;
            rb.linearVelocity = direction * Speed;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (hasImpacted) return;

            // WALL HIT
            if (IsWall(other))
            {
                Vector3 p = other.ClosestPoint(transform.position);
                Impact(p, hitUnit: false);

                if (destroyOnImpact)
                    Destroy(gameObject);

                return;
            }

            // UNIT HIT
            var health = other.GetComponentInParent<IHealth>();
            if (health == null) return;
            if (alreadyHit.Contains(health)) return;
            if (!FactionUtility.CanDamage(SourceFaction, health, FriendlyFire)) return;

            alreadyHit.Add(health);

            int damageToDeal = Mathf.RoundToInt(Damage * Mathf.Pow(damageFalloffPerPierce, pierceCount));
            damageToDeal = Mathf.Max(0, damageToDeal);

            // Apply damage
            health.GetHealthSystem()?.Damage(damageToDeal);

            pierceCount++;

            Vector3 hitPoint = other.ClosestPoint(transform.position);
            Impact(hitPoint, hitUnit: true);

            if (!canPierce || pierceCount > maxPierces)
            {
                if (destroyOnImpact)
                    Destroy(gameObject);
            }
        }

        private bool IsWall(Collider2D other)
        {
            if (wallsLayer >= 0 && other.gameObject.layer == wallsLayer) return true;
            if (!string.IsNullOrWhiteSpace(wallsTag) && other.CompareTag(wallsTag)) return true;
            return false;
        }

        private void Impact(Vector3 position, bool hitUnit)
        {
            hasImpacted = true;

            // stop further triggers + movement
            if (col != null) col.enabled = false;
            if (rb != null) rb.linearVelocity = Vector2.zero;

            // notify listeners (explosions, special effects, etc.)
            OnImpacted?.Invoke(position, hitUnit);

            // vfx
            var vfx = hitUnit ? impactHitVFX : impactWallVFX;
            if (vfx != null)
            {
                var fx = Instantiate(vfx, position, Quaternion.identity);
                Destroy(fx, 2f);
            }

            // sfx
            if (impactSound != null)
                AudioSource.PlayClipAtPoint(impactSound, position, Mathf.Clamp01(impactSoundVolume));
        }

        // ---------------------------
        // Impact Listener API (NEW)
        // ---------------------------

        /// <summary>
        /// Typical use: projectile.SetImpactListener(explosion.HandleProjectileImpact);
        /// If clearExisting=true (default), it replaces any previous listeners.
        /// </summary>
        public void SetImpactListener(Action<Vector3, bool> listener, bool clearExisting = true)
        {
            if (clearExisting) OnImpacted = null;
            if (listener != null) OnImpacted += listener;
        }

        public void AddImpactListener(Action<Vector3, bool> listener)
        {
            if (listener != null) OnImpacted += listener;
        }

        public void RemoveImpactListener(Action<Vector3, bool> listener)
        {
            if (listener != null) OnImpacted -= listener;
        }

        // ---------------------------
        // Config helpers
        // ---------------------------

        public void ConfigureImpactEffects(GameObject hitVfx, GameObject wallVfx, AudioClip hitSound, float volume = 0.8f)
        {
            impactHitVFX = hitVfx;
            impactWallVFX = wallVfx;
            impactSound = hitSound;
            impactSoundVolume = Mathf.Clamp01(volume);
        }

        public void ConfigurePiercing(bool enabled, int maxPierce, float falloff)
        {
            canPierce = enabled;
            maxPierces = Mathf.Max(0, maxPierce);
            damageFalloffPerPierce = Mathf.Clamp(falloff, 0.1f, 1f);
        }
    }
}
