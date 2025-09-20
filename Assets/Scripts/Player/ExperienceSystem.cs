using UnityEngine;
using System;

namespace Havengard.Progression
{
    public class ExperienceSystem : MonoBehaviour
    {
        [SerializeField] private PlayerClass playerClass;
        public PlayerClass ClassData => playerClass;

        public int Level { get; private set; } = 1;
        public int CurrentEXP { get; private set; }

        public event Action<int> OnLevelUp;
        public event Action<int> OnExpGained;

        public void SetClass(PlayerClass newClass)
        {
            playerClass = newClass;
            Level = 1;
            CurrentEXP = 0;
        }

        public void AddEXP(int amount)
        {
            CurrentEXP += amount;
            OnExpGained?.Invoke(amount);

            while (Level - 1 < playerClass.expToLevel.Length &&
                   CurrentEXP >= playerClass.expToLevel[Level - 1])
            {
                CurrentEXP -= playerClass.expToLevel[Level - 1];
                Level++;
                OnLevelUp?.Invoke(Level);
            }
        }
    }
}
