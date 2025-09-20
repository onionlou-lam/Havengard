using UnityEngine;
using Havengard.Progression;
using Havengard.Abilities;
using Havengard.Items;
using Havengard.HealthSystem;
using Havengard.Character;
using System.Collections.Generic;

namespace Havengard.Heroes
{
    [RequireComponent(typeof(AbilityUser))]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(ResourceSystem))]
    public class HeroInstance : MonoBehaviour
    {
        #region Quest Tracking
        public bool IsOnQuest { get; private set; }
        public int DaysRemainingOnQuest { get; private set; }

        public void StartQuest(int durationDays)
        {
            IsOnQuest = true;
            DaysRemainingOnQuest = durationDays;
        }

        public void ProgressQuestDay()
        {
            if (!IsOnQuest) return;
            DaysRemainingOnQuest--;
            if (DaysRemainingOnQuest <= 0)
            {
                CompleteQuest();
            }
        }

        private void CompleteQuest()
        {
            IsOnQuest = false;
            // Rewards are applied in QuestSystem
        }
        #endregion

        public HeroData Data { get; private set; }
        public ExperienceSystem ExpSystem { get; private set; }
        public AbilityUser AbilityUser { get; private set; }
        public Health Health { get; private set; }
        public ResourceSystem ResourceSystem { get; private set; }

        public List<ItemData> Inventory { get; private set; } = new();
        public List<ItemData> EquippedItems { get; private set; } = new();

        private void Awake()
        {
            AbilityUser = GetComponent<AbilityUser>();
            Health = GetComponent<Health>();
            ResourceSystem = GetComponent<ResourceSystem>();
        }

        public void Init(HeroData data)
        {
            Data = data;

            // Setup Experience
            ExpSystem = gameObject.AddComponent<ExperienceSystem>();
            ExpSystem.SetClass(data.playerClass);
            ExpSystem.OnLevelUp += HandleLevelUp;

            // Assign starting abilities
            AbilityUser.AssignAbilities(data.startingAbilities);

            // Sync stats
            ApplyStatsToSystems();
        }

        /// <summary>
        /// Calculates final stats (base + items + level scaling).
        /// </summary>
        public HeroStats GetStats()
        {
            HeroStats stats = new HeroStats
            {
                MaxHP = Data.baseHP,
                Attack = Data.baseAttack,
                Defense = Data.baseDefense,
                MaxResource = Data.baseResource
            };

            // Level scaling (+10% per level)
            if (ExpSystem != null && ExpSystem.Level > 1)
            {
                float multiplier = 1f + 0.1f * (ExpSystem.Level - 1);
                stats.MaxHP = Mathf.RoundToInt(stats.MaxHP * multiplier);
                stats.Attack = Mathf.RoundToInt(stats.Attack * multiplier);
                stats.Defense = Mathf.RoundToInt(stats.Defense * multiplier);
                stats.MaxResource = Mathf.RoundToInt(stats.MaxResource * multiplier);
            }

            // Equipment bonuses
            foreach (var item in EquippedItems)
            {
                if (item == null) continue;
                stats.MaxHP += item.healthBonus;
                stats.Attack += item.attackBonus;
                stats.Defense += item.defenseBonus;
                stats.MaxResource += item.resourceBonus;
            }

            return stats;
        }

        /// <summary>
        /// Pushes calculated stats into HealthSystem and ResourceSystem.
        /// </summary>
        public void ApplyStatsToSystems()
        {
            HeroStats stats = GetStats();
            if (Health != null)
            {
                var hs = Health.GetHealthSystem();
                float percent = hs.GetHealthNormalized();

                // Ensure we pass an int
                hs.SetMaxHealth(stats.MaxHP, true);
                hs.SetHealth(Mathf.RoundToInt(stats.MaxHP * percent));
            }

            // Resource
            if (ResourceSystem != null)
            {
                float percent = ResourceSystem.Max > 0 ? ResourceSystem.Current / ResourceSystem.Max : 1f;
                ResourceSystem.SetMaxResource(stats.MaxResource, false);
                ResourceSystem.SetResource(stats.MaxResource * percent);
            }
        }

        private void HandleLevelUp(int newLevel)
        {
            Debug.Log($"{Data.heroName} reached Level {newLevel}!");
            ApplyStatsToSystems();
        }

        // ---- Save / Load ----

        public HeroSaveData ToSaveData()
        {
            HeroSaveData save = new HeroSaveData
            {
                heroName = Data.heroName,
                className = ExpSystem.ClassData.className,
                level = ExpSystem.Level,
                currentEXP = ExpSystem.CurrentEXP,
                abilityNames = GetAbilityNames(),
                traitNames = GetTraitNames(),
                currentHP = Health?.GetHealthSystem().GetHealth() ?? Data.baseHP,
                currentResource = Mathf.RoundToInt(ResourceSystem?.Current ?? Data.baseResource),
                equippedItemNames = GetItemNames(EquippedItems),
                inventoryItemNames = GetItemNames(Inventory),
                position = transform.position
            };
            return save;
        }

        public void LoadFromSaveData(HeroSaveData save, HeroData heroData, ItemDatabase itemDB)
        {
            Data = heroData;

            ExpSystem = gameObject.AddComponent<ExperienceSystem>();
            ExpSystem.SetClass(heroData.playerClass);
            ExpSystem.AddEXP(save.currentEXP);
            ExpSystem.OnLevelUp += HandleLevelUp;

            AbilityUser.AssignAbilities(heroData.startingAbilities);

            EquippedItems = itemDB.GetItemsByNames(save.equippedItemNames);
            Inventory = itemDB.GetItemsByNames(save.inventoryItemNames);

            // Restore position
            transform.position = save.position;

            // Sync stats and restore current values
            ApplyStatsToSystems();
            if (Health != null)
                Health.GetHealthSystem().SetHealth(save.currentHP);
            if (ResourceSystem != null)
                ResourceSystem.SetResource(save.currentResource);
        }

        // ---- Utility methods (same as before) ----
        private string[] GetAbilityNames() { /* ... */ return null; }
        private string[] GetTraitNames() { /* ... */ return null; }
        private string[] GetItemNames(List<ItemData> items) { /* ... */ return null; }
    }
}
