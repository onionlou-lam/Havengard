using Havengard.Abilities;
using Havengard.Heroes;
using Havengard.Progression;
using UnityEngine;

namespace Havengard.Heroes
{
    [CreateAssetMenu(menuName = "Havengard/Hero")]
    public class HeroData : ScriptableObject
    {
        [Header("Identity")]
        public string heroName;
        [TextArea] public string backstory;
        public Sprite portrait;

        [Header("Class & Abilities")]
        public PlayerClass playerClass;
        public AbilityBase[] startingAbilities;
        public string subclass; // e.g. "Fire Mage"

        [Header("Base Stats")]
        public int baseHP = 100;
        public int baseAttack = 10;
        public int baseDefense = 5;
        public int baseResource = 50;

        [Header("Traits")]
        public Trait[] passiveTraits;
    }
}
