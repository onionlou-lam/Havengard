using System.Collections.Generic;
using Havengard.Expeditions;
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

        [Header("Completion")]
        [Tooltip("The expedition the player must complete successfully. Leave empty for quests finished another way.")]
        public ExpeditionData expedition;

        [Header("Turn-in Reward")]
        public QuestRewardMode rewardMode = QuestRewardMode.Fixed;
        [Tooltip("Fixed: every reward is given. Choice: the player picks one.")]
        public List<QuestReward> rewards = new List<QuestReward>();

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(questId)) questId = name;
        }
    }
}