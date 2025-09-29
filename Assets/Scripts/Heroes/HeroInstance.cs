using UnityEngine;
using Havengard.Abilities;
using Havengard.HealthSystem;
using Havengard.Character;
using Havengard.Progression;
using System.Collections.Generic;

namespace Havengard.Heroes
{
    [RequireComponent(typeof(AbilityUser))]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(StatsComponent))]
    [DisallowMultipleComponent]
    public class HeroInstance : MonoBehaviour
    {
        public HeroData Data { get; private set; }
        public PlayerClass Class => Data?.heroClass;

        private AbilityUser abilityUser;
        private Health health;
        private ResourceSystem resourceSystem;
        private StatsComponent stats;
        private ExperienceSystem expSystem;

        private bool isOnQuest;
        private int questDaysRemaining;

        private void Awake()
        {
            abilityUser = GetComponent<AbilityUser>();
            health = GetComponent<Health>();
            resourceSystem = GetComponent<ResourceSystem>();
            stats = GetComponent<StatsComponent>();
            expSystem = GetComponent<ExperienceSystem>();
        }

        public void Initialize(HeroData data)
        {
            Data = data;

            // --- Stats from class ---
            if (stats != null && data.heroClass != null)
            {
                stats.CurrentStats.MaxHP = data.overrideStats ? data.overrideHP : data.heroClass.baseHP;
                stats.CurrentStats.Attack = data.overrideStats ? data.overrideAttack : data.heroClass.baseAttack;
                stats.CurrentStats.Defense = data.overrideStats ? data.overrideDefense : data.heroClass.baseDefense;
                stats.CurrentStats.MaxResource = data.overrideStats ? data.overrideResource : data.heroClass.baseResource;
                stats.CurrentStats.AttackSpeed = data.heroClass.baseAttackSpeed;
                stats.CurrentStats.MoveSpeed = data.heroClass.baseMoveSpeed;
                stats.CurrentStats.CritChance = data.heroClass.baseCritChance;
                stats.CurrentStats.CritMultiplier = data.heroClass.baseCritMultiplier;
            }

            if (health != null)
                health.GetHealthSystem().SetMaxHealth(stats.CurrentStats.MaxHP, true);

            if (resourceSystem != null)
                resourceSystem.SetMax(stats.CurrentStats.MaxResource, true);

            // --- Abilities ---
            List<AbilityBase> unlockedAbilities = new List<AbilityBase>();

            if (data.startingAbilities != null && data.startingAbilities.Count > 0)
            {
                unlockedAbilities.AddRange(data.startingAbilities);
            }
            else if (data.heroClass != null && data.heroClass.classAbilities.Length > 0)
            {
                foreach (var ca in data.heroClass.classAbilities)
                {
                    if (ca.requiredLevel <= 1 && ca.ability != null)
                        unlockedAbilities.Add(ca.ability);
                }
            }

            abilityUser.AssignAbilities(unlockedAbilities);

            // --- EXP progression ---
            if (expSystem != null && data.heroClass != null)
            {
                expSystem.InitEXPTable(data.heroClass.expToLevel);
                expSystem.OnLevelUp += HandleLevelUp;
            }
        }

        private void HandleLevelUp(int newLevel)
        {
            if (Data?.heroClass == null) return;

            // Growth
            stats.CurrentStats.MaxHP += Data.heroClass.hpGrowth;
            stats.CurrentStats.Attack += Data.heroClass.attackGrowth;
            stats.CurrentStats.Defense += Data.heroClass.defenseGrowth;
            stats.CurrentStats.MaxResource += Data.heroClass.resourceGrowth;

            if (health != null)
                health.GetHealthSystem().SetMaxHealth(stats.CurrentStats.MaxHP, false);

            if (resourceSystem != null)
                resourceSystem.SetMax(stats.CurrentStats.MaxResource, false);

            // Ability unlocks
            foreach (var ca in Data.heroClass.classAbilities)
            {
                if (ca.requiredLevel == newLevel && ca.ability != null)
                {
                    Debug.Log($"{Data.heroName} unlocked new ability: {ca.ability.AbilityName} at level {newLevel}");
                    abilityUser.AddAbility(ca.ability); // We’ll add this helper
                }
            }
        }

        // -----------------------------
        // Compatibility wrappers
        // -----------------------------

        public Stats GetStats() => stats.CurrentStats;
        public void Init(HeroData data) => Initialize(data);
        public ExperienceSystem ExpSystem => expSystem;
        public bool IsOnQuest => isOnQuest;

        public void StartQuest(int durationDays)
        {
            isOnQuest = true;
            questDaysRemaining = durationDays;
        }

        public void ProgressQuestDay()
        {
            if (!isOnQuest) return;

            questDaysRemaining--;
            if (questDaysRemaining <= 0)
            {
                isOnQuest = false;
                Debug.Log($"{Data.heroName} has returned from their quest!");
            }
        }
    }
}
