using System;

namespace Havengard.Save
{
    /// <summary>
    /// Serializable data for a single hero
    /// </summary>
    [Serializable]
    public class HeroSaveData
    {
        public string heroDataName;           // Name of HeroData asset
        public int level;                     // Current level
        public int currentExp;                // Current EXP
        public int[] unlockedAbilityIndices;  // Indices into PlayerClass.classAbilities

        // Health/Resource state
        public int currentHealth;
        public int currentResource;

        // Assignment state (for future phases)
        public string assignmentType;         // "None", "Quest", "Defence", "Training"
        public string assignedLocation;       // "Warrior Guild", "North Gate", etc.

        // Quest state
        public bool isOnQuest;
        public int questDaysRemaining;

        public HeroSaveData() { }
    }
}