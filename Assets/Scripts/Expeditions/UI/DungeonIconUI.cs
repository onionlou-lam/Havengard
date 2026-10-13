using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

        [Header("Pending Reward Indicator")]
        [Tooltip("Shown when this dungeon has a completed expedition with an unviewed reward, prompting the player to click and claim it.")]
        [SerializeField] private GameObject pendingRewardBadge;
        [Tooltip("Optional pulsing scale animation applied to the pending reward badge to draw attention.")]
        [SerializeField] private float badgePulseSpeed = 3f;
        [SerializeField] private float badgePulseScale = 0.15f;

        [Header("Follower Activity Animation")]
        [Tooltip("Optional - plays a small follower activity animation (e.g. attacking, scouting) while an expedition is active here.")]
        [SerializeField] private DungeonMissionAnimationUI missionAnimation;

        [Header("Hover")]
        [SerializeField] private GameObject hoverHighlight;
        [SerializeField] private float hoverScale = 1.1f;
        [Tooltip("How long the highlight takes to shrink and fade after the mouse leaves.")]
        [SerializeField] private float hoverFadeDuration = 0.2f;
        [Tooltip("Scale the highlight shrinks to while fading out.")]
        [SerializeField] private float hoverExitScale = 0.6f;

        [Header("In Progress Styling")]
        [SerializeField] private Color inProgressTextColor = new Color(0.55f, 0.55f, 0.55f, 1f);

        private Vector3 originalScale;
        private Vector3 badgeOriginalScale;
        private ExpeditionInstance activeExpedition;
        private ExpeditionInstance pendingResultExpedition;
        private bool wasActiveLastUpdate;

        private CanvasGroup hoverCanvasGroup;
        private Vector3 hoverOriginalScale = Vector3.one;
        private Coroutine hoverFadeRoutine;

        private Color defaultStartTextColor = Color.white;

        private void Start()
        {
            originalScale = transform.localScale;

            if (hoverHighlight != null)
            {
                hoverOriginalScale = hoverHighlight.transform.localScale;
                hoverCanvasGroup = hoverHighlight.GetComponent<CanvasGroup>();
                if (hoverCanvasGroup == null)
                    hoverCanvasGroup = hoverHighlight.AddComponent<CanvasGroup>();

                hoverCanvasGroup.blocksRaycasts = false;
                hoverCanvasGroup.alpha = 0f;
                hoverHighlight.SetActive(false);
            }

            if (activeIndicator != null)
                activeIndicator.SetActive(false);

            if (pendingRewardBadge != null)
            {
                badgeOriginalScale = pendingRewardBadge.transform.localScale;
                pendingRewardBadge.SetActive(false);
            }

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

            // Pulse the pending reward badge to draw the player's attention
            if (pendingRewardBadge != null && pendingRewardBadge.activeSelf)
            {
                float pulse = ExpeditionDisplayUtility.GetBadgePulseScale(badgePulseSpeed, badgePulseScale);
                pendingRewardBadge.transform.localScale = badgeOriginalScale * pulse;
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

            pendingResultExpedition = ExpeditionManager.Instance.GetPendingResult(expeditionData);

            bool hasActiveExpedition = activeExpedition != null;
            bool hasPendingReward = pendingResultExpedition != null;

            if (activeIndicator != null)
                activeIndicator.SetActive(hasActiveExpedition);

            if (pendingRewardBadge != null)
                pendingRewardBadge.SetActive(hasPendingReward);

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
                progressText.text = ExpeditionDisplayUtility.GetProgressLabel(activeExpedition);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (expeditionData == null)
            {
                Debug.LogWarning("[DungeonIconUI] No expedition data assigned");
                return;
            }

            // A completed-but-unviewed reward always takes priority - single click reveals it
            if (pendingResultExpedition != null)
            {
                Debug.Log($"[DungeonIconUI] Claiming reward for: {expeditionData.displayName}");

                if (ExpeditionMapController.Instance != null)
                {
                    ExpeditionMapController.Instance.OpenExpeditionResult(pendingResultExpedition);
                }
                return;
            }

            // Active expeditions still open the panel - it displays them in "in progress" mode
            Debug.Log($"[DungeonIconUI] Selected dungeon: {expeditionData.displayName}");

            if (ExpeditionMapController.Instance != null)
            {
                ExpeditionMapController.Instance.OpenExpeditionSetup(expeditionData);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (hoverHighlight != null)
            {
                if (hoverFadeRoutine != null)
                    StopCoroutine(hoverFadeRoutine);

                hoverHighlight.transform.localScale = hoverOriginalScale;
                hoverCanvasGroup.alpha = 1f;
                hoverHighlight.SetActive(true);
            }

            transform.localScale = originalScale * hoverScale;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            transform.localScale = originalScale;

            if (hoverHighlight != null && hoverHighlight.activeSelf && isActiveAndEnabled)
            {
                if (hoverFadeRoutine != null)
                    StopCoroutine(hoverFadeRoutine);

                hoverFadeRoutine = StartCoroutine(FadeOutHover());
            }
        }

        private IEnumerator FadeOutHover()
        {
            float elapsed = 0f;
            Vector3 startScale = hoverHighlight.transform.localScale;
            Vector3 endScale = hoverOriginalScale * hoverExitScale;

            while (elapsed < hoverFadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / hoverFadeDuration);

                hoverCanvasGroup.alpha = 1f - t;
                hoverHighlight.transform.localScale = Vector3.Lerp(startScale, endScale, t);
                yield return null;
            }

            hoverHighlight.SetActive(false);
            hoverHighlight.transform.localScale = hoverOriginalScale;
            hoverFadeRoutine = null;
        }

        public ExpeditionData ExpeditionData => expeditionData;
    }
}