using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Havengard.Audio;

namespace Havengard.Expeditions.UI
{
    /// <summary>
    /// Reward/result reveal popup shown when the player clicks a dungeon icon
    /// whose expedition has completed but not yet been viewed. Displays success/failure,
    /// reward values with icons, and triggers follower celebration/sad reactions and audio.
    /// </summary>
    public class ExpeditionResultPanel : MonoBehaviour
    {
        [Header("Panel References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Outcome Display")]
        [SerializeField] private TextMeshProUGUI dungeonNameText;
        [SerializeField] private TextMeshProUGUI outcomeText;
        [SerializeField] private Image outcomeIcon;
        [SerializeField] private Color successColor = new Color(0.4f, 1f, 0.4f, 1f);
        [SerializeField] private Color failureColor = new Color(1f, 0.4f, 0.4f, 1f);

        [Header("Reward Display")]
        [SerializeField] private RewardIconLibrary iconLibrary;
        [SerializeField] private GameObject goldRewardRoot;
        [SerializeField] private Image goldIconImage;
        [SerializeField] private TextMeshProUGUI goldValueText;
        [SerializeField] private GameObject celestiumRewardRoot;
        [SerializeField] private Image celestiumIconImage;
        [SerializeField] private TextMeshProUGUI celestiumValueText;
        [SerializeField] private GameObject experienceRewardRoot;
        [SerializeField] private Image experienceIconImage;
        [SerializeField] private TextMeshProUGUI experienceValueText;

        [Header("Bonus Item Rewards")]
        [SerializeField] private Transform bonusItemContainer;
        [SerializeField] private GameObject bonusItemIconPrefab;

        [Header("Follower Reactions")]
        [Tooltip("Optional - portraits/prefabs instantiated per participating follower to show celebrate/sad reactions.")]
        [SerializeField] private Transform followerReactionContainer;
        [SerializeField] private GameObject followerReactionPrefab;

        [Header("Buttons")]
        [SerializeField] private Button closeButton;

        [Header("Animation Settings")]
        [SerializeField] private float fadeInDuration = 0.35f;
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private float rewardStaggerDelay = 0.15f;

        private ExpeditionInstance currentExpedition;
        private readonly List<GameObject> spawnedRewardIcons = new List<GameObject>();
        private readonly List<GameObject> spawnedFollowerReactions = new List<GameObject>();

        private void Start()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(ClosePanel);

            if (panelRoot != null)
                panelRoot.SetActive(false);
        }

        /// <summary>
        /// Shows the result/reward popup for a completed expedition.
        /// </summary>
        public void ShowResult(ExpeditionInstance expedition)
        {
            if (expedition == null || expedition.pendingResult == null)
            {
                Debug.LogWarning("[ExpeditionResultPanel] Cannot show result - expedition or pendingResult is null");
                return;
            }

            currentExpedition = expedition;
            var result = expedition.pendingResult;

            ClearSpawnedVisuals();

            if (panelRoot != null)
                panelRoot.SetActive(true);

            if (dungeonNameText != null)
                dungeonNameText.text = expedition.expeditionData.displayName;

            UpdateOutcomeDisplay(result.success);
            UpdateRewardDisplay(result);
            SpawnFollowerReactions(expedition, result.success);

            if (UIAudioManager.Instance != null)
            {
                if (result.success)
                {
                    UIAudioManager.Instance.PlaySuccess();
                    UIAudioManager.Instance.PlayRewardChime();
                }
                else
                {
                    UIAudioManager.Instance.PlayFailure();
                }
            }

            Debug.Log($"[ExpeditionResultPanel] Showing result for {expedition.expeditionData.displayName}: " +
                       $"{(result.success ? "SUCCESS" : "FAILURE")}");

            StopAllCoroutines();
            StartCoroutine(AnimateReveal());
        }

        /// <summary>
        /// Closes the panel and acknowledges the result with the ExpeditionManager,
        /// which removes the expedition from the active list.
        /// </summary>
        public void ClosePanel()
        {
            if (currentExpedition != null && ExpeditionManager.Instance != null)
            {
                ExpeditionManager.Instance.AcknowledgeExpeditionResult(currentExpedition);
            }

            currentExpedition = null;

            if (panelRoot != null)
                panelRoot.SetActive(false);

            ClearSpawnedVisuals();
        }

        private void UpdateOutcomeDisplay(bool success)
        {
            if (outcomeText != null)
            {
                outcomeText.text = success ? "Expedition Successful!" : "Expedition Failed";
                outcomeText.color = success ? successColor : failureColor;
            }

            if (outcomeIcon != null && iconLibrary != null)
            {
                var sprite = success ? iconLibrary.successIcon : iconLibrary.failureIcon;
                if (sprite != null)
                {
                    outcomeIcon.sprite = sprite;
                    outcomeIcon.gameObject.SetActive(true);
                }
                else
                {
                    outcomeIcon.gameObject.SetActive(false);
                }
            }
        }

        private void UpdateRewardDisplay(ExpeditionResult result)
        {
            bool hasGold = result.success && result.goldReward > 0;
            if (goldRewardRoot != null) goldRewardRoot.SetActive(hasGold);
            if (hasGold)
            {
                if (goldValueText != null) goldValueText.text = $"+{result.goldReward}";
                if (goldIconImage != null && iconLibrary != null && iconLibrary.goldIcon != null)
                    goldIconImage.sprite = iconLibrary.goldIcon;
            }

            bool hasCelestium = result.success && result.celestiumReward > 0;
            if (celestiumRewardRoot != null) celestiumRewardRoot.SetActive(hasCelestium);
            if (hasCelestium)
            {
                if (celestiumValueText != null) celestiumValueText.text = $"+{result.celestiumReward}";
                if (celestiumIconImage != null && iconLibrary != null && iconLibrary.celestiumIcon != null)
                    celestiumIconImage.sprite = iconLibrary.celestiumIcon;
            }

            bool hasExperience = result.success && result.experienceReward > 0;
            if (experienceRewardRoot != null) experienceRewardRoot.SetActive(hasExperience);
            if (hasExperience)
            {
                if (experienceValueText != null) experienceValueText.text = $"+{result.experienceReward} EXP";
                if (experienceIconImage != null && iconLibrary != null && iconLibrary.experienceIcon != null)
                    experienceIconImage.sprite = iconLibrary.experienceIcon;
            }

            // Bonus items
            if (bonusItemContainer != null && bonusItemIconPrefab != null)
            {
                foreach (var item in result.bonusItemRewards)
                {
                    if (item == null) continue;

                    var iconObj = Instantiate(bonusItemIconPrefab, bonusItemContainer);
                    spawnedRewardIcons.Add(iconObj);

                    var image = iconObj.GetComponentInChildren<Image>();
                    if (image != null && item.icon != null)
                        image.sprite = item.icon;

                    var label = iconObj.GetComponentInChildren<TextMeshProUGUI>();
                    if (label != null)
                        label.text = item.itemName;
                }
            }
        }

        private void SpawnFollowerReactions(ExpeditionInstance expedition, bool success)
        {
            if (followerReactionContainer == null || followerReactionPrefab == null)
                return;

            foreach (var follower in expedition.assignedFollowers)
            {
                if (follower == null || follower.Data == null) continue;

                var reactionObj = Instantiate(followerReactionPrefab, followerReactionContainer);
                spawnedFollowerReactions.Add(reactionObj);

                var portrait = reactionObj.GetComponentInChildren<Image>();
                if (portrait != null && follower.Data.portrait != null)
                    portrait.sprite = follower.Data.portrait;

                var reaction = reactionObj.GetComponentInChildren<FollowerMissionReaction>();
                if (reaction != null)
                    reaction.PlayReaction(success);
            }
        }

        private void ClearSpawnedVisuals()
        {
            foreach (var obj in spawnedRewardIcons)
                if (obj != null) Destroy(obj);
            spawnedRewardIcons.Clear();

            foreach (var obj in spawnedFollowerReactions)
                if (obj != null) Destroy(obj);
            spawnedFollowerReactions.Clear();
        }

        /// <summary>
        /// Fades the panel in, matching the project's NotificationUI coroutine-based fade convention.
        /// </summary>
        private IEnumerator AnimateReveal()
        {
            if (canvasGroup == null) yield break;

            float elapsed = 0f;
            canvasGroup.alpha = 0f;

            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = fadeCurve.Evaluate(elapsed / fadeInDuration);
                yield return null;
            }

            canvasGroup.alpha = 1f;
        }

        public bool IsOpen => panelRoot != null && panelRoot.activeSelf;
    }
}