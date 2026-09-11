using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Havengard.Core.Heroes;
using Havengard.Core.HealthManagement;
using Havengard.Waves;

namespace Havengard.Expeditions
{
    /// <summary>
    /// Central manager for the expedition system
    /// Tracks active expeditions, follower assignments, and progression
    /// </summary>
    public class ExpeditionManager : MonoBehaviour
    {
        public static ExpeditionManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField]
        [Tooltip("Maximum number of followers that can be assigned across all active expeditions")]
        private int maxActiveExpeditionFollowers = 10;

        [Header("References")]
        [SerializeField]
        [Tooltip("Optional reference to WaveManager for automatic integration")]
        private WaveManager waveManager;

        // Runtime state
        private List<ExpeditionInstance> activeExpeditions = new List<ExpeditionInstance>();
        private List<HeroInstance> allFollowers = new List<HeroInstance>();

        // Events
        public System.Action<ExpeditionInstance> OnExpeditionStarted;
        public System.Action<ExpeditionInstance, ExpeditionResult> OnExpeditionCompleted;
        public System.Action<ExpeditionInstance> OnMainStoryExpeditionCompleted;
        public System.Action OnExpeditionsUpdated;

        public int MaxActiveExpeditionFollowers => maxActiveExpeditionFollowers;
        public IReadOnlyList<ExpeditionInstance> ActiveExpeditions => activeExpeditions.AsReadOnly();
        public IReadOnlyList<HeroInstance> AllFollowers => allFollowers.AsReadOnly();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // Auto-find WaveManager if not assigned
            if (waveManager == null)
            {
                waveManager = FindFirstObjectByType<WaveManager>();
            }

            // Subscribe to wave completion
            if (waveManager != null && waveManager.waveEvents != null)
            {
                waveManager.waveEvents.OnWaveCleared.AddListener(OnWaveCompleted);
                Debug.Log("[ExpeditionManager] Subscribed to WaveManager.OnWaveCleared");
            }
            else
            {
                Debug.LogWarning("[ExpeditionManager] WaveManager or waveEvents not found. Expeditions will not progress automatically.");
            }

            // Find all followers in the scene
            RefreshFollowerList();
        }

        private void OnDestroy()
        {
            if (waveManager != null && waveManager.waveEvents != null)
            {
                waveManager.waveEvents.OnWaveCleared.RemoveListener(OnWaveCompleted);
            }
        }

        /// <summary>
        /// Refreshes the list of available followers from the scene
        /// </summary>
        public void RefreshFollowerList()
        {
            allFollowers.Clear();
            allFollowers.AddRange(FindObjectsByType<HeroInstance>(FindObjectsSortMode.None)
                .Where(h => h.Data != null && h.Data.faction == Units.Faction.Ally));

            Debug.Log($"[ExpeditionManager] Found {allFollowers.Count} followers");
        }

        /// <summary>
        /// Gets the current status of a follower
        /// </summary>
        public FollowerStatus GetFollowerStatus(HeroInstance hero)
        {
            if (hero == null) return FollowerStatus.Available;

            // Check if on expedition
            if (IsFollowerOnExpedition(hero))
            {
                return FollowerStatus.OnExpedition;
            }

            // Check if wounded (health <= 0)
            var health = hero.GetComponent<Health>();
            if (health != null && health.CurrentHealth <= 0)
            {
                return FollowerStatus.Wounded;
            }

            return FollowerStatus.Available;
        }

        /// <summary>
        /// Checks if a follower is currently assigned to an expedition
        /// </summary>
        public bool IsFollowerOnExpedition(HeroInstance hero)
        {
            return activeExpeditions.Any(exp =>
                exp.status == ExpeditionStatus.Active &&
                exp.assignedFollowers.Contains(hero));
        }

        /// <summary>
        /// Gets the current number of followers on active expeditions
        /// </summary>
        public int GetActiveExpeditionFollowerCount()
        {
            return activeExpeditions
                .Where(exp => exp.status == ExpeditionStatus.Active)
                .Sum(exp => exp.assignedFollowers.Count);
        }

        /// <summary>
        /// Validates if an expedition can be started with the given followers
        /// </summary>
        public bool CanStartExpedition(ExpeditionData data, List<HeroInstance> followers, out string reason)
        {
            reason = string.Empty;

            if (data == null)
            {
                reason = "Expedition data is null";
                return false;
            }

            if (!data.isAvailable || !data.isUnlocked)
            {
                reason = "Expedition is not available";
                return false;
            }

            if (followers == null || followers.Count == 0)
            {
                reason = "No followers assigned";
                return false;
            }

            if (followers.Count > data.maximumPartySize)
            {
                reason = $"Party exceeds maximum size ({data.maximumPartySize})";
                return false;
            }

            // Check global follower limit
            int currentActive = GetActiveExpeditionFollowerCount();
            if (currentActive + followers.Count > maxActiveExpeditionFollowers)
            {
                reason = $"Would exceed global expedition limit ({maxActiveExpeditionFollowers})";
                return false;
            }

            // Check if any followers are unavailable
            foreach (var follower in followers)
            {
                var status = GetFollowerStatus(follower);
                if (status != FollowerStatus.Available)
                {
                    reason = $"{follower.Data.heroName} is {status}";
                    return false;
                }
            }

            // Check required level if enabled
            if (data.hasRequiredLevel)
            {
                bool meetsRequirement = ValidatePartyLevel(followers, data.requiredLevel);
                if (!meetsRequirement)
                {
                    reason = $"Party does not meet required level ({data.requiredLevel})";
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Validates if the party meets the level requirement
        /// Can be customized later (e.g., average level, minimum level, etc.)
        /// </summary>
        protected virtual bool ValidatePartyLevel(List<HeroInstance> followers, int requiredLevel)
        {
            // Current rule: At least one follower must meet the required level
            // Can be changed to average level, all followers, etc.
            return followers.Any(f => f.ExpSystem != null && f.ExpSystem.Level >= requiredLevel);
        }

        /// <summary>
        /// Starts a new expedition
        /// </summary>
        public bool StartExpedition(ExpeditionData data, List<HeroInstance> followers)
        {
            if (!CanStartExpedition(data, followers, out string reason))
            {
                Debug.LogWarning($"[ExpeditionManager] Cannot start expedition: {reason}");
                return false;
            }

            int currentWave = waveManager != null ? waveManager.CurrentWaveIndex : 0;
            var instance = new ExpeditionInstance(data, followers, currentWave);

            // Mark followers as on expedition
            foreach (var follower in followers)
            {
                follower.StartQuest(data.durationInDays); // Reuse existing quest system
            }

            activeExpeditions.Add(instance);

            Debug.Log($"[ExpeditionManager] Started expedition: {data.displayName}");
            Debug.Log($"[ExpeditionManager] Party size: {followers.Count}/{data.maximumPartySize}");
            Debug.Log($"[ExpeditionManager] Active followers: {GetActiveExpeditionFollowerCount()}/{maxActiveExpeditionFollowers}");

            OnExpeditionStarted?.Invoke(instance);
            OnExpeditionsUpdated?.Invoke();

            return true;
        }

        /// <summary>
        /// Called when a wave is completed - advances all active expeditions
        /// </summary>
        public void OnWaveCompleted(int waveIndex)
        {
            Debug.Log($"[ExpeditionManager] Wave {waveIndex + 1} completed. Advancing expeditions...");

            var completedExpeditions = new List<ExpeditionInstance>();

            foreach (var expedition in activeExpeditions)
            {
                if (expedition.status != ExpeditionStatus.Active) continue;

                expedition.AdvanceDay();

                Debug.Log($"[ExpeditionManager] {expedition.expeditionData.displayName}: Day {expedition.daysElapsed}/{expedition.durationInDays}");

                if (expedition.IsComplete())
                {
                    completedExpeditions.Add(expedition);
                }
            }

            // Process completed expeditions
            foreach (var expedition in completedExpeditions)
            {
                CompleteExpedition(expedition);
            }

            OnExpeditionsUpdated?.Invoke();
        }

        /// <summary>
        /// Completes an expedition and processes results
        /// </summary>
        private void CompleteExpedition(ExpeditionInstance expedition)
        {
            expedition.status = ExpeditionStatus.Completed;

            Debug.Log($"[ExpeditionManager] Expedition completed: {expedition.expeditionData.displayName}");
            Debug.Log($"[ExpeditionManager] Followers returned: {expedition.assignedFollowers.Count}");
            Debug.Log($"[ExpeditionManager] Duration: {expedition.durationInDays} days");

            // Return followers to available status
            foreach (var follower in expedition.assignedFollowers)
            {
                follower.CompleteQuest(); // Reuse existing quest system
            }

            // Create result and evaluate success/failure
            var result = new ExpeditionResult(expedition.expeditionId, expedition.assignedFollowers);
            EvaluateExpeditionSuccess(expedition, result);

            Debug.Log($"[ExpeditionManager] Result: {(result.success ? "SUCCESS" : "FAILURE")} " +
                      $"(rolled {result.rolledValue:F2} vs chance {result.successChance:F2})");

            // Trigger events
            OnExpeditionCompleted?.Invoke(expedition, result);

            if (expedition.isMainStoryMission)
            {
                Debug.Log("[ExpeditionManager] Main story expedition completed.");
                OnMainStoryExpeditionCompleted?.Invoke(expedition);
            }

            // Remove from active list
            activeExpeditions.Remove(expedition);

            // Process rewards (only meaningful on success by default)
            ProcessRewards(result);
        }

        /// <summary>
        /// Rolls for expedition success based on ExpeditionData.baseSuccessChance.
        /// Populates the result with the outcome, the rolled value, and the chance used.
        /// Override EvaluateDungeonEffects/EvaluateFollowerEffects to modify the chance
        /// before the roll is performed.
        /// </summary>
        private void EvaluateExpeditionSuccess(ExpeditionInstance expedition, ExpeditionResult result)
        {
            float successChance = Mathf.Clamp01(expedition.expeditionData.baseSuccessChance);

            // Extension points - can modify successChance based on party/dungeon effects
            successChance = ModifySuccessChance(expedition, successChance);

            float roll = Random.value;

            result.successChance = successChance;
            result.rolledValue = roll;
            result.success = roll <= successChance;
        }

        /// <summary>
        /// Extension point for modifying the success chance before the roll.
        /// Combine follower bonuses, dungeon modifiers, etc. here.
        /// </summary>
        protected virtual float ModifySuccessChance(ExpeditionInstance expedition, float baseChance)
        {
            return baseChance;
        }

        /// <summary>
        /// Gets the expedition a follower is currently assigned to
        /// </summary>
        public ExpeditionInstance GetFollowerExpedition(HeroInstance hero)
        {
            return activeExpeditions.FirstOrDefault(exp =>
                exp.status == ExpeditionStatus.Active &&
                exp.assignedFollowers.Contains(hero));
        }

        /// <summary>
        /// Extension point for future reward processing
        /// </summary>
        protected virtual void ProcessRewards(ExpeditionResult result)
        {
            // Future implementation:
            // - Grant gold/celestium/exp (only if result.success == true)
            // - Apply follower-specific effects
            // - Generate items
            // - Trigger quest progression
        }

        /// <summary>
        /// Extension point for evaluating follower effects
        /// </summary>
        protected virtual void EvaluateFollowerEffects(ExpeditionInstance expedition)
        {
            // Future implementation:
            // - 2x Gold effect
            // - Reduced duration effect
            // - Class-specific bonuses
            // etc.
        }

        /// <summary>
        /// Extension point for evaluating dungeon effects
        /// </summary>
        protected virtual void EvaluateDungeonEffects(ExpeditionInstance expedition)
        {
            // Future implementation:
            // - Success chance modifiers based on party composition
            // - Special events
            // etc.
        }

        //public ExpeditionSaveData GetSaveData() { /* ... */ }
        //public void LoadSaveData(ExpeditionSaveData data) { /* ... */ }
    }
}