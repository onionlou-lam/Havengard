using UnityEngine;
using Havengard.HealthSystem;
using Havengard.Units;
using Havengard.Combat;
using System.Collections.Generic;

namespace Havengard.Abilities
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        [Header("Runtime Info")]
        public Faction SourceFaction { get; private set; }
        public bool FriendlyFire { get; private set; }
        public int Damage { get; private set; }
        public float Speed { get; private set; }

        private Vector2 direction;
        private Rigidbody2D rb;
        private float lifetime = 5f;

        [Header("Impact Settings")]
        [SerializeField] private GameObject impactEffectPrefab;       // hit effect (enemy/target)
        [SerializeField] private GameObject impactMissEffectPrefab;   // miss effect (wall)
        [SerializeField] private AudioClip impactSound;
        [SerializeField] private bool destroyOnImpact = true;

        [Header("Piercing Settings")]
        [SerializeField] private bool canPierce = false;
        [SerializeField] private int maxPierces = 0;
        [SerializeField, Range(0.1f, 1f)] private float damageFalloffPerPierce = 1f;

        private int pierceCount = 0;
        private HashSet<IHealth> alreadyHit = new HashSet<IHealth>();

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.freezeRotation = true;
        }

        public void Init(Vector2 dir, Faction faction, bool allowFriendlyFire, int dmg, float projectileSpeed)
        {
            direction = dir.normalized;
            SourceFaction = faction;
            FriendlyFire = allowFriendlyFire;
            Damage = dmg;
            Speed = projectileSpeed;

            Destroy(gameObject, lifetime);
        }

        private void FixedUpdate()
        {
            // Use physics velocity for collision-aware movement
            if (rb != null)
                rb.linearVelocity = direction * Speed;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log($"Projectile hit: {other.name} on layer {LayerMask.LayerToName(other.gameObject.layer)}");
            // Check if collided with a wall or solid geometry
            if (other.gameObject.layer == LayerMask.NameToLayer("Walls") ||
                other.gameObject.CompareTag("Wall"))
            {
                HandleWallCollision(other);
                return;
            }

            // Check for hit targets
            var health = other.GetComponentInParent<IHealth>();
            if (health == null) return;
            if (alreadyHit.Contains(health)) return;
            if (!FactionUtility.CanDamage(SourceFaction, health, FriendlyFire)) return;

            alreadyHit.Add(health);

            int damageToDeal = Mathf.RoundToInt(Damage * Mathf.Pow(damageFalloffPerPierce, pierceCount));
            Debug.Log($"[Projectile] Damaging {other.name} for {damageToDeal}. HP before={health.GetHealthSystem()?.GetHealth()}");
            health.GetHealthSystem().Damage(damageToDeal);

            pierceCount++;
            SpawnImpactEffect(impactEffectPrefab, other.transform.position);

            if (impactSound != null)
                AudioSource.PlayClipAtPoint(impactSound, other.transform.position, 0.8f);

            if (!canPierce || pierceCount > maxPierces)
            {
                if (destroyOnImpact)
                    Destroy(gameObject);
            }
        }

        private void HandleWallCollision(Collider2D wallCollider)
        {
            // Stop movement
            rb.linearVelocity = Vector2.zero;

            // Spawn "miss" or "impact" VFX
            if (impactMissEffectPrefab != null)
                SpawnImpactEffect(impactMissEffectPrefab, transform.position);
            else if (impactEffectPrefab != null)
                SpawnImpactEffect(impactEffectPrefab, transform.position);

            if (impactSound != null)
                AudioSource.PlayClipAtPoint(impactSound, transform.position, 0.8f);

            Destroy(gameObject);
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
