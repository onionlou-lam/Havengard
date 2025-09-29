using UnityEngine;
using System;

namespace Havengard.Progression
{
    public class ExperienceSystem : MonoBehaviour
    {
        public int CurrentEXP { get; private set; }
        public int Level { get; private set; } = 1;

        private int[] expToLevel;

        public event Action<int> OnLevelUp; // passes new level

        public void InitEXPTable(int[] expTable)
        {
            expToLevel = expTable;
        }

        public void AddEXP(int amount)
        {
            CurrentEXP += amount;

            while (Level - 1 < expToLevel.Length && CurrentEXP >= expToLevel[Level - 1])
            {
                CurrentEXP -= expToLevel[Level - 1];
                Level++;
                OnLevelUp?.Invoke(Level);
                Debug.Log($"Level up! New level: {Level}");
            }
        }
    }
}
