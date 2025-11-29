using UnityEngine;

namespace Havengard.Character
{
    [System.Serializable]
    public class Stats
    {
        public int MaxHP;
        public int Attack;
        public int Defense;
        public int MaxResource;
        public float AttackSpeed;
        public float MoveSpeed;

        [Header("Combat Extensions")]
        public float CritChance;     // e.g. 0.1 = 10%
        public float CritMultiplier; // e.g. 2.0 = double damage
    }

    public class StatsComponent : MonoBehaviour
    {
        [SerializeField] private Stats baseStats = new Stats();
        public Stats CurrentStats { get; private set; }

        private void Awake()
        {
            ResetStats();
        }

        /// <summary>
        /// Copies base stats into CurrentStats (used on spawn/init).
        /// </summary>
        public void ResetStats()
        {
            CurrentStats = new Stats
            {
                MaxHP = baseStats.MaxHP,
                Attack = baseStats.Attack,
                Defense = baseStats.Defense,
                MaxResource = baseStats.MaxResource,
                AttackSpeed = baseStats.AttackSpeed,
                MoveSpeed = baseStats.MoveSpeed,
                CritChance = baseStats.CritChance,
                CritMultiplier = baseStats.CritMultiplier
            };
        }

        /// <summary>
        /// Apply modifiers (e.g. from items, buffs, leveling).
        /// </summary>
        public void ApplyModifiers(int hpMod, int atkMod, int defMod, int resMod,
                                   float critChanceMod = 0f, float critMultMod = 0f)
        {
            CurrentStats.MaxHP += hpMod;
            CurrentStats.Attack += atkMod;
            CurrentStats.Defense += defMod;
            CurrentStats.MaxResource += resMod;
            CurrentStats.CritChance += critChanceMod;
            CurrentStats.CritMultiplier += critMultMod;
        }
    }
}
