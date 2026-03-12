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
        public string heroName;
        public Sprite portrait;
        public Faction faction = Faction.Ally;

        [Header("Class Reference")]
        public PlayerClass heroClass;   // link to WarriorClass, MageClass, etc.

        [Header("Hero Overrides")]
        public List<AbilityBase> startingAbilities; // subset of class allowed abilities
        public string backstory;
        public bool overrideStats;
        public int overrideHP;
        public int overrideAttack;
        public int overrideDefense;
        public int overrideResource;
    }
}
