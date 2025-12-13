using System;                       // for Action
using UnityEngine;
using Havengard.Units;

namespace Havengard.HealthSystem
{
    /// <summary>
    /// MonoBehaviour wrapper for HealthSystem.
    /// Used on all units (Player, Heroes, Allies, Enemies).
    /// </summary>
    [DisallowMultipleComponent]
    public class Health : MonoBehaviour, IHealth
    {
        [Header("Config")]
        [Tooltip("Fallback max health if no stats component is present.")]
        [SerializeField] private int startingMaxHealth = 100;

        [Tooltip("Faction for damage-filtering (Player, Ally, Enemy, Neutral, etc.).")]
        [SerializeField] private Faction faction = Faction.Neutral;

        private HealthSystem healthSystem;

        // Forwarded events (UI and gameplay can subscribe here)
        public event Action OnDamaged;
        public event Action OnHealed;
        public event Action OnDeath;

        private void Awake()
        {
            try
            {
                // Try to pull MaxHP from a StatsComponent if one exists,
                // otherwise fall back to startingMaxHealth.
                int maxHP = startingMaxHealth;

                var statsComponent = GetComponent<Character.StatsComponent>();
                if (statsComponent != null && statsComponent.CurrentStats.MaxHP > 0)
                {
                    maxHP = statsComponent.CurrentStats.MaxHP;
                }

                healthSystem = new HealthSystem(maxHP);

                // Forward HealthSystem events
                healthSystem.OnHealthChanged += HandleHealthChanged;
                healthSystem.OnDeath += HandleDeath;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Health] Error during Awake on {name}: {ex.Message}\n{ex.StackTrace}");
                // Do NOT disable the component – leave it enabled so you can see errors.
            }
        }

        private void OnDestroy()
        {
            if (healthSystem == null) return;

            healthSystem.OnHealthChanged -= HandleHealthChanged;
            healthSystem.OnDeath -= HandleDeath;
            Debug.Log("HandleDeath OnDestroy called");
        }

        // -------- Event forwarding --------

        private void HandleHealthChanged()
        {
            OnDamaged?.Invoke();
            OnHealed?.Invoke(); // you can split these out later if you want
        }

        private void HandleDeath()
        {
            OnDeath?.Invoke();
            Debug.Log("HandleDeath called");
        }

        // -------- IHealth implementation --------

        public HealthSystem GetHealthSystem() => healthSystem;

        public Faction GetFaction() => faction;

        // Optional helper: allows other systems to override faction at runtime
        public void SetFaction(Faction newFaction)
        {
            faction = newFaction;
        }

        // Optional helper: allows other systems to override starting max HP before Awake runs
        public void SetStartingMaxHealth(int value)
        {
            startingMaxHealth = Mathf.Max(1, value);
        }
    }
}
