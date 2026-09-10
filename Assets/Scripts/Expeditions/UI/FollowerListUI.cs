using UnityEngine;
using System.Collections.Generic;
using Havengard.Core.Heroes;

namespace Havengard.Expeditions.UI
{
    /// <summary>
    /// UI component that displays the list of available followers
    /// </summary>
    public class FollowerListUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform followerEntryContainer;
        [SerializeField] private GameObject followerEntryPrefab;

        private List<FollowerEntryUI> followerEntries = new List<FollowerEntryUI>();

        private void Start()
        {
            RefreshFollowerList();

            // Subscribe to expedition updates
            if (ExpeditionManager.Instance != null)
            {
                ExpeditionManager.Instance.OnExpeditionsUpdated += RefreshFollowerList;
            }
        }

        private void OnDestroy()
        {
            if (ExpeditionManager.Instance != null)
            {
                ExpeditionManager.Instance.OnExpeditionsUpdated -= RefreshFollowerList;
            }
        }

        /// <summary>
        /// Refreshes the follower list from ExpeditionManager
        /// </summary>
        public void RefreshFollowerList()
        {
            if (ExpeditionManager.Instance == null || followerEntryContainer == null)
                return;

            // Clear existing entries
            foreach (var entry in followerEntries)
            {
                if (entry != null)
                    Destroy(entry.gameObject);
            }
            followerEntries.Clear();

            // Create entries for all followers
            var followers = ExpeditionManager.Instance.AllFollowers;
            foreach (var follower in followers)
            {
                if (follower == null || follower.Data == null) continue;

                var entryObj = Instantiate(followerEntryPrefab, followerEntryContainer);
                var entryUI = entryObj.GetComponent<FollowerEntryUI>();

                if (entryUI != null)
                {
                    entryUI.Initialize(follower);
                    followerEntries.Add(entryUI);
                }
            }

            Debug.Log($"[FollowerListUI] Displayed {followerEntries.Count} followers");
        }
    }
}