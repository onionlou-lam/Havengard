using Havengard.Quests;
using Yarn.Unity;
using UnityEngine;

namespace Havengard.Dialogue
{
    /// <summary>Quest facts that Yarn scripts can read. The game owns the state, Yarn only asks.</summary>
    public static class QuestDialogue
    {
        /// <summary>"none", "accepted", "inprogress" or "complete".</summary>
        [YarnFunction("quest_state")]
        public static string QuestState(string questId)
        {
            ExpeditionLog log = ExpeditionLog.Instance;
            if (log == null)
            {
                Debug.LogWarning("[QuestDialogue] quest_state('" + questId + "') called but there is no ExpeditionLog in the scene.");
                return "none";
            }

            string state = log.GetProgress(questId).ToString().ToLowerInvariant();

            if (state == "none" && log.Available.Count > 0)
            {
                Debug.Log("[QuestDialogue] quest_state('" + questId + "') = none. Known ids: " + KnownIds(log));
            }

            return state;
        }

        private static string KnownIds(ExpeditionLog log)
        {
            var ids = new System.Collections.Generic.List<string>();
            foreach (QuestDefinition quest in log.Available)
            {
                if (quest != null) ids.Add("'" + quest.questId + "'");
            }

            return string.Join(", ", ids);
        }

        /// <summary>True once the quest is on the expedition list, in any state.</summary>
        [YarnFunction("quest_given")]
        public static bool QuestGiven(string questId)
        {
            return QuestState(questId) != "none";
        }

        /// <summary>The state of the quest owned by the NPC currently speaking.</summary>
        [YarnFunction("npc_quest_state")]
        public static string NpcQuestState()
        {
            string questId = DialogueContext.CurrentQuestId;
            if (string.IsNullOrEmpty(questId))
            {
                Debug.LogWarning("[QuestDialogue] npc_quest_state() used, but the speaking NPC has no Quest assigned.");
                return "none";
            }

            return QuestState(questId);
        }
    }
}