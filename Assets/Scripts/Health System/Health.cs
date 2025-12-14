using System;
using UnityEngine;
using Havengard.Units;

namespace Havengard.HealthSystem
{
    [DisallowMultipleComponent]
    public class Health : MonoBehaviour, IHealth
    {
        [Header("Config")]
        [Tooltip("Fallback max health if no stats component is present or not initialised yet.")]
        [SerializeField] private int startingMaxHealth = 100;

        [Tooltip("Faction for damage-filtering (Player, Ally, Enemy, Neutral, etc.).")]
        [SerializeField] private Faction faction = Faction.Neutral;

        private HealthSystem healthSystem;
        private bool hooked;

        public event Action OnDamaged;
        public event Action OnHealed;
        public event Action OnDeath;

        private void Awake()
        {
            // Don’t rely on script execution order. Create system if needed.
            EnsureInitialized();
        }

        private void OnDestroy()
        {
            Unhook();
        }

        private void EnsureInitialized()
        {
            if (healthSystem != null) return;

            int maxHP = Mathf.Max(1, startingMaxHealth);

            // Pull from stats if available (and already initialised)
            var statsComponent = GetComponent<Havengard.Character.StatsComponent>();
            if (statsComponent != null && statsComponent.CurrentStats != null && statsComponent.CurrentStats.MaxHP > 0)
            {
                maxHP = statsComponent.CurrentStats.MaxHP;
            }

            healthSystem = new HealthSystem(maxHP);
            Hook();
        }

        private void Hook()
        {
            if (hooked || healthSystem == null) return;
            healthSystem.OnHealthChanged += HandleHealthChanged;
            healthSystem.OnDeath += HandleDeath;
            hooked = true;
        }

        private void Unhook()
        {
            if (!hooked || healthSystem == null) return;
            healthSystem.OnHealthChanged -= HandleHealthChanged;
            healthSystem.OnDeath -= HandleDeath;
            hooked = false;
        }

        private void HandleHealthChanged()
        {
            // For UI this is fine. You can split damaged/healed later.
            OnDamaged?.Invoke();
            OnHealed?.Invoke();
        }

        private void HandleDeath()
        {
            OnDeath?.Invoke();
        }

        // -------- IHealth --------
        public HealthSystem GetHealthSystem()
        {
            EnsureInitialized();
            return healthSystem;
        }

        public Faction GetFaction() => faction;

        // -------- helpers --------
        public void SetFaction(Faction newFaction) => faction = newFaction;

        public void SetStartingMaxHealth(int value) => startingMaxHealth = Mathf.Max(1, value);

        /// <summary>
        /// Call after stats are updated (e.g., HeroInstance init / level up).
        /// </summary>
        public void SetMaxHealthFromStats(bool refill = true)
        {
            var statsComponent = GetComponent<Havengard.Character.StatsComponent>();
            if (statsComponent == null || statsComponent.CurrentStats == null) return;

            int newMax = Mathf.Max(1, statsComponent.CurrentStats.MaxHP);

            EnsureInitialized();
            healthSystem.SetMaxHealth(newMax, refill);
        }
    }
}
