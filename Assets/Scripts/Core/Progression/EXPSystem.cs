using UnityEngine;
using System;

namespace Havengard.Core.Progression
{
    public class EXPSystem : MonoBehaviour
    {
        public int CurrentEXP { get; private set; }
        public int Level { get; private set; } = 1;

        private int[] expToLevel;

        // NEW: Skill Points
        public int SkillPoints { get; private set; }
        public int SpentSkillPoints { get; private set; }
        
        public int AvailableSkillPoints => SkillPoints - SpentSkillPoints;

        // Events
        public event Action OnExpChanged;
        public event Action<int> OnLevelUp;
        public event Action<int> OnSkillPointsChanged; // NEW

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
                
                // Grant skill point on level up
                GrantSkillPoint();
                
                Debug.Log($"[ExpSystem] Level up! {name} is now level {Level}");
                OnLevelUp?.Invoke(Level);
            }

            RaiseChanged();
        }

        /// <summary>
        /// Reset EXP and level (for loading saves)
        /// </summary>
        public void ResetEXP()
        {
            CurrentEXP = 0;
            Level = 1;
            SkillPoints = 0;
            SpentSkillPoints = 0;
            RaiseChanged();
            Debug.Log($"[ExpSystem] Reset {name} to level 1");
        }
        
        /// <summary>
        /// Set EXP and level directly (for loading saves)
        /// </summary>
        public void SetEXPAndLevel(int exp, int level)
        {
            CurrentEXP = exp;
            Level = level;
            RaiseChanged();
            Debug.Log($"[ExpSystem] Set {name} to level {level} with {exp} EXP");
        }
        
        // NEW SKILL POINT METHODS
        
        /// <summary>
        /// Grant a skill point (called on level up)
        /// </summary>
        private void GrantSkillPoint()
        {
            SkillPoints++;
            OnSkillPointsChanged?.Invoke(AvailableSkillPoints);
            Debug.Log($"[ExpSystem] {name} gained 1 skill point! Total: {SkillPoints}, Available: {AvailableSkillPoints}");
        }
        
        /// <summary>
        /// Spend skill points to unlock an ability
        /// </summary>
        public bool TrySpendSkillPoints(int amount)
        {
            if (AvailableSkillPoints < amount)
            {
                Debug.LogWarning($"[ExpSystem] {name} cannot spend {amount} skill points. Available: {AvailableSkillPoints}");
                return false;
            }
            
            SpentSkillPoints += amount;
            OnSkillPointsChanged?.Invoke(AvailableSkillPoints);
            Debug.Log($"[ExpSystem] {name} spent {amount} skill points. Available: {AvailableSkillPoints}");
            return true;
        }
        
        /// <summary>
        /// Refund skill points (for respec functionality)
        /// </summary>
        public void RefundSkillPoints(int amount)
        {
            SpentSkillPoints = Mathf.Max(0, SpentSkillPoints - amount);
            OnSkillPointsChanged?.Invoke(AvailableSkillPoints);
            Debug.Log($"[ExpSystem] {name} refunded {amount} skill points. Available: {AvailableSkillPoints}");
        }
        
        /// <summary>
        /// Set skill points directly (for loading saves)
        /// </summary>
        public void SetSkillPoints(int total, int spent)
        {
            SkillPoints = total;
            SpentSkillPoints = spent;
            OnSkillPointsChanged?.Invoke(AvailableSkillPoints);
        }

        /// <summary>
        /// Add skill points directly (for testing/admin/rewards)
        /// </summary>
        public void AddSkillPoints(int amount)
        {
            if (amount <= 0) return;
            
            SkillPoints += amount;
            OnSkillPointsChanged?.Invoke(AvailableSkillPoints);
            Debug.Log($"[ExpSystem] {name} gained {amount} skill points! Total: {SkillPoints}, Available: {AvailableSkillPoints}");
        }

        private void RaiseChanged()
        {
            OnExpChanged?.Invoke();
        }
    }
}
