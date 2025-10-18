using UnityEngine;
using Havengard.HealthSystem;
using Havengard.Units;
using Havengard.Combat;
using System.Collections.Generic;

namespace Havengard.Abilities
{
    public class Projectile : MonoBehaviour
    {
        [Header("Runtime Info")]
        public Faction SourceFaction { get; private set; }
        public bool FriendlyFire { get; private set; }
        public int Damage { get; private set; }
        public float Speed { get; private set; }

        private Vector2 direction;
        private float lifetime = 5f;

        [Header("Impact Settings")]
        [SerializeField] private GameObject impactEffectPrefab;
        [SerializeField] private GameObject impactMissEffectPrefab;
        [SerializeField] private AudioClip impactSound;
        [SerializeField] private bool destroyOnImpact = true;

        [Header("Piercing Settings")]
        [SerializeField] private bool canPierce = false;
        [SerializeField] private int maxPierces = 0;
        [SerializeField, Range(0.1f, 1f)] private float damageFalloffPerPierce = 1f;

        private int pierceCount = 0;
        private HashSet<IHealth> alreadyHit = new HashSet<IHealth>();

        public void Init(Vector2 dir, Faction faction, bool allowFriendlyFire, int dmg, float projectileSpeed)
        {
            direction = dir.normalized;
            SourceFaction = faction;
            FriendlyFire = allowFriendlyFire;
            Damage = dmg;
            Speed = projectileSpeed;
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            transform.position += (Vector3)(direction * Speed * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var health = other.GetComponent<IHealth>();
            if (health == null) return;
            if (alreadyHit.Contains(health)) return;
            if (!FactionUtility.CanDamage(SourceFaction, health, FriendlyFire)) return;

            alreadyHit.Add(health);

            int damageToDeal = Mathf.RoundToInt(Damage * Mathf.Pow(damageFalloffPerPierce, pierceCount));
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
