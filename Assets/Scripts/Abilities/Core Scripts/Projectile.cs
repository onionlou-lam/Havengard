using UnityEngine;
using Havengard.HealthSystem;
using Havengard.Units;
using Havengard.Combat;
using Havengard.Utility;

namespace Havengard.Abilities
{
    public class Projectile : MonoBehaviour
    {
        public Faction sourceFaction;
        public bool friendlyFire = false;

        public int damage = 10;
        public float speed = 10f;
        public float lifeTime = 5f;

        [Header("Impact Effects")]
        public GameObject impactEffect;
        public AudioClip impactSound;
        public float impactSoundVolume = 0.8f;

        private Vector3 direction;
        private ObjectPool projectilePool;
        private ObjectPool impactPool;
        private float lifetimeTimer;

        public void Init(
            Vector3 dir,
            Faction faction,
            bool allowFriendlyFire,
            int dmg,
            float spd,
            GameObject impactVFX = null,
            AudioClip impactSFX = null,
            float sfxVolume = 0.8f,
            ObjectPool sourcePool = null,
            ObjectPool vfxPool = null)
        {
            direction = dir.normalized;
            sourceFaction = faction;
            friendlyFire = allowFriendlyFire;
            damage = dmg;
            speed = spd;
            impactEffect = impactVFX;
            impactSound = impactSFX;
            impactSoundVolume = sfxVolume;
            projectilePool = sourcePool;
            impactPool = vfxPool;

            lifetimeTimer = 0f;
        }

        private void Update()
        {
            transform.position += direction * (speed * Time.deltaTime);

            lifetimeTimer += Time.deltaTime;
            if (lifetimeTimer >= lifeTime)
            {
                ReturnToPool();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var health = other.GetComponent<IHealth>();
            if (!FactionUtility.CanDamage(sourceFaction, health, friendlyFire)) return;

            int finalDamage = CombatCalculator.CalculateDamage(gameObject, other.gameObject);
            health.GetHealthSystem().Damage(finalDamage);

            // Spawn VFX
            if (impactEffect != null && impactPool != null)
            {
                GameObject vfx = impactPool.Get(transform.position, Quaternion.identity);
                vfx.GetComponent<ParticleSystem>()?.Play();
            }

            // Play SFX
            if (impactSound != null)
            {
                AudioSource.PlayClipAtPoint(impactSound, transform.position, impactSoundVolume);
            }

            ReturnToPool();
        }

        private void ReturnToPool()
        {
            if (projectilePool != null)
                projectilePool.Return(gameObject);
            else
                Destroy(gameObject);
        }
    }
}
