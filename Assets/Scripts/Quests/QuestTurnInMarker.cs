using Havengard.Dialogue;
using UnityEngine;

namespace Havengard.Quests
{
    /// <summary>Shows an indicator above an NPC while their quest is waiting to be handed in.</summary>
    public class QuestTurnInMarker : MonoBehaviour
    {
        [Tooltip("Child object shown while a reward is waiting. Must not be the object this component is on.")]
        [SerializeField] private GameObject marker;

        private IQuestDialogueSource source;
        private ExpeditionLog log;

        private void Awake()
        {
            source = GetComponent<IQuestDialogueSource>();
            if (marker != null) marker.SetActive(false);
        }

        private void Start()
        {
            log = ExpeditionLog.Instance;
            if (log != null) log.ProgressChanged += HandleProgressChanged;
            Refresh();
        }

        private void OnDestroy()
        {
            if (log != null) log.ProgressChanged -= HandleProgressChanged;
        }

        private void HandleProgressChanged(QuestDefinition quest, QuestProgress progress)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (marker == null) return;

            string questId = source != null ? source.QuestId : null;
            bool waiting = log != null && !string.IsNullOrEmpty(questId)
                && log.GetProgress(questId) == QuestProgress.ReadyToClaim;

            marker.SetActive(waiting);
        }
    }
}