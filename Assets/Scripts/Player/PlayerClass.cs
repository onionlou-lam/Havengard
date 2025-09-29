using UnityEngine;
using Havengard.Abilities;

namespace Havengard.Progression
{
    [CreateAssetMenu(menuName = "Havengard/Classes/Player Class")]
    public class PlayerClass : ScriptableObject
    {
        [Header("Class Info")]
        public string className;
        public Sprite classIcon;

        [Header("Base Stats")]
        public int baseHP = 100;
        public int baseAttack = 10;
        public int baseDefense = 5;
        public int baseResource = 50;
        public float baseAttackSpeed = 1f;
        public float baseMoveSpeed = 3f;

        [Header("Crit Stats")]
        [Range(0f, 1f)] public float baseCritChance = 0.1f;
        public float baseCritMultiplier = 2f;

        [Header("Progression")]
        public float baseRollCooldown = 3f;
        public int[] expToLevel = new int[] { 100, 250, 500, 1000 };

        [Header("Per Level Growth")]
        public int hpGrowth = 10;
        public int attackGrowth = 2;
        public int defenseGrowth = 1;
        public int resourceGrowth = 5;

        [Header("Class Abilities")]
        public ClassAbility[] classAbilities; // includes requiredLevel
    }
}
