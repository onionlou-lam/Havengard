using System.Collections.Generic;
using UnityEngine;
using Havengard.Abilities;
using Havengard.Core.HealthManagement;
using Havengard.Core.Character;
using Havengard.Core.Progression;

namespace Havengard.Core.Heroes
{
    [RequireComponent(typeof(AbilityUser))]
    [RequireComponent(typeof(Havengard.Core.HealthManagement.Health))]
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

        private Havengard.Core.HealthManagement.Health health;
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
            // Resolve components (don't assume Awake order)
            abilityUser ??= GetComponent<AbilityUser>();
            expSystem ??= GetComponent<EXPSystem>();
            health ??= GetComponent<HealthManagement.Health>();
            resourceSystem ??= GetComponent<ResourceSystem>();
            statsComponent ??= GetComponent<StatsComponent>();

            // Ensure StatsComponent has a runtime instance
            if (statsComponent != null && statsComponent.CurrentStats == null)
                statsComponent.SetCurrentStats(null);

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
            // Set max health through Health component
            health.SetMaxHealthFromStats(refill: true);

            // Set max resource through ResourceSystem
            resourceSystem.SetMaxResource(statsComponent.CurrentStats.MaxResource);
            resourceSystem.SetToMax();

            // ----- 3) EXP System -----
            if (playerClassData.expToLevel != null && playerClassData.expToLevel.Length > 0)
            {
                expSystem.InitEXPTable(playerClassData.expToLevel);
            }

            // ----- 4) Abilities -----
            // Note: AbilityUser doesn't have InitAbilities() or heroClass field
            // These would need to be added to AbilityUser if needed
            // For now, skip this initialization
        }

        #region Quest/Expedition Methods
        
        /// <summary>
        /// Starts a quest or expedition for this hero
        /// </summary>
        public void StartQuest(int durationDays)
        {
            isOnQuest = true;
            questDaysRemaining = durationDays;
            Debug.Log($"[HeroInstance] {Data?.heroName ?? name} started quest/expedition for {durationDays} days");
        }

        /// <summary>
        /// Advances the quest/expedition by one day
        /// </summary>
        public void ProgressQuestDay()
        {
            if (!isOnQuest) return;

            questDaysRemaining--;

            if (questDaysRemaining <= 0)
            {
                CompleteQuest();
            }
        }

        /// <summary>
        /// Completes the quest/expedition
        /// </summary>
        public void CompleteQuest()
        {
            isOnQuest = false;
            questDaysRemaining = 0;
            Debug.Log($"[HeroInstance] {Data?.heroName ?? name} completed quest/expedition");
        }

        #endregion

        #region EXP Methods

        /// <summary>
        /// Grants EXP to this hero
        /// </summary>
        public void GrantEXP(int amount)
        {
            if (expSystem != null)
            {
                expSystem.AddEXP(amount);
            }
            else
            {
                Debug.LogWarning($"[HeroInstance] {Data?.heroName ?? name} has no EXPSystem to grant EXP");
            }
        }

        #endregion
    }
}

