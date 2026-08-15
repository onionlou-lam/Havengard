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

        [Header("Class Hierarchy")]
        [Tooltip("If this is a specialization, reference the base class (e.g., Ranger) to inherit its abilities")]
        public PlayerClass baseClass;

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
        [Tooltip("Abilities specific to THIS class/specialization")]
        public ClassAbility[] classAbilities;

        [Header("Specializations (Optional)")]
        [Tooltip("If this is a main class, define 3 specializations here for the skill tree tabs")]
        public PlayerClass[] specializations = new PlayerClass[3];

        /// <summary>
        /// Get all abilities including inherited ones from base class
        /// </summary>
        public ClassAbility[] GetAllAbilities()
        {
            // If no base class, just return our abilities
            if (baseClass == null)
                return classAbilities ?? new ClassAbility[0];

            // Merge base class abilities with our own
            var baseAbilities = baseClass.classAbilities ?? new ClassAbility[0];
            var specAbilities = classAbilities ?? new ClassAbility[0];

            var combined = new ClassAbility[baseAbilities.Length + specAbilities.Length];
            
            // Copy base class abilities first
            System.Array.Copy(baseAbilities, 0, combined, 0, baseAbilities.Length);
            
            // Then add specialization abilities
            System.Array.Copy(specAbilities, 0, combined, baseAbilities.Length, specAbilities.Length);

            return combined;
        }

        /// <summary>
        /// Check if this is a specialization (has a base class)
        /// </summary>
        public bool IsSpecialization()
        {
            return baseClass != null;
        }

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