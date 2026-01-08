using UnityEngine;

namespace Havengard.Character
{
    [System.Serializable]
    public class Stats
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

        public Stats Clone()
        {
            return new Stats
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
        [SerializeField] private Stats baseStats = new Stats();

        public Stats BaseStats => baseStats;

        // Runtime stats (mutated by buffs/debuffs)
        public Stats CurrentStats { get; private set; }

        private void Awake()
        {
            // Always start runtime stats from base
            CurrentStats = baseStats != null ? baseStats.Clone() : new Stats();
        }

        /// <summary>Hard overwrite runtime stats.</summary>
        public void SetCurrentStats(Stats newStats)
        {
            CurrentStats = newStats != null ? newStats.Clone() : new Stats();
        }

        /// <summary>Alias for older code paths.</summary>
        public void SetStats(Stats newStats) => SetCurrentStats(newStats);

        public Stats GetCurrentStatsClone() => CurrentStats != null ? CurrentStats.Clone() : new Stats();
        public Stats GetBaseStatsClone() => baseStats != null ? baseStats.Clone() : new Stats();
    }
}
