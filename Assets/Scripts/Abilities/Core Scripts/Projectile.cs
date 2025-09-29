using UnityEngine;
using Havengard.HealthSystem;
using Havengard.Units;
using Havengard.Combat;

namespace Havengard.Abilities
{
    public class Projectile : MonoBehaviour
    {
        public Faction sourceFaction;
        public bool friendlyFire = false;

        public int damage = 10;
        public float speed = 10f;
        public float lifeTime = 5f;

        private Vector3 direction;

        public void Init(Vector3 dir, Faction faction, bool allowFriendlyFire, int dmg, float spd)
        {
            direction = dir.normalized;
            sourceFaction = faction;
            friendlyFire = allowFriendlyFire;
            damage = dmg;
            speed = spd;

            Destroy(gameObject, lifeTime);
        }

        private void Update()
        {
            transform.position += direction * (speed * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var health = other.GetComponent<IHealth>();
            if (!FactionUtility.CanDamage(sourceFaction, health, friendlyFire)) return;

            // Apply defense reduction
            int finalDamage = CombatCalculator.CalculateDamage(gameObject, other.gameObject);
            health.GetHealthSystem().Damage(finalDamage);

            Destroy(gameObject);
        }
    }
}
