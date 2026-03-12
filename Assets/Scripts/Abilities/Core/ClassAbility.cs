using System;
using UnityEngine;

namespace Havengard.Abilities
{
    [Serializable]
    public class ClassAbility
    {
        public AbilityBase ability;
        public int requiredLevel = 1; // default unlocked at level 1
    }
}
