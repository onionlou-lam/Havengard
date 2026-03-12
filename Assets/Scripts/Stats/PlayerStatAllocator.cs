using UnityEngine;
using System;
using System.Collections.Generic;
using Havengard.Combat;
using Havengard.Core.Character;
using Havengard.Core.HealthSystem;

namespace Havengard.Core.Heroes
{
    /// <summary>
    /// Manages stat point allocation for the player
    /// </summary>
    public class PlayerStatAllocator : MonoBehaviour
    {
        [Header("Stat Points")]
        [SerializeField] private int unspentStatPoints = 0;
        [SerializeField] private int statPointsPerLevel = 3;
        
        [Header("Power Points")]
        [SerializeField] private int unspentPowerPoints = 0;
        [SerializeField] private int powerPointsPerLevel = 1;

        [Header("Stat Costs")]
        [SerializeField] private int healthCost = 1;
        [SerializeField] private int defenseCost = 1;
        [SerializeField] private int attackCost = 1;
        [SerializeField] private int resourceCost = 1;
        [SerializeField] private int movementSpeedCost = 1;

        [Header("Power Costs")]
        [SerializeField] private int abilityLevelCost = 1;
        [SerializeField] private int damageTypeCost = 1;

        [Header("Damage Type Bonuses")]
        [SerializeField] private Dictionary<DamageType, float> damageTypeBonuses = new Dictionary<DamageType, float>()
        {
            { DamageType.Physical, 0f },
            { DamageType.Fire, 0f },
            { DamageType.Frost, 0f },
            { DamageType.Lightning, 0f },
            { DamageType.Holy, 0f }
        };

        [Header("References")]
        [SerializeField] private Health health;
        [SerializeField] private StatsComponent stats;

        // Events
        public event Action<int> OnStatPointsChanged;
        public event Action<int> OnPowerPointsChanged;
        public event Action<DamageType, float> OnDamageTypeBonusChanged;

        public int UnspentStatPoints => unspentStatPoints;
        public int UnspentPowerPoints => unspentPowerPoints;
        public bool HasUnspentPoints => unspentStatPoints > 0 || unspentPowerPoints > 0;

        private void Awake()
        {
            if (health == null) health = GetComponent<Health>();
            if (stats == null) stats = GetComponent<StatsComponent>();

            // Subscribe to level up
            var expSystem = GetComponent<Havengard.Core.Progression.EXPSystem>();
            if (expSystem != null)
            {
                expSystem.OnLevelUp += OnPlayerLevelUp;
            }

            // Initialize damage type bonuses
            if (damageTypeBonuses == null || damageTypeBonuses.Count == 0)
            {
                damageTypeBonuses = new Dictionary<DamageType, float>()
                {
                    { DamageType.Physical, 0f },
                    { DamageType.Fire, 0f },
                    { DamageType.Frost, 0f },
                    { DamageType.Lightning, 0f },
                    { DamageType.Holy, 0f }
                };
            }
        }

        private void OnPlayerLevelUp(int newLevel)
        {
            GrantStatPoints(statPointsPerLevel);
            GrantPowerPoints(powerPointsPerLevel);
            Debug.Log($"[PlayerStatAllocator] Level {newLevel}! Granted {statPointsPerLevel} stat points and {powerPointsPerLevel} power points");
        }

        #region Stat Points

        public void GrantStatPoints(int amount)
        {
            unspentStatPoints += amount;
            OnStatPointsChanged?.Invoke(unspentStatPoints);
        }

        public bool CanAllocateStat(int cost)
        {
            return unspentStatPoints >= cost;
        }

        public bool AllocateHealth(int points = 1)
        {
            int cost = healthCost * points;
            if (!CanAllocateStat(cost)) return false;

            unspentStatPoints -= cost;
            
            if (health != null)
            {
                var healthSystem = health.GetHealthSystem();
                if (healthSystem != null)
                {
                    int bonusHealth = 10 * points;
                    healthSystem.SetMaxHealth(healthSystem.MaxHealth + bonusHealth, refill: false);
                    Debug.Log($"[PlayerStatAllocator] Allocated {points} points to Health (+{bonusHealth} max HP)");
                }
            }

            OnStatPointsChanged?.Invoke(unspentStatPoints);
            return true;
        }

        public bool AllocateDefense(int points = 1)
        {
            int cost = defenseCost * points;
            if (!CanAllocateStat(cost)) return false;

            unspentStatPoints -= cost;
            
            if (stats != null && stats.CurrentStats != null)
            {
                float bonusDefense = 2f * points; // +2 defense per point
                stats.CurrentStats.Defense += Mathf.RoundToInt(bonusDefense);
                Debug.Log($"[PlayerStatAllocator] Allocated {points} points to Defense (+{bonusDefense})");
            }

            OnStatPointsChanged?.Invoke(unspentStatPoints);
            return true;
        }

        public bool AllocateAttack(int points = 1)
        {
            int cost = attackCost * points;
            if (!CanAllocateStat(cost)) return false;

            unspentStatPoints -= cost;
            
            if (stats != null && stats.CurrentStats != null)
            {
                float bonusAttack = 3f * points; // +3 attack per point
                stats.CurrentStats.Attack += Mathf.RoundToInt(bonusAttack);
                Debug.Log($"[PlayerStatAllocator] Allocated {points} points to Attack (+{bonusAttack})");
            }

            OnStatPointsChanged?.Invoke(unspentStatPoints);
            return true;
        }

        public bool AllocateResource(int points = 1)
        {
            int cost = resourceCost * points;
            if (!CanAllocateStat(cost)) return false;

            unspentStatPoints -= cost;
            
            // Find resource system (mana/energy/etc)
            var resourceSystem = GetComponent<ResourceSystem>();
            if (resourceSystem != null)
            {
                int bonusResource = 10 * points; // +10 resource per point
                resourceSystem.IncreaseMaxResource(bonusResource);
                Debug.Log($"[PlayerStatAllocator] Allocated {points} points to Resource (+{bonusResource} max)");
            }

            OnStatPointsChanged?.Invoke(unspentStatPoints);
            return true;
        }

        public bool AllocateMovementSpeed(int points = 1)
        {
            int cost = movementSpeedCost * points;
            if (!CanAllocateStat(cost)) return false;

            unspentStatPoints -= cost;
            
            if (stats != null && stats.CurrentStats != null)
            {
                float bonusSpeed = 0.05f * points; // +5% speed per point
                stats.CurrentStats.MoveSpeed += bonusSpeed;
                Debug.Log($"[PlayerStatAllocator] Allocated {points} points to Movement Speed (+{bonusSpeed * 100}%)");
            }

            OnStatPointsChanged?.Invoke(unspentStatPoints);
            return true;
        }

        #endregion

        #region Power Points

        public void GrantPowerPoints(int amount)
        {
            unspentPowerPoints += amount;
            OnPowerPointsChanged?.Invoke(unspentPowerPoints);
        }

        public bool CanAllocatePower(int cost)
        {
            return unspentPowerPoints >= cost;
        }

        public bool UpgradeAbilityLevel(string abilityName, int points = 1)
        {
            int cost = abilityLevelCost * points;
            if (!CanAllocatePower(cost)) return false;

            unspentPowerPoints -= cost;
            
            // TODO: Implement ability level upgrade through AbilityUser
            var abilityUser = GetComponent<Havengard.Abilities.AbilityUser>();
            if (abilityUser != null)
            {
                // This would require extending AbilityUser to support level upgrades
                Debug.Log($"[PlayerStatAllocator] Upgraded {abilityName} by {points} levels");
            }

            OnPowerPointsChanged?.Invoke(unspentPowerPoints);
            return true;
        }

        public bool IncreaseDamageType(DamageType damageType, int points = 1)
        {
            int cost = damageTypeCost * points;
            if (!CanAllocatePower(cost)) return false;

            unspentPowerPoints -= cost;
            
            float bonusPerPoint = 0.1f; // +10% damage per point
            if (!damageTypeBonuses.ContainsKey(damageType))
            {
                damageTypeBonuses[damageType] = 0f;
            }
            
            damageTypeBonuses[damageType] += bonusPerPoint * points;
            
            OnDamageTypeBonusChanged?.Invoke(damageType, damageTypeBonuses[damageType]);
            OnPowerPointsChanged?.Invoke(unspentPowerPoints);
            
            Debug.Log($"[PlayerStatAllocator] Increased {damageType} damage bonus to {damageTypeBonuses[damageType] * 100}%");
            return true;
        }

        public float GetDamageTypeBonus(DamageType damageType)
        {
            if (damageTypeBonuses.ContainsKey(damageType))
                return damageTypeBonuses[damageType];
            return 0f;
        }

        #endregion
    }
}