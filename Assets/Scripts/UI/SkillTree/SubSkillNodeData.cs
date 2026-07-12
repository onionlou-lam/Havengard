using System;
using UnityEngine;
using Havengard.Abilities;

namespace Havengard.Abilities
{
    /// <summary>
    /// Represents a sub-skill node in the skill tree with direct references (no indices!)
    /// </summary>
    [Serializable]
    public class SubSkillNodeData
    {
        [Header("Sub-Skill Reference")]
        [Tooltip("The sub-skill modifier ScriptableObject")]
        public AbilitySubSkill subSkillModifier;

        [Header("Skill Tree Settings")]
        [Tooltip("Position offset from parent node (x=column, y=row)")]
        public Vector2 positionOffset = new Vector2(1f, 0f);

        [Tooltip("Level required to unlock this sub-skill")]
        public int requiredLevel = 1;

        [Tooltip("Skill points required to unlock")]
        public int skillPointCost = 1;

        [Header("UI Display")]
        [Tooltip("Optional custom icon (uses sub-skill icon if empty)")]
        public Sprite customIcon;

        [Tooltip("Optional description override")]
        [TextArea(2, 3)]
        public string customDescription = "";

        /// <summary>
        /// Get display icon for this sub-skill node
        /// </summary>
        public Sprite GetIcon()
        {
            if (customIcon != null)
                return customIcon;

            if (subSkillModifier != null && subSkillModifier.icon != null)
                return subSkillModifier.icon;

            return null;
        }

        /// <summary>
        /// Get display name for this sub-skill node
        /// </summary>
        public string GetName()
        {
            return subSkillModifier != null ? subSkillModifier.subSkillName : "Unknown Sub-Skill";
        }

        /// <summary>
        /// Get description for this sub-skill node
        /// </summary>
        public string GetDescription()
        {
            if (!string.IsNullOrEmpty(customDescription))
                return customDescription;

            if (subSkillModifier != null)
                return subSkillModifier.GetTooltipText();

            return "No description available.";
        }

        /// <summary>
        /// Validate this sub-skill data
        /// </summary>
        public bool IsValid()
        {
            return subSkillModifier != null;
        }
    }
}