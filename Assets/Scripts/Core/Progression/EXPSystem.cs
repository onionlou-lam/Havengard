using UnityEngine;
using System;

namespace Havengard.Core.Progression
{
    public class EXPSystem : MonoBehaviour
    {
        public int CurrentEXP { get; private set; }
        public int Level { get; private set; } = 1;

        private int[] expToLevel;

        public event Action OnExpChanged;
        public event Action<int> OnLevelUp;

        public int CurrentExp => CurrentEXP;
        public int CurrentLevel => Level;

        public int ExpToNextLevel
        {
            get
            {
                if (expToLevel == null || expToLevel.Length == 0)
                    return 1;

                int index = Level - 1;
                if (index < 0 || index >= expToLevel.Length)
                    return expToLevel[expToLevel.Length - 1];

                return expToLevel[index];
            }
        }

        private void Awake()
        {
            Debug.Log($"[ExpSystem] Awake on {name}");
        }

        public void InitEXPTable(int[] expTable)
        {
            expToLevel = expTable;

            if (expToLevel == null || expToLevel.Length == 0)
            {
                Debug.LogWarning($"[ExpSystem] InitEXPTable on {name} called with null/empty array.");
            }
            else
            {
                Debug.Log($"[ExpSystem] EXP table initialised on {name}. Length={expToLevel.Length}, first={expToLevel[0]}");
            }

            RaiseChanged();
        }

        public void AddEXP(int amount)
        {

            if (amount <= 0)
            {
                Debug.Log($"[ExpSystem] Ignoring non-positive EXP {amount} on {name}");
                return;
            }

            if (expToLevel == null || expToLevel.Length == 0)
            {
                Debug.LogWarning($"[ExpSystem] AddEXP called on {name} but expToLevel is null/empty. Using fallback ExpToNextLevel=1.");
            }

            CurrentEXP += amount;

            while (expToLevel != null &&
                   Level - 1 < expToLevel.Length &&
                   CurrentEXP >= expToLevel[Level - 1])
            {
                CurrentEXP -= expToLevel[Level - 1];
                Level++;
                Debug.Log($"[ExpSystem] Level up! {name} is now level {Level}");
                OnLevelUp?.Invoke(Level);
            }

            RaiseChanged();
        }

        private void RaiseChanged()
        {
            OnExpChanged?.Invoke();
        }
    }
}
