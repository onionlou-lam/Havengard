using Havengard.Abilities;
using UnityEngine;

namespace Havengard.Progression
{
    [CreateAssetMenu(menuName = "Havengard/Classes/Player Class")]
    public class PlayerClass : ScriptableObject
    {
        [Header("Class Info")]
        public string className;
        public Sprite classIcon;

        [Header("Progression")]
        public float baseRollCooldown = 3f;
        public int[] expToLevel = new int[] { 100, 250, 500, 1000 };
        // array for required EXP per level

        [Header("Allowed Abilities")]
        public AbilityBase[] allowedAbilities;
    }
}
