using System;
using UnityEngine;

namespace Havengard.Abilities
{
    [Serializable]
    public class ClassAbility
    {
        [Header("Ability Reference")]
        [Tooltip("The ability this node unlocks")]
        public AbilityBase ability;

        [Tooltip("Level required to unlock this ability")]
        public int requiredLevel = 1;

        [Header("Skill Tree")]
        [Tooltip("Indices of abilities that must be unlocked first (leave empty for starting abilities)")]
        public int[] prerequisiteIndices = new int[0];

        [Tooltip("Position in skill tree UI grid (x=column, y=row)")]
        public Vector2 treePosition = Vector2.zero;

        [Tooltip("Skill points required to unlock")]
        public int skillPointCost = 1;

        [Header("UI Display")]
        [Tooltip("Optional description override (uses ability description if empty)")]
        [TextArea(2, 4)]
        public string customDescription = "";

        /// <summary>
        /// Get the description to display in UI
        /// </summary>
        public string GetDescription()
        {
            if (!string.IsNullOrEmpty(customDescription))
                return customDescription;

            if (ability != null)
                return ability.description;

            return "No description available.";
        }

        /// <summary>
        /// Check if this ability has prerequisites
        /// </summary>
        public bool HasPrerequisites()
        {
            return prerequisiteIndices != null && prerequisiteIndices.Length > 0;
        }

        /// <summary>
        /// Check if prerequisites are met based on unlocked abilities
        /// </summary>
        public bool ArePrerequisitesMet(bool[] unlockedAbilities)
        {
            if (!HasPrerequisites())
                return true;

            foreach (int prereqIndex in prerequisiteIndices)
            {
                if (prereqIndex < 0 || prereqIndex >= unlockedAbilities.Length)
                    continue;

                if (!unlockedAbilities[prereqIndex])
                    return false;
            }

            return true;
        }
    }
}