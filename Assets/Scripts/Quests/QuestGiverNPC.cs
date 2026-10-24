using Havengard.Dialogue;
using Havengard.Interactions;
using UnityEngine;

namespace Havengard.Quests
{
    /// <summary>NPC whose dialogue can add a quest to the expedition list via the 'start_quest' command.</summary>
    public class QuestGiverNPC : NPCInteractable
    {
        [Header("Quest")]
        [SerializeField] private QuestDefinition quest;

        protected override void OnQuestRequested(string id)
        {
            if (quest == null)
            {
                Debug.LogWarning("[QuestGiverNpc] No QuestDefinition assigned on " + name, this);
                return;
            }

            if (ExpeditionLog.Instance == null)
            {
                Debug.LogWarning("[QuestGiverNpc] No ExpeditionLog in the scene.", this);
                return;
            }

            bool added = ExpeditionLog.Instance.TryAdd(quest);
            Debug.Log("[QuestGiverNpc] '" + quest.displayName + "' " +
                (added ? "added to the expedition list." : "was already on the list."), this);
        }
    }
}