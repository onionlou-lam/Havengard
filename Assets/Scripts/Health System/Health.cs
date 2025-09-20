using System;                       // <-- needed for Action
using UnityEngine;
using Havengard.HealthSystem;
using Havengard.Units;
using Havengard.Heroes;

namespace Havengard.HealthSystem
{
    /// <summary>
    /// MonoBehaviour wrapper for HealthSystem.
    /// Used on all units (Player, Heroes, Allies, Enemies).
    /// </summary>
    [DisallowMultipleComponent]
    public class Health : MonoBehaviour, IHealth
    {
        [SerializeField] private int startingMaxHealth = 100;
        [SerializeField] private Faction faction = Faction.Neutral;

        private HealthSystem healthSystem;

        // Forwarded events (UI and gameplay can subscribe here)
        public event Action OnDamaged;
        public event Action OnHealed;
        public event Action OnDeath;

        private void Awake()
        {
            // If this object has a HeroInstance, use its stats for max HP
            var hero = GetComponent<HeroInstance>();
            if (hero != null)
            {
                int maxHP = hero.GetStats().MaxHP;
                healthSystem = new HealthSystem(maxHP);
            }
            else
            {
                healthSystem = new HealthSystem(startingMaxHealth);
            }

            // Forward HealthSystem events to local events
            healthSystem.OnHealthChanged += () => OnDamaged?.Invoke();
            healthSystem.OnDeath += () => OnDeath?.Invoke();
        }

        public HealthSystem GetHealthSystem() => healthSystem;
        public Faction GetFaction() => faction;
    }
}
