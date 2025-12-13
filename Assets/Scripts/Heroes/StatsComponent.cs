using UnityEngine;

namespace Havengard.Character
{
    /// <summary>
    /// Reference-type stats container so we can safely mutate fields via a property.
    /// </summary>
    [System.Serializable]
    public class Stats
    {
        public int MaxHP;
        public int Attack;
        public int Defense;
        public int MaxResource;

        public float AttackSpeed;
        public float MoveSpeed;

        public float CritChance;
        public float CritMultiplier;

        public Stats Clone()
        {
            return (Stats)MemberwiseClone();
        }
    }

    [DisallowMultipleComponent]
    public class StatsComponent : MonoBehaviour
    {
        [Header("Base Stats (optional)")]
        public Stats baseStats = new Stats();

        /// <summary>
        /// Runtime stats used by combat/abilities.
        /// </summary>
        public Stats CurrentStats { get; private set; }

        private void Awake()
        {
            if (baseStats == null)
                baseStats = new Stats();

            // Work on a cloned instance so we don't mutate the asset reference.
            CurrentStats = baseStats.Clone();
        }

        /// <summary>
        /// If you ever want to replace the entire stats object at runtime.
        /// </summary>
        public void SetCurrentStats(Stats newStats)
        {
            CurrentStats = newStats ?? baseStats.Clone();
        }
    }
}
