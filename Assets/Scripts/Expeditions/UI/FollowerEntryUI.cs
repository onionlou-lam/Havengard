using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Havengard.Core.Heroes;

namespace Havengard.Expeditions.UI
{
    /// <summary>
    /// UI component for a single follower entry in the follower list
    /// </summary>
    public class FollowerEntryUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image portraitImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI classText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Button selectButton;
        [SerializeField] private Image backgroundImage;

        [Header("Visual Styling")]
        [SerializeField] private Color availableColor = Color.white;
        [SerializeField] private Color unavailableColor = Color.gray;
        [SerializeField] private Color selectedColor = Color.green;

        private HeroInstance heroInstance;
        private bool isSelected = false;

        public HeroInstance HeroInstance => heroInstance;

        /// <summary>
        /// Initializes the entry with hero data
        /// </summary>
        public void Initialize(HeroInstance hero)
        {
            heroInstance = hero;
            isSelected = false;

            if (hero == null || hero.Data == null)
            {
                Debug.LogWarning("[FollowerEntryUI] Cannot initialize with null hero");
                return;
            }

            // Set portrait
            if (portraitImage != null && hero.Data.portrait != null)
                portraitImage.sprite = hero.Data.portrait;

            // Set name
            if (nameText != null)
                nameText.text = hero.Data.heroName;

            // Set level
            if (levelText != null && hero.ExpSystem != null)
                levelText.text = $"Lv. {hero.ExpSystem.Level}";

            // Set class
            if (classText != null && hero.Class != null)
                classText.text = hero.Class.className;

            // Setup button
            if (selectButton != null)
                selectButton.onClick.AddListener(OnClicked);

            UpdateDisplay();
        }

        /// <summary>
        /// Updates the visual display based on current status
        /// </summary>
        private void UpdateDisplay()
        {
            if (ExpeditionManager.Instance == null || heroInstance == null)
                return;

            var status = ExpeditionManager.Instance.GetFollowerStatus(heroInstance);

            // Update status text
            if (statusText != null)
            {
                switch (status)
                {
                    case FollowerStatus.Available:
                        statusText.text = "Available";
                        statusText.color = Color.green;
                        break;
                    case FollowerStatus.Wounded:
                        statusText.text = "Wounded";
                        statusText.color = Color.red;
                        break;
                    case FollowerStatus.OnExpedition:
                        statusText.text = "On Expedition";
                        statusText.color = Color.yellow;
                        break;
                }
            }

            // Update button interactability
            bool canSelect = status == FollowerStatus.Available;
            if (selectButton != null)
                selectButton.interactable = canSelect;

            // Update background color
            if (backgroundImage != null)
            {
                if (isSelected)
                    backgroundImage.color = selectedColor;
                else if (canSelect)
                    backgroundImage.color = availableColor;
                else
                    backgroundImage.color = unavailableColor;
            }
        }

        /// <summary>
        /// Called when the entry is clicked
        /// </summary>
        private void OnClicked()
        {
            var setupPanel = FindFirstObjectByType<ExpeditionSetupPanel>();
            if (setupPanel == null || !setupPanel.IsOpen)
            {
                Debug.LogWarning("[FollowerEntryUI] No expedition setup panel is open");
                return;
            }

            if (isSelected)
            {
                // Deselect
                setupPanel.DeselectFollower(heroInstance);
                isSelected = false;
            }
            else
            {
                // Select
                setupPanel.SelectFollower(heroInstance);
                isSelected = true;
            }

            UpdateDisplay();
        }

        /// <summary>
        /// Marks this entry as selected (visual only)
        /// </summary>
        public void SetSelected(bool selected)
        {
            isSelected = selected;
            UpdateDisplay();
        }
    }
}