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

        [Header("Sub-Skills (Direct References)")]
        [Tooltip("Sub-skill options for this ability - shown as child nodes in skill tree")]
        public SubSkillNodeData[] subSkills = new SubSkillNodeData[0];

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

        /// <summary>
        /// Check if this ability has sub-skill options
        /// </summary>
        public bool HasSubSkills()
        {
            return subSkills != null && subSkills.Length > 0;
        }

        /// <summary>
        /// Get number of valid sub-skills
        /// </summary>
        public int GetSubSkillCount()
        {
            if (!HasSubSkills())
                return 0;

            int count = 0;
            foreach (var subSkill in subSkills)
            {
                if (subSkill != null && subSkill.IsValid())
                    count++;
            }
            return count;
        }

        /// <summary>
        /// Get a specific sub-skill by index
        /// </summary>
        public SubSkillNodeData GetSubSkill(int index)
        {
            if (subSkills == null || index < 0 || index >= subSkills.Length)
                return null;

            return subSkills[index];
        }
    }
}