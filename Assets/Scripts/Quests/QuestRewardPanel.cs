using System.Collections.Generic;
using Havengard.Audio;
using Havengard.Core;
using Havengard.Expeditions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Havengard.Quests
{
    /// <summary>Popup for handing in a finished quest: shows the rewards, or lets the player pick one.</summary>
    public class QuestRewardPanel : MonoBehaviour
    {
        [Header("Panel")]
        [Tooltip("Child object that is shown and hidden. Keep this component on an always-active object.")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TextMeshProUGUI headerText;
        [SerializeField] private TextMeshProUGUI questNameText;
        [SerializeField] private TextMeshProUGUI messageText;

        [Header("Rewards")]
        [SerializeField] private RewardIconLibrary iconLibrary;
        [SerializeField] private Transform rewardContainer;
        [SerializeField] private QuestRewardEntryUI rewardEntryPrefab;

        [Header("Buttons")]
        [SerializeField] private Button claimButton;
        [SerializeField] private Button closeButton;

        [Header("Feedback")]
        [SerializeField] private ParticleSystem completeEffect;
        [Tooltip("Optional. Falls back to the UIAudioManager reward chime.")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip completeClip;

        private readonly List<QuestRewardEntryUI> entries = new List<QuestRewardEntryUI>();
        private QuestDefinition current;
        private bool claimed;
        private bool lockHeld;

        private void Awake()
        {
            if (claimButton != null) claimButton.onClick.AddListener(ClaimAll);
            if (closeButton != null) closeButton.onClick.AddListener(Hide);
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        private void OnDestroy()
        {
            ReleaseLock();
        }

        public void Show(QuestDefinition quest)
        {
            if (quest == null || ExpeditionLog.Instance == null) return;

            if (ExpeditionLog.Instance.GetProgress(quest.questId) != QuestProgress.ReadyToClaim)
            {
                Debug.LogWarning("[QuestRewardPanel] '" + quest.questId + "' is not ready to claim.", this);
                return;
            }

            current = quest;
            claimed = false;
            ClearEntries();

            bool choice = quest.rewardMode == QuestRewardMode.Choice;
            SetText(headerText, choice ? "Choose Your Reward" : "Quest Complete!");
            SetText(questNameText, quest.displayName);
            SetText(messageText, string.Empty);

            foreach (QuestReward reward in quest.rewards)
            {
                QuestReward picked = reward;
                QuestRewardEntryUI entry = Instantiate(rewardEntryPrefab, rewardContainer);
                entry.Setup(reward.GetIcon(iconLibrary), reward.GetLabel(),
                    choice ? (UnityEngine.Events.UnityAction)(() => Claim(new List<QuestReward> { picked })) : null);
                entries.Add(entry);
            }

            if (claimButton != null) claimButton.gameObject.SetActive(!choice);
            if (closeButton != null) closeButton.gameObject.SetActive(true);

            panelRoot.SetActive(true);
            AcquireLock();
        }

        public void Hide()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            ClearEntries();
            current = null;
            ReleaseLock();
        }

        private void ClaimAll()
        {
            if (current != null) Claim(current.rewards);
        }

        private void Claim(IList<QuestReward> rewards)
        {
            if (claimed || current == null) return;

            foreach (QuestReward reward in rewards)
            {
                string reason;
                if (!QuestRewardGrant.CanGrant(reward, out reason))
                {
                    SetText(messageText, reason);
                    return;
                }
            }

            foreach (QuestReward reward in rewards)
            {
                if (!QuestRewardGrant.Grant(reward))
                    Debug.LogWarning("[QuestRewardPanel] Could not grant '" + reward.GetLabel() + "'.", this);
            }

            claimed = true;
            ExpeditionLog.Instance.SetProgress(current, QuestProgress.Complete);

            SetText(headerText, "Reward Claimed!");
            SetText(messageText, string.Empty);
            if (claimButton != null) claimButton.gameObject.SetActive(false);
            foreach (QuestRewardEntryUI entry in entries) entry.Lock();

            if (completeEffect != null) completeEffect.Play();

            if (audioSource != null && completeClip != null)
                audioSource.PlayOneShot(completeClip);
            else if (UIAudioManager.Instance != null)
                UIAudioManager.Instance.PlayRewardChime();
        }

        private void ClearEntries()
        {
            foreach (QuestRewardEntryUI entry in entries)
            {
                if (entry != null) Destroy(entry.gameObject);
            }

            entries.Clear();
        }

        private void AcquireLock()
        {
            if (lockHeld) return;
            PlayerInputLock.Acquire();
            lockHeld = true;
        }

        private void ReleaseLock()
        {
            if (!lockHeld) return;
            lockHeld = false;
            PlayerInputLock.Release();
        }

        private static void SetText(TextMeshProUGUI text, string value)
        {
            if (text != null) text.text = value;
        }
    }
}