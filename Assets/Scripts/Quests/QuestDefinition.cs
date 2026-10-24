using UnityEngine;

namespace Havengard.Quests
{
    /// <summary>Authoring asset for a quest an NPC can add to the expedition list.</summary>
    [CreateAssetMenu(menuName = "Havengard/Quests/Quest Definition")]
    public class QuestDefinition : ScriptableObject
    {
        [Tooltip("Unique and stable. Saves will refer to this id, so don't rename it later.")]
        public string questId;
        public string displayName;
        [TextArea(2, 4)] public string description;

        [Header("Rewards")]
        public int durationDays = 3;
        public int baseGoldReward;
        public int baseExpReward;
        public int baseCelestiumReward;

        /// <summary>Converts to the runtime Quest class that QuestSystem already uses.</summary>
        public Quest ToQuest()
        {
            return new Quest
            {
                questName = displayName,
                durationDays = durationDays,
                baseGoldReward = baseGoldReward,
                baseExpReward = baseExpReward,
                baseCelestiumReward = baseCelestiumReward
            };
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(questId)) questId = name;
        }
    }
}