using Havengard.Expeditions;
using UnityEngine;

namespace Havengard.Quests
{
    /// <summary>Moves quests through their states as the expeditions they depend on start and finish.</summary>
    public class QuestExpeditionTracker : MonoBehaviour
    {
        private ExpeditionManager manager;

        private void Start()
        {
            manager = ExpeditionManager.Instance;
            if (manager == null)
            {
                Debug.LogWarning("[QuestExpeditionTracker] No ExpeditionManager found. Quests won't follow expeditions.", this);
                return;
            }

            manager.OnExpeditionStarted += HandleStarted;
            manager.OnExpeditionCompleted += HandleCompleted;
        }

        private void OnDestroy()
        {
            if (manager == null) return;

            manager.OnExpeditionStarted -= HandleStarted;
            manager.OnExpeditionCompleted -= HandleCompleted;
        }

        private void HandleStarted(ExpeditionInstance expedition)
        {
            ExpeditionLog log = ExpeditionLog.Instance;
            if (log == null) return;

            foreach (QuestDefinition quest in log.Available)
            {
                if (quest.expedition != expedition.expeditionData) continue;
                if (log.GetProgress(quest.questId) != QuestProgress.Accepted) continue;

                Debug.Log("[QuestExpeditionTracker] '" + quest.questId + "' -> InProgress", this);
                log.SetProgress(quest, QuestProgress.InProgress);
            }
        }

        private void HandleCompleted(ExpeditionInstance expedition, ExpeditionResult result)
        {
            ExpeditionLog log = ExpeditionLog.Instance;
            if (log == null) return;

            foreach (QuestDefinition quest in log.Available)
            {
                if (quest.expedition != expedition.expeditionData) continue;

                QuestProgress current = log.GetProgress(quest.questId);
                if (current != QuestProgress.Accepted && current != QuestProgress.InProgress) continue;

                // A failed run doesn't fail the quest. The player can send another party.
                QuestProgress next = result.success ? QuestProgress.ReadyToClaim : QuestProgress.Accepted;
                Debug.Log("[QuestExpeditionTracker] '" + quest.questId + "' -> " + next, this);
                log.SetProgress(quest, next);
            }
        }
    }
}