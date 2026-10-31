using Havengard.Abilities;
using Havengard.Core.Progression;
using Havengard.Units;
using System.Collections.Generic;
using UnityEngine;
using Havengard.Dialogue;

namespace Havengard.Core.Heroes
{
    [CreateAssetMenu(menuName = "Havengard/Hero Data")]
    public class HeroData : ScriptableObject
    {
        [Header("Identity")]
        public string heroName;
        public Sprite portrait;
        public Faction faction = Faction.Ally;

        [Header("Dialogue")]
        [Tooltip("This hero's spoken lines. Optional. Yarn fallbacks are used when empty.")]
        public HeroDialogueProfile dialogueProfile;

        [Header("Class Reference")]
        public PlayerClass heroClass;   // Link to WarriorClass, MageClass, etc.

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

        public Sprite idleSprite;
    }
}