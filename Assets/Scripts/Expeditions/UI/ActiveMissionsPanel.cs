using UnityEngine;
using System.Collections.Generic;

namespace Havengard.Expeditions.UI
{
    /// <summary>
    /// Compact list of currently in-progress expeditions and completed-but-unclaimed
    /// expeditions, meant to sit alongside FollowerListUI in the same side panel
    /// (e.g. as a tab or a stacked section). Lets the player jump straight to the
    /// dungeon's reward/result popup or the map without hunting for the dungeon icon.
    /// </summary>
    public class ActiveMissionsPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform missionEntryContainer;
        [SerializeField] private GameObject missionEntryPrefab;

        [Header("Empty State")]
        [Tooltip("Optional - shown when there are no active or pending expeditions.")]
        [SerializeField] private GameObject emptyStateRoot;

        private readonly List<MissionEntryUI> missionEntries = new List<MissionEntryUI>();

        private void Start()
        {
            RefreshMissionList();

            if (ExpeditionManager.Instance != null)
            {
                ExpeditionManager.Instance.OnExpeditionsUpdated += RefreshMissionList;
            }
        }

        private void OnDestroy()
        {
            if (ExpeditionManager.Instance != null)
            {
                ExpeditionManager.Instance.OnExpeditionsUpdated -= RefreshMissionList;
            }
        }

        /// <summary>
        /// Refreshes the mission list from ExpeditionManager, showing both
        /// Active expeditions (with progress) and Completed-unviewed expeditions (claimable).
        /// </summary>
        public void RefreshMissionList()
        {
            if (ExpeditionManager.Instance == null || missionEntryContainer == null)
                return;

            // Clear existing entries
            foreach (var entry in missionEntries)
            {
                if (entry != null)
                    Destroy(entry.gameObject);
            }
            missionEntries.Clear();

            var expeditions = ExpeditionManager.Instance.ActiveExpeditions;
            foreach (var expedition in expeditions)
            {
                bool isActive = expedition.status == ExpeditionStatus.Active;
                bool isPendingResult = expedition.HasUnviewedResult();

                if (!isActive && !isPendingResult)
                    continue;

                var entryObj = Instantiate(missionEntryPrefab, missionEntryContainer);
                var entryUI = entryObj.GetComponent<MissionEntryUI>();

                if (entryUI != null)
                {
                    entryUI.Initialize(expedition);
                    missionEntries.Add(entryUI);
                }
            }

            if (emptyStateRoot != null)
                emptyStateRoot.SetActive(missionEntries.Count == 0);

            Debug.Log($"[ActiveMissionsPanel] Displayed {missionEntries.Count} mission(s)");
        }
    }
}