using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Havengard.Core.Heroes;

namespace Havengard.Expeditions.UI
{
    /// <summary>
    /// UI panel for setting up an expedition
    /// Displays dungeon info and allows follower selection
    /// </summary>
    public class ExpeditionSetupPanel : MonoBehaviour
    {
        [Header("Panel References")]
        [SerializeField] private GameObject panelRoot;

        [Header("Dungeon Info")]
        [SerializeField] private TextMeshProUGUI dungeonNameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI recommendedLevelText;
        [SerializeField] private TextMeshProUGUI requiredLevelText;
        [SerializeField] private TextMeshProUGUI partySizeText;
        [SerializeField] private TextMeshProUGUI durationText;
        [SerializeField] private TextMeshProUGUI missionTypeText;
        [SerializeField] private Image previewImage;

        [Header("Party Info")]
        [SerializeField] private TextMeshProUGUI selectedFollowersText;
        [SerializeField] private Transform followerPortraitContainer;
        [SerializeField] private GameObject followerPortraitPrefab;

        [Header("Buttons")]
        [SerializeField] private Button startExpeditionButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI startButtonText;

        private ExpeditionData currentExpedition;
        private List<HeroInstance> selectedFollowers = new List<HeroInstance>();

        private void Start()
        {
            if (startExpeditionButton != null)
                startExpeditionButton.onClick.AddListener(OnStartExpeditionClicked);

            if (closeButton != null)
                closeButton.onClick.AddListener(ClosePanel);

            ClosePanel();
        }

        /// <summary>
        /// Opens the panel for a specific expedition
        /// </summary>
        public void OpenPanel(ExpeditionData data)
        {
            if (data == null)
            {
                Debug.LogWarning("[ExpeditionSetupPanel] Cannot open with null expedition data");
                return;
            }

            currentExpedition = data;
            selectedFollowers.Clear();

            if (panelRoot != null)
                panelRoot.SetActive(true);

            UpdateDungeonInfo();
            UpdatePartyInfo();
            UpdateStartButton();

            Debug.Log($"[ExpeditionSetupPanel] Opened for: {data.displayName}");
        }

        /// <summary>
        /// Closes the setup panel
        /// </summary>
        public void ClosePanel()
        {
            if (panelRoot != null)
                panelRoot.SetActive(false);

            currentExpedition = null;
            selectedFollowers.Clear();
        }

        /// <summary>
        /// Adds a follower to the selected party
        /// </summary>
        public void SelectFollower(HeroInstance hero)
        {
            if (currentExpedition == null) return;

            if (selectedFollowers.Contains(hero))
            {
                Debug.LogWarning($"[ExpeditionSetupPanel] {hero.Data.heroName} is already selected");
                return;
            }

            if (selectedFollowers.Count >= currentExpedition.maximumPartySize)
            {
                Debug.LogWarning($"[ExpeditionSetupPanel] Party is full ({currentExpedition.maximumPartySize})");
                return;
            }

            var status = ExpeditionManager.Instance.GetFollowerStatus(hero);
            if (status != FollowerStatus.Available)
            {
                Debug.LogWarning($"[ExpeditionSetupPanel] {hero.Data.heroName} is {status}");
                return;
            }

            selectedFollowers.Add(hero);
            Debug.Log($"[ExpeditionSetupPanel] Selected follower: {hero.Data.heroName}");

            UpdatePartyInfo();
            UpdateStartButton();
        }

        /// <summary>
        /// Removes a follower from the selected party
        /// </summary>
        public void DeselectFollower(HeroInstance hero)
        {
            if (selectedFollowers.Remove(hero))
            {
                Debug.Log($"[ExpeditionSetupPanel] Deselected follower: {hero.Data.heroName}");
                UpdatePartyInfo();
                UpdateStartButton();
            }
        }

        /// <summary>
        /// Clears all selected followers
        /// </summary>
        public void ClearSelection()
        {
            selectedFollowers.Clear();
            UpdatePartyInfo();
            UpdateStartButton();
        }

        /// <summary>
        /// Updates the dungeon information display
        /// </summary>
        private void UpdateDungeonInfo()
        {
            if (currentExpedition == null) return;

            if (dungeonNameText != null)
                dungeonNameText.text = currentExpedition.displayName;

            if (descriptionText != null)
                descriptionText.text = currentExpedition.description;

            if (recommendedLevelText != null)
                recommendedLevelText.text = $"Recommended Level: {currentExpedition.recommendedLevel}";

            if (requiredLevelText != null)
            {
                if (currentExpedition.hasRequiredLevel)
                    requiredLevelText.text = $"Required Level: {currentExpedition.requiredLevel}";
                else
                    requiredLevelText.text = "Required Level: None";
            }

            if (partySizeText != null)
                partySizeText.text = $"Recommended Party: {currentExpedition.recommendedPartySize}   Maximum: {currentExpedition.maximumPartySize}";

            if (durationText != null)
                durationText.text = $"Estimated Completion: {currentExpedition.durationInDays} {(currentExpedition.durationInDays == 1 ? "Day" : "Days")}";

            if (missionTypeText != null)
                missionTypeText.text = $"Mission Type: {currentExpedition.missionType}";

            if (previewImage != null && currentExpedition.previewImage != null)
                previewImage.sprite = currentExpedition.previewImage;
        }

        /// <summary>
        /// Updates the party information display
        /// </summary>
        private void UpdatePartyInfo()
        {
            if (currentExpedition == null) return;

            if (selectedFollowersText != null)
            {
                selectedFollowersText.text = $"Selected: {selectedFollowers.Count} / {currentExpedition.maximumPartySize}";
            }

            // Update portrait display
            if (followerPortraitContainer != null)
            {
                // Clear existing portraits
                foreach (Transform child in followerPortraitContainer)
                {
                    Destroy(child.gameObject);
                }

                // Create portraits for selected followers
                foreach (var follower in selectedFollowers)
                {
                    if (followerPortraitPrefab != null && follower.Data != null)
                    {
                        var portraitObj = Instantiate(followerPortraitPrefab, followerPortraitContainer);
                        var image = portraitObj.GetComponent<Image>();
                        if (image != null && follower.Data.portrait != null)
                        {
                            image.sprite = follower.Data.portrait;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates the start button state
        /// </summary>
        private void UpdateStartButton()
        {
            if (startExpeditionButton == null || currentExpedition == null) return;

            bool canStart = ExpeditionManager.Instance.CanStartExpedition(
                currentExpedition,
                selectedFollowers,
                out string reason
            );

            startExpeditionButton.interactable = canStart;

            if (startButtonText != null)
            {
                if (canStart)
                    startButtonText.text = "Start Expedition";
                else
                    startButtonText.text = reason;
            }
        }

        /// <summary>
        /// Called when the Start Expedition button is clicked
        /// </summary>
        private void OnStartExpeditionClicked()
        {
            if (currentExpedition == null || selectedFollowers.Count == 0)
                return;

            bool success = ExpeditionManager.Instance.StartExpedition(currentExpedition, selectedFollowers);

            if (success)
            {
                ClosePanel();
                // Optionally close the map
                // ExpeditionMapController.Instance.CloseMap();
            }
        }

        public bool IsOpen => panelRoot != null && panelRoot.activeSelf;
        public ExpeditionData CurrentExpedition => currentExpedition;
        public List<HeroInstance> SelectedFollowers => new List<HeroInstance>(selectedFollowers);
    }
}