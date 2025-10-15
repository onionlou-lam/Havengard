using UnityEngine;
using Havengard.HealthSystem;
using Havengard.Units;
using Havengard.Combat;

namespace Havengard.Abilities
{
    public class WallOfFireZone : MonoBehaviour
    {
        public Faction sourceFaction;
        public bool friendlyFire;
        public int baseDamage;

        [Header("DoT Settings")]
        [SerializeField] private float tickInterval = 1f;
        [SerializeField] private int dotDamage = 5;
        [SerializeField] private float dotDuration = 3f;

        public void Init(Faction faction, bool allowFriendlyFire, int dmg)
        {
            sourceFaction = faction;
            friendlyFire = allowFriendlyFire;
            baseDamage = dmg;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var health = other.GetComponent<IHealth>();
            if (health == null) return;
            if (!FactionUtility.CanDamage(sourceFaction, health, friendlyFire)) return;

            // Initial burst
            int hitDamage = CombatCalculator.CalculateDamage(gameObject, other.gameObject);
            health.GetHealthSystem().Damage(hitDamage);

            // Apply stacking DoT
            DamageOverTimeEffect.ApplyTo(other.gameObject, dotDamage, tickInterval, dotDuration);
        }
    }
}
