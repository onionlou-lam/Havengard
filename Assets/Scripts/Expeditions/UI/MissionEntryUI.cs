using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Havengard.Expeditions.UI
{
    /// <summary>
    /// A single row in ActiveMissionsPanel representing one expedition that is
    /// either still in progress or completed and awaiting the player to claim the reward.
    /// </summary>
    public class MissionEntryUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image dungeonIcon;
        [SerializeField] private TextMeshProUGUI dungeonNameText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Image progressFill;
        [SerializeField] private GameObject pendingRewardBadge;
        [SerializeField] private Button entryButton;

        [Header("Status Colors")]
        [SerializeField] private Color inProgressColor = Color.yellow;
        [SerializeField] private Color readyToClaimColor = new Color(0.4f, 1f, 0.4f, 1f);

        private ExpeditionInstance expedition;

        public void Initialize(ExpeditionInstance instance)
        {
            expedition = instance;

            if (expedition == null || expedition.expeditionData == null)
            {
                Debug.LogWarning("[MissionEntryUI] Cannot initialize with null expedition/data");
                return;
            }

            if (dungeonIcon != null && expedition.expeditionData.mapIcon != null)
                dungeonIcon.sprite = expedition.expeditionData.mapIcon;

            if (dungeonNameText != null)
                dungeonNameText.text = expedition.expeditionData.displayName;

            bool isPendingResult = expedition.HasUnviewedResult();

            if (pendingRewardBadge != null)
                pendingRewardBadge.SetActive(isPendingResult);

            if (statusText != null)
            {
                if (isPendingResult)
                {
                    statusText.text = "Reward Ready!";
                    statusText.color = readyToClaimColor;
                }
                else
                {
                    statusText.text = ExpeditionDisplayUtility.GetProgressLabel(expedition);
                    statusText.color = inProgressColor;
                }
            }

            if (progressFill != null)
            {
                progressFill.gameObject.SetActive(!isPendingResult);
                if (!isPendingResult)
                    progressFill.fillAmount = expedition.GetProgress();
            }

            if (entryButton != null)
            {
                entryButton.onClick.RemoveAllListeners();
                entryButton.onClick.AddListener(OnEntryClicked);
            }
        }

        /// <summary>
        /// Clicking a ready-to-claim mission opens the reward panel directly.
        /// Clicking an in-progress mission is a no-op (nothing to claim yet),
        /// but could be extended to focus/highlight the dungeon on the map.
        /// </summary>
        private void OnEntryClicked()
        {
            if (expedition == null) return;

            if (expedition.HasUnviewedResult() && ExpeditionMapController.Instance != null)
            {
                ExpeditionMapController.Instance.OpenExpeditionResult(expedition);
            }
        }
    }
}