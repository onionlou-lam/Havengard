using UnityEngine;
using System.Collections.Generic;
using Havengard.Abilities;
using Havengard.Core.HealthSystem;
using Havengard.Core.Character;
using Havengard.Core.Progression;

namespace Havengard.Core.Heroes
{
    [RequireComponent(typeof(AbilityUser))]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(StatsComponent))]
    [RequireComponent(typeof(ResourceSystem))]
    [RequireComponent(typeof(EXPSystem))]
    [DisallowMultipleComponent]
    public class HeroInstance : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private HeroData heroData;
        [SerializeField] private PlayerClass playerClassOverride;

        [Header("Systems (optional manual wiring)")]
        [SerializeField] private AbilityUser abilityUser;
        [SerializeField] private EXPSystem expSystem;

        private Health health;
        private ResourceSystem resourceSystem;
        private StatsComponent statsComponent;

        private bool isOnQuest;
        private int questDaysRemaining;

        public HeroData Data => heroData;

        public PlayerClass Class
        {
            get
            {
                if (heroData != null && heroData.heroClass != null)
                    return heroData.heroClass;
                return playerClassOverride;
            }
        }

        public EXPSystem ExpSystem => expSystem;
        public bool IsOnQuest => isOnQuest;

        public HeroStats GetStats() => (statsComponent != null) ? statsComponent.CurrentStats : null;

        private void Awake()
        {
            // Resolve components (don’t assume Awake order)
            abilityUser ??= GetComponent<AbilityUser>();
            expSystem ??= GetComponent<EXPSystem>();
            health ??= GetComponent<Health>();
            resourceSystem ??= GetComponent<ResourceSystem>();
            statsComponent ??= GetComponent<StatsComponent>();

            // Ensure StatsComponent has a runtime instance
            if (statsComponent != null && statsComponent.CurrentStats == null)
                statsComponent.SetCurrentStats(null);

            //Debug.Log($"[HeroInstance] Awake on {name}. HeroData={(heroData ? heroData.name : "NULL")} Class={(Class ? Class.name : "NULL")}");

            if (heroData != null)
                InitializeFromData(heroData, Class);
            else
                Debug.LogWarning($"[HeroInstance] {name} has no HeroData assigned.");
        }

        public void Init(HeroData data)
        {
            heroData = data;
            InitializeFromData(heroData, Class);
        }

        private void InitializeFromData(HeroData data, PlayerClass playerClassData)
        {
            // Guard checks
            var missing = new List<string>();
            if (statsComponent == null) missing.Add("StatsComponent");
            if (health == null) missing.Add("Health");
            if (resourceSystem == null) missing.Add("ResourceSystem");
            if (abilityUser == null) missing.Add("AbilityUser");
            if (expSystem == null) missing.Add("EXPSystem");

            if (missing.Count > 0)
            {
                Debug.LogWarning($"[HeroInstance] {name} missing core components: {string.Join(", ", missing)}");
                return;
            }

            if (data == null)
            {
                Debug.LogWarning($"[HeroInstance] {name} InitializeFromData called with null HeroData.");
                return;
            }

            if (playerClassData == null)
            {
                Debug.LogWarning($"[HeroInstance] {name} has no PlayerClass assigned (HeroData.heroClass or override).");
                return;
            }

            // Ensure runtime stats container exists
            if (statsComponent.CurrentStats == null)
                statsComponent.SetCurrentStats(null);

            // ----- 1) Stats -----
            int baseHP = data.overrideStats ? data.overrideHP : playerClassData.baseHP;
            int baseAttack = data.overrideStats ? data.overrideAttack : playerClassData.baseAttack;
            int baseDefense = data.overrideStats ? data.overrideDefense : playerClassData.baseDefense;
            int baseResource = data.overrideStats ? data.overrideResource : playerClassData.baseResource;

            statsComponent.CurrentStats.MaxHP = Mathf.Max(1, baseHP);
            statsComponent.CurrentStats.Attack = Mathf.Max(0, baseAttack);
            statsComponent.CurrentStats.Defense = Mathf.Max(0, baseDefense);
            statsComponent.CurrentStats.MaxResource = Mathf.Max(1, baseResource);

            statsComponent.CurrentStats.AttackSpeed = playerClassData.baseAttackSpeed;
            statsComponent.CurrentStats.MoveSpeed = playerClassData.baseMoveSpeed;
            statsComponent.CurrentStats.CritChance = playerClassData.baseCritChance;
            statsComponent.CurrentStats.CritMultiplier = playerClassData.baseCritMultiplier;

            // ----- 2) Health & Resource -----
            // Health now safely initialises itself lazily, and can sync from stats
            health.SetStartingMaxHealth(statsComponent.CurrentStats.MaxHP);
            health.SetMaxHealthFromStats(refill: true);

            // ResourceSystem naming: your version uses SetMax(...)
            resourceSystem.SetMaxResource(statsComponent.CurrentStats.MaxResource);
            resourceSystem.SetToMax();

            // ----- 3) Abilities -----
            var unlockedAbilities = new List<AbilityBase>();
            if (data.startingAbilities != null && data.startingAbilities.Count > 0)
                unlockedAbilities.AddRange(data.startingAbilities);

            abilityUser.AssignAbilities(unlockedAbilities);

            // ----- 4) EXP table -----
            if (playerClassData.expToLevel != null && playerClassData.expToLevel.Length > 0)
            {
                //Debug.Log($"[HeroInstance] Initialising EXP table on {name} from PlayerClass {playerClassData.name}");
                expSystem.InitEXPTable(playerClassData.expToLevel);
            }
            else
            {
                Debug.LogWarning($"[HeroInstance] {name} PlayerClass {playerClassData.name} has no expToLevel table.");
                expSystem.InitEXPTable(new int[] { 100 }); // safe default
            }

            expSystem.OnLevelUp -= HandleLevelUp;
            expSystem.OnLevelUp += HandleLevelUp;
        }

        private void HandleLevelUp(int newLevel)
        {
            var classData = Class;
            if (classData == null || statsComponent == null || statsComponent.CurrentStats == null) return;

            statsComponent.CurrentStats.MaxHP += classData.hpGrowth;
            statsComponent.CurrentStats.Attack += classData.attackGrowth;
            statsComponent.CurrentStats.Defense += classData.defenseGrowth;
            statsComponent.CurrentStats.MaxResource += classData.resourceGrowth;

            health.SetStartingMaxHealth(statsComponent.CurrentStats.MaxHP);
            health.SetMaxHealthFromStats(refill: false);
            resourceSystem.SetMaxResource(statsComponent.CurrentStats.MaxResource);
            Debug.Log($"[HeroInstance] {(Data != null ? Data.heroName : name)} reached level {newLevel}.");
        }

        public void GrantEXP(int amount)
        {
            //Debug.Log($"[HeroInstance] GrantEXP({amount}) called on {name}");
            expSystem?.AddEXP(amount);
        }

        // ---------- Quest helpers ----------
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
                Debug.Log($"[HeroInstance] {(Data != null ? Data.heroName : name)} has returned from their quest!");
            }
        }
    }
}
