using UnityEngine;
using Havengard.Abilities;

namespace Havengard.Core.Progression
{
    /// <summary>
    /// Defines a player class with base stats, progression, and skill tree specializations.
    /// Can represent a main class (e.g., Mage) or a specialization (e.g., Arcane Mage).
    /// </summary>
    [CreateAssetMenu(menuName = "Havengard/Classes/Player Class")]
    public class PlayerClass : ScriptableObject
    {
        [Header("Class Info")]
        public string className;
        [Tooltip("For skill tree tabs - e.g., 'Arcane', 'Fire', 'Frost' (leave empty if not a specialization)")]
        public string specializationName;
        public Sprite classIcon;

        [Header("Base Stats")]
        public int baseHP = 100;
        public int baseAttack = 10;
        public int baseDefense = 5;
        public int baseResource = 50;
        public float baseAttackSpeed = 1f;
        public float baseMoveSpeed = 3f;

        [Header("Crit Stats")]
        [Range(0f, 1f)] public float baseCritChance = 0.1f;
        public float baseCritMultiplier = 2f;

        [Header("Progression")]
        public float baseRollCooldown = 3f;
        public int[] expToLevel = new int[] { 100, 250, 500, 1000 };

        [Header("Per Level Growth")]
        public int hpGrowth = 10;
        public int attackGrowth = 2;
        public int defenseGrowth = 1;
        public int resourceGrowth = 5;

        [Header("Class Abilities / Skill Tree")]
        [Tooltip("All abilities available to this class/specialization in skill tree order")]
        public ClassAbility[] classAbilities;

        [Header("Specializations (Optional)")]
        [Tooltip("If this is a main class, define 3 specializations here for the skill tree tabs")]
        public PlayerClass[] specializations = new PlayerClass[3];

        /// <summary>
        /// Check if this class has specializations defined
        /// </summary>
        public bool HasSpecializations()
        {
            return specializations != null &&
                   specializations.Length >= 3 &&
                   specializations[0] != null &&
                   specializations[1] != null &&
                   specializations[2] != null;
        }

        /// <summary>
        /// Get a specific specialization by index (0-2)
        /// </summary>
        public PlayerClass GetSpecialization(int index)
        {
            if (specializations == null || index < 0 || index >= specializations.Length)
                return null;

            return specializations[index];
        }

        /// <summary>
        /// Get the display name for skill tree tab
        /// </summary>
        public string GetTabName()
        {
            return !string.IsNullOrEmpty(specializationName) ? specializationName : className;
        }
    }
}