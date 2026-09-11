using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Linq;

namespace Havengard.Expeditions.UI
{
    /// <summary>
    /// UI component for a clickable dungeon icon on the expedition map
    /// </summary>
    public class DungeonIconUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Data")]
        [SerializeField] private ExpeditionData expeditionData;

        [Header("Visual")]
        [SerializeField] private Image iconImage;
        [SerializeField] private GameObject activeIndicator;
        [SerializeField] private Image progressFill;
        [SerializeField] private TextMeshProUGUI progressText;

        [Header("Follower Activity Animation")]
        [Tooltip("Optional - plays a small follower activity animation (e.g. attacking, scouting) while an expedition is active here.")]
        [SerializeField] private DungeonMissionAnimationUI missionAnimation;

        [Header("Hover")]
        [SerializeField] private GameObject hoverHighlight;
        [SerializeField] private float hoverScale = 1.1f;

        private Vector3 originalScale;
        private ExpeditionInstance activeExpedition;
        private bool wasActiveLastUpdate;

        private void Start()
        {
            originalScale = transform.localScale;

            if (hoverHighlight != null)
                hoverHighlight.SetActive(false);

            if (activeIndicator != null)
                activeIndicator.SetActive(false);

            if (missionAnimation != null)
                missionAnimation.StopAnimation();

            // Subscribe to expedition updates
            if (ExpeditionManager.Instance != null)
            {
                ExpeditionManager.Instance.OnExpeditionsUpdated += UpdateDisplay;
            }

            UpdateDisplay();
        }

        private void OnDestroy()
        {
            if (ExpeditionManager.Instance != null)
            {
                ExpeditionManager.Instance.OnExpeditionsUpdated -= UpdateDisplay;
            }
        }

        private void Update()
        {
            // Update progress if expedition is active
            if (activeExpedition != null && activeExpedition.status == ExpeditionStatus.Active)
            {
                UpdateProgress();
            }
        }

        /// <summary>
        /// Updates the visual display based on expedition state
        /// </summary>
        private void UpdateDisplay()
        {
            if (ExpeditionManager.Instance == null || expeditionData == null)
                return;

            // Use LINQ FirstOrDefault instead of List.Find
            activeExpedition = ExpeditionManager.Instance.ActiveExpeditions
                .FirstOrDefault(exp => exp.expeditionData == expeditionData && exp.status == ExpeditionStatus.Active);

            bool hasActiveExpedition = activeExpedition != null;

            if (activeIndicator != null)
                activeIndicator.SetActive(hasActiveExpedition);

            if (hasActiveExpedition)
            {
                UpdateProgress();
            }
            else
            {
                if (progressFill != null)
                    progressFill.fillAmount = 0f;

                if (progressText != null)
                    progressText.text = "";
            }

            // Update follower activity animation only on state transitions
            if (missionAnimation != null)
            {
                if (hasActiveExpedition && !wasActiveLastUpdate)
                {
                    missionAnimation.PlayForMissionType(activeExpedition.missionType);
                }
                else if (!hasActiveExpedition && wasActiveLastUpdate)
                {
                    missionAnimation.StopAnimation();
                }
            }

            wasActiveLastUpdate = hasActiveExpedition;

            // Update icon sprite
            if (iconImage != null && expeditionData.mapIcon != null)
                iconImage.sprite = expeditionData.mapIcon;
        }

        /// <summary>
        /// Updates the progress display
        /// </summary>
        private void UpdateProgress()
        {
            if (activeExpedition == null) return;

            float progress = activeExpedition.GetProgress();

            if (progressFill != null)
                progressFill.fillAmount = progress;

            if (progressText != null)
            {
                progressText.text = $"Day {activeExpedition.daysElapsed}/{activeExpedition.durationInDays}";
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (expeditionData == null)
            {
                Debug.LogWarning("[DungeonIconUI] No expedition data assigned");
                return;
            }

            // Check if there's already an active expedition here
            if (activeExpedition != null && activeExpedition.status == ExpeditionStatus.Active)
            {
                Debug.Log($"[DungeonIconUI] Expedition already active at {expeditionData.displayName}");
                // Could show a different panel here showing active expedition details
                return;
            }

            Debug.Log($"[DungeonIconUI] Selected dungeon: {expeditionData.displayName}");

            // Open the expedition setup panel
            if (ExpeditionMapController.Instance != null)
            {
                ExpeditionMapController.Instance.OpenExpeditionSetup(expeditionData);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (hoverHighlight != null)
                hoverHighlight.SetActive(true);

            transform.localScale = originalScale * hoverScale;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (hoverHighlight != null)
                hoverHighlight.SetActive(false);

            transform.localScale = originalScale;
        }

        public ExpeditionData ExpeditionData => expeditionData;
    }
}