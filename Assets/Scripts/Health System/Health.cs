using Havengard.Combat;
using Havengard.HealthSystem;
using Havengard.Heroes;
using Havengard.Units;
using System; // for Action
using UnityEngine;

namespace Havengard.HealthSystem
{
    /// <summary>
    /// MonoBehaviour wrapper for HealthSystem.
    /// Used on all units (Player, Allies, Enemies).
    /// Handles auto-registration in UnitTargetManager.
    /// </summary>
    [DisallowMultipleComponent]
    public class Health : MonoBehaviour, IHealth
    {
        [Header("Health Settings")]
        [SerializeField] private int startingMaxHealth = 100;
        [SerializeField] private Faction faction = Faction.Neutral;

        private HealthSystem healthSystem;

        // Forwarded gameplay events
        public event Action OnDamaged;
        public event Action OnHealed;
        public event Action OnDeath;

        // Cached flag to prevent double registration
        private bool isRegistered;

        private void Awake()
        {
            // Initialize health system (using hero stats if present)
            var hero = GetComponent<HeroInstance>();
            int maxHP = hero != null ? hero.GetStats().MaxHP : startingMaxHealth;
            healthSystem = new HealthSystem(maxHP);

            // Forward events
            healthSystem.OnHealthChanged += () => OnDamaged?.Invoke();
            healthSystem.OnDeath += () => OnDeath?.Invoke();

            RegisterSelf();
        }

        private void OnEnable() => RegisterSelf();

        private void OnDisable() => UnregisterSelf();

        private void OnDestroy() => UnregisterSelf();

        private void RegisterSelf()
        {
            if (isRegistered) return;
            UnitTargetManager.Register(this);
            isRegistered = true;
        }

        private void UnregisterSelf()
        {
            if (!isRegistered) return;
            UnitTargetManager.Unregister(this);
            isRegistered = false;
        }
        public void Damage(int amount)
        {
            if (amount <= 0) return;
            healthSystem.Damage(amount);
            OnDamaged?.Invoke();
        }

        public void Heal(int amount)
        {
            if (amount <= 0) return;
            healthSystem.Heal(amount);
            OnHealed?.Invoke();
        }

        public HealthSystem GetHealthSystem() => healthSystem;
        public Faction GetFaction() => faction;
    }
}
