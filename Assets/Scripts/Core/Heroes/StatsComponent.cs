using UnityEngine;

namespace Havengard.Core.Character
{
    [System.Serializable]
    public class HeroStats
    {
        public int MaxHP = 100;

        public int Attack = 10;
        public int Defense = 5;

        public float AttackSpeed = 1f;
        public float MoveSpeed = 5f;

        public float CritChance = 0.05f;
        public float CritMultiplier = 1.5f;

        // ? Resource (mana) support
        public int MaxResource = 50;

        public HeroStats Clone()
        {
            return new HeroStats
            {
                MaxHP = MaxHP,
                Attack = Attack,
                Defense = Defense,
                AttackSpeed = AttackSpeed,
                MoveSpeed = MoveSpeed,
                CritChance = CritChance,
                CritMultiplier = CritMultiplier,
                MaxResource = MaxResource
            };
        }
    }

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
    }
}
