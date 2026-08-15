using UnityEngine;

namespace Havengard.Core.Character
{
    [DisallowMultipleComponent]
    public class StatsComponent : MonoBehaviour
    {
        [Header("Base stats (authoring)")]
        [SerializeField] private HeroStats baseStats = new HeroStats();

        public HeroStats BaseStats => baseStats;

        // Runtime stats (mutated by buffs/debuffs)
        public HeroStats CurrentStats { get; private set; }

        private void Awake()
        {
            // Always start runtime stats from base
            CurrentStats = baseStats != null ? baseStats.Clone() : new HeroStats();
        }

        /// <summary>Hard overwrite runtime stats.</summary>
        public void SetCurrentStats(HeroStats newStats)
        {
            CurrentStats = newStats != null ? newStats.Clone() : new HeroStats();
        }

        /// <summary>Alias for older code paths.</summary>
        public void SetStats(HeroStats newStats) => SetCurrentStats(newStats);

        public HeroStats GetCurrentStatsClone() => CurrentStats != null ? CurrentStats.Clone() : new HeroStats();
        public HeroStats GetBaseStatsClone() => baseStats != null ? baseStats.Clone() : new HeroStats();

        /// <summary>
        /// Apply stat modifiers (for buffs, items, etc.)
        /// </summary>
        public void ApplyModifiers(HeroStats modifiers, bool isMultiplicative = false)
        {
            if (CurrentStats == null) return;
            CurrentStats.ApplyModifiers(modifiers, isMultiplicative);
        }

        /// <summary>
        /// Reset current stats back to base stats
        /// </summary>
        public void ResetToBaseStats()
        {
            CurrentStats = baseStats != null ? baseStats.Clone() : new HeroStats();
        }

        /// <summary>
        /// Sync max health/resource with attached systems
        /// </summary>
        public void SyncWithSystems()
        {
            if (CurrentStats == null) return;

            // Sync health
            var health = GetComponent<Havengard.Core.HealthSystem.Health>();
            if (health != null)
            {
                health.SetMaxHealthFromStats(refill: false);
            }

            // Sync resource
            var resourceSystem = GetComponent<Havengard.Abilities.ResourceSystem>();
            if (resourceSystem != null)
            {
                resourceSystem.SyncFromStats();
            }
        }
    }
}