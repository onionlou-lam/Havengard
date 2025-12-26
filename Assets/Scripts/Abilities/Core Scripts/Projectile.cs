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
        [Tooltip("Optional: if set, projectile will treat this layer as walls/solid geometry.")]
        [SerializeField] private string wallsLayerName = "Walls";
        [Tooltip("Optional: if set, projectile will treat this tag as walls/solid geometry.")]
        [SerializeField] private string wallsTag = "Wall";

        [Header("Impact Settings")]
        [SerializeField] private GameObject impactHitVFX;   // hit a unit
        [SerializeField] private GameObject impactWallVFX;  // hit a wall
        [SerializeField] private AudioClip impactSound;
        [SerializeField] private float impactSoundVolume = 0.8f;
        [SerializeField] private bool destroyOnImpact = true;

        [Header("Projectile VFX (Optional)")]
        [SerializeField] private GameObject muzzleVFX;
        [SerializeField] private float muzzleLifetime = 1.5f;
        [SerializeField] private GameObject projectileVFX;
        [SerializeField] private GameObject[] trailVFX;
        [SerializeField] private float detachedTrailLifetime = 2f;

        [Header("Piercing Settings")]
        [SerializeField] private bool canPierce = false;
        [SerializeField] private int maxPierces = 0;
        [SerializeField, Range(0.1f, 1f)] private float damageFalloffPerPierce = 1f;

        private Vector2 direction;
        private Rigidbody2D rb;
        private Collider2D col;
        private bool hasImpacted;

        private int pierceCount = 0;
        private readonly HashSet<IHealth> alreadyHit = new HashSet<IHealth>();

        private GameObject projectileVfxInstance;
        private readonly List<GameObject> trailInstances = new List<GameObject>();

        private int wallsLayer = -1;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();

            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.freezeRotation = true;

            // We are using trigger-based hits
            if (col != null) col.isTrigger = true;

            wallsLayer = !string.IsNullOrWhiteSpace(wallsLayerName)
                ? LayerMask.NameToLayer(wallsLayerName)
                : -1;
        }

        public void Init(Vector2 dir, Faction faction, bool allowFriendlyFire, int dmg, float projectileSpeed)
        {
            direction = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.right;
            SourceFaction = faction;
            FriendlyFire = allowFriendlyFire;
            Damage = dmg;
            Speed = projectileSpeed;

            SpawnLaunchVFX();

            if (lifetime > 0f)
                Destroy(gameObject, lifetime);
        }

        private void FixedUpdate()
        {
            // Critical: once impacted, never re-apply velocity
            if (hasImpacted) return;

            if (rb != null)
                rb.linearVelocity = direction * Speed;
        }

        private void SpawnLaunchVFX()
        {
            if (muzzleVFX != null)
            {
                var muzzle = Instantiate(muzzleVFX, transform.position, transform.rotation);
                Destroy(muzzle, Mathf.Max(0.1f, muzzleLifetime));
            }

            if (projectileVFX != null)
            {
                projectileVfxInstance = Instantiate(projectileVFX, transform.position, transform.rotation, transform);
            }

            if (trailVFX != null && trailVFX.Length > 0)
            {
                foreach (var t in trailVFX)
                {
                    if (t == null) continue;
                    Transform parent = projectileVfxInstance != null ? projectileVfxInstance.transform : transform;
                    var trail = Instantiate(t, parent.position, parent.rotation, parent);
                    trailInstances.Add(trail);
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (hasImpacted) return;

            // WALL HIT
            if (IsWall(other))
            {
                Impact(position: other.ClosestPoint(transform.position), hitTransform: other.transform, hitUnit: false);

                // Stop + destroy so it can't ghost through
                if (destroyOnImpact)
                    Destroy(gameObject);

                return;
            }

            // UNIT HIT (use parent so child colliders work)
            var health = other.GetComponentInParent<IHealth>();
            if (health == null) return;
            if (alreadyHit.Contains(health)) return;
            if (!FactionUtility.CanDamage(SourceFaction, health, FriendlyFire)) return;

            alreadyHit.Add(health);

            int damageToDeal = Mathf.RoundToInt(Damage * Mathf.Pow(damageFalloffPerPierce, pierceCount));
            damageToDeal = Mathf.Max(0, damageToDeal);

            health.GetHealthSystem()?.Damage(damageToDeal);

            pierceCount++;

            Impact(position: other.ClosestPoint(transform.position), hitTransform: other.transform, hitUnit: true);

            if (!canPierce || pierceCount > maxPierces)
            {
                if (destroyOnImpact)
                    Destroy(gameObject);
            }
        }

        private bool IsWall(Collider2D other)
        {
            if (wallsLayer >= 0 && other.gameObject.layer == wallsLayer)
                return true;

            if (!string.IsNullOrWhiteSpace(wallsTag) && other.CompareTag(wallsTag))
                return true;

            return false;
        }

        private void Impact(Vector3 position, Transform hitTransform, bool hitUnit)
        {
            hasImpacted = true;

            // Prevent further triggers immediately
            if (col != null) col.enabled = false;

            // Stop motion immediately
            if (rb != null) rb.linearVelocity = Vector2.zero;

            // Spawn VFX
            var vfx = hitUnit ? impactHitVFX : impactWallVFX;
            if (vfx != null)
            {
                var fx = Instantiate(vfx, position, Quaternion.identity);
                Destroy(fx, 2f);
            }

            // Play SFX
            if (impactSound != null)
                AudioSource.PlayClipAtPoint(impactSound, position, impactSoundVolume);

            DetachTrails();
        }

        private void DetachTrails()
        {
            for (int i = 0; i < trailInstances.Count; i++)
            {
                var trail = trailInstances[i];
                if (trail == null) continue;

                trail.transform.SetParent(null, worldPositionStays: true);
                Destroy(trail, Mathf.Max(0.1f, detachedTrailLifetime));
            }

            if (projectileVfxInstance != null)
            {
                projectileVfxInstance.transform.SetParent(null, worldPositionStays: true);
                Destroy(projectileVfxInstance, Mathf.Max(0.1f, detachedTrailLifetime));
            }
        }

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
