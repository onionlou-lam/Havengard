using UnityEngine;
using Havengard.Units;
using System;

namespace Havengard.Core.HealthSystem
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

        // IHealth events
        public event Action<int> OnDamaged;
        public event Action<int> OnHealed;
        public event Action<int, int> OnHealthChanged;
        public event Action OnDeath;

        public int CurrentHealth => EnsureAndGet().CurrentHealth;
        public int MaxHealth => EnsureAndGet().MaxHealth;
        public bool IsDead => EnsureAndGet().IsDead;

        private void Awake()
        {
            EnsureInitialized();
        }

        private void OnDestroy()
        {
            Unhook();
        }

        private HealthSystem EnsureAndGet()
        {
            EnsureInitialized();
            return healthSystem;
        }

        private void EnsureInitialized()
        {
            if (healthSystem != null) return;

            int maxHP = Mathf.Max(1, startingMaxHealth);

            var statsComponent = GetComponent<Havengard.Core.Character.StatsComponent>();
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

            healthSystem.OnDamaged += HandleDamaged;
            healthSystem.OnHealed += HandleHealed;
            healthSystem.OnHealthChanged += HandleHealthChanged;
            healthSystem.OnDeath += HandleDeath;

            hooked = true;
        }

        private void Unhook()
        {
            if (!hooked || healthSystem == null) return;

            healthSystem.OnDamaged -= HandleDamaged;
            healthSystem.OnHealed -= HandleHealed;
            healthSystem.OnHealthChanged -= HandleHealthChanged;
            healthSystem.OnDeath -= HandleDeath;

            hooked = false;
        }

        private void HandleDamaged(int amount) => OnDamaged?.Invoke(amount);
        private void HandleHealed(int amount) => OnHealed?.Invoke(amount);
        private void HandleHealthChanged(int current, int max) => OnHealthChanged?.Invoke(current, max);
        private void HandleDeath() => OnDeath?.Invoke();

        // -------- IHealth --------
        public HealthSystem GetHealthSystem()
        {
            EnsureInitialized();
            return healthSystem;
        }

        public bool TryGetHealthSystem(out HealthSystem system)
        {
            EnsureInitialized();
            system = healthSystem;
            return system != null;
        }

        public Faction GetFaction() => faction;

        // -------- Convenience API --------
        public int Damage(int amount) => EnsureAndGet().Damage(amount);
        public int Heal(int amount) => EnsureAndGet().Heal(amount);

        // ADD THESE COMPATIBILITY METHODS for ability system
        public void TakeDamage(int amount, GameObject source)
        {
            Damage(amount);
        }

        public void TakeDamage(int amount)
        {
            Damage(amount);
        }

        // -------- helpers --------
        public void SetFaction(Faction newFaction) => faction = newFaction;

        public void SetStartingMaxHealth(int value) => startingMaxHealth = Mathf.Max(1, value);

        public void SetMaxHealthFromStats(bool refill = true)
        {
            var statsComponent = GetComponent<Havengard.Core.Character.StatsComponent>();
            if (statsComponent == null || statsComponent.CurrentStats == null) return;

            int newMax = Mathf.Max(1, statsComponent.CurrentStats.MaxHP);

            EnsureInitialized();
            healthSystem.SetMaxHealth(newMax, refill);
        }
        
        // ADD THIS METHOD FOR SAVE SYSTEM
        /// <summary>
        /// Set health directly (for loading saves)
        /// </summary>
        public void SetHealth(int amount)
        {
            EnsureInitialized();
            
            int newHealth = Mathf.Clamp(amount, 0, healthSystem.MaxHealth);
            
            // Use internal HealthSystem to set health directly
            // Since HealthSystem doesn't expose a direct setter, we'll heal to target
            int currentHealth = healthSystem.CurrentHealth;
            
            if (newHealth > currentHealth)
            {
                healthSystem.Heal(newHealth - currentHealth);
            }
            else if (newHealth < currentHealth)
            {
                healthSystem.Damage(currentHealth - newHealth);
            }
            
            Debug.Log($"[Health] Health set to: {newHealth}/{healthSystem.MaxHealth}");
        }
    }
}