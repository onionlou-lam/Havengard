using System;

namespace Havengard.Abilities
{
    /// <summary>
    /// Tracks which sub-skill was selected for a given ability
    /// </summary>
    [Serializable]
    public struct SubSkillSelection
    {
        public int abilityIndex; // Index in classAbilities array
        public int subSkillIndex; // Which sub-skill option was chosen (0, 1, 2, etc.) -1 = none

        public SubSkillSelection(int abilityIdx)
        {
            abilityIndex = abilityIdx;
            subSkillIndex = -1;
        }

        public bool HasSelection()
        {
            return subSkillIndex >= 0;
        }

        public bool IsSelected(int subIdx)
        {
            return subSkillIndex == subIdx;
        }
    }
}