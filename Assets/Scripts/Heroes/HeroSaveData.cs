using System;
using UnityEngine;

namespace Havengard.Heroes
{
    [Serializable]
    public struct HeroSaveData
    {
        public string heroName;
        public string className;
        public int level;
        public int currentEXP;

        public string[] abilityNames;
        public string[] traitNames;

        public int currentHP;
        public int currentResource;

        public string[] equippedItemNames;   // equipment
        public string[] inventoryItemNames;  // backpack / carried items

        public Vector3 position;
    }
}
