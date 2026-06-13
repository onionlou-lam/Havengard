using Havengard.Abilities;
using Havengard.Core.Progression;
using Havengard.Units;
using System.Collections.Generic;
using UnityEngine;

namespace Havengard.Core.Heroes
{
    [CreateAssetMenu(menuName = "Havengard/Hero Data")]
    public class HeroData : ScriptableObject
    {
        [Header("Identity")]
        public string heroName;
        public Sprite portrait;
        public Faction faction = Faction.Ally;

        [Header("Class Reference")]
        public PlayerClass heroClass;   // Link to WarriorClass, MageClass, etc.

        [Header("Starting Abilities - DEPRECATED")]
        [Tooltip("OLD SYSTEM - Use startingUnlockedIndices instead")]
        public List<AbilityBase> startingAbilities; // Keep for backward compatibility

        [Header("Starting Abilities - NEW SYSTEM")]
        [Tooltip("Indices of abilities from heroClass.classAbilities that are unlocked at start")]
        public int[] startingUnlockedIndices = new int[0]; // NEW!

        [Tooltip("Starting skill points (in addition to level-based points)")]
        public int bonusStartingSkillPoints = 0; // NEW!

        [Header("Hero Overrides")]
        public string backstory;
        public bool overrideStats;
        public int overrideHP;
        public int overrideAttack;
        public int overrideDefense;
        public int overrideResource;
    }
}