using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Havengard.Core.Progression;
using Havengard.Audio;
using System;

namespace Havengard.UI
{
    /// <summary>
    /// Character creation UI panel - handles class selection, appearance, and name input
    /// </summary>
    public class CharacterCreationUI : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject creationPanel;

        [Header("Class Selection")]
        [SerializeField] private Button mageButton;
        [SerializeField] private Button warriorButton;
        [SerializeField] private Button rangerButton;
        [SerializeField] private Image mageHighlight;
        [SerializeField] private Image warriorHighlight;
        [SerializeField] private Image rangerHighlight;

        [Header("Class Data")]
        [SerializeField] private PlayerClass mageClass;
        [SerializeField] private PlayerClass warriorClass;
        [SerializeField] private PlayerClass rangerClass;

        [Header("Appearance Selection")]
        [SerializeField] private Toggle maleToggle;
        [SerializeField] private Toggle femaleToggle;
        [SerializeField] private Image characterPreview;
        [SerializeField] private Sprite maleWarriorSprite;
        [SerializeField] private Sprite femaleWarriorSprite;
        [SerializeField] private Sprite maleMageSprite;
        [SerializeField] private Sprite femaleMageSprite;
        [SerializeField] private Sprite maleRangerSprite;
        [SerializeField] private Sprite femaleRangerSprite;

        [Header("Name Input")]
        [SerializeField] private TMP_InputField characterNameInput;

        [Header("Buttons")]
        [SerializeField] private Button createButton;
        [SerializeField] private Button cancelButton;

        [Header("Class Description")]
        [SerializeField] private TextMeshProUGUI classDescriptionText;

        // Events
        public event Action<CharacterCreationData> OnCharacterCreated;
        public event Action OnCreationCancelled;

        // State
        private PlayerClass selectedClass;
        private bool isMale = true;

        private void Start()
        {
            // Hook up class selection buttons
            if (mageButton != null)
            {
                mageButton.onClick.AddListener(() => SelectClass(mageClass, mageHighlight));
                AddButtonSounds(mageButton);
            }

            if (warriorButton != null)
            {
                warriorButton.onClick.AddListener(() => SelectClass(warriorClass, warriorHighlight));
                AddButtonSounds(warriorButton);
            }

            if (rangerButton != null)
            {
                rangerButton.onClick.AddListener(() => SelectClass(rangerClass, rangerHighlight));
                AddButtonSounds(rangerButton);
            }

            // Hook up gender toggles
            if (maleToggle != null)
                maleToggle.onValueChanged.AddListener((isOn) => { if (isOn) SelectGender(true); });

            if (femaleToggle != null)
                femaleToggle.onValueChanged.AddListener((isOn) => { if (isOn) SelectGender(false); });

            // Hook up action buttons
            if (createButton != null)
            {
                createButton.onClick.AddListener(OnCreateButtonClicked);
                AddButtonSounds(createButton);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(OnCancelButtonClicked);
                AddButtonSounds(cancelButton);
            }

            // Default selection
            if (warriorClass != null)
                SelectClass(warriorClass, warriorHighlight);

            if (maleToggle != null)
                maleToggle.isOn = true;
        }

        /// <summary>
        /// Add UI sounds to a button
        /// </summary>
        private void AddButtonSounds(Button button)
        {
            if (button == null) return;

            var trigger = button.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = button.gameObject.AddComponent<EventTrigger>();

            // Hover sound
            var hoverEntry = new EventTrigger.Entry();
            hoverEntry.eventID = EventTriggerType.PointerEnter;
            hoverEntry.callback.AddListener((data) => { 
                if (UIAudioManager.Instance != null && button.interactable)
                    UIAudioManager.Instance.PlayButtonHover(); 
            });
            trigger.triggers.Add(hoverEntry);
        }

        /// <summary>
        /// Show the character creation panel
        /// </summary>
        public void Show()
        {
            if (creationPanel != null)
                creationPanel.SetActive(true);

            // Reset to defaults
            if (characterNameInput != null)
                characterNameInput.text = "";

            if (warriorClass != null)
                SelectClass(warriorClass, warriorHighlight);

            if (maleToggle != null)
                maleToggle.isOn = true;

            if (UIAudioManager.Instance != null)
                UIAudioManager.Instance.PlayPanelOpen();
        }

        /// <summary>
        /// Hide the character creation panel
        /// </summary>
        public void Hide()
        {
            if (creationPanel != null)
                creationPanel.SetActive(false);

            if (UIAudioManager.Instance != null)
                UIAudioManager.Instance.PlayPanelClose();
        }

        /// <summary>
        /// Select a character class
        /// </summary>
        private void SelectClass(PlayerClass playerClass, Image highlight)
        {
            selectedClass = playerClass;

            // Update highlights
            if (mageHighlight != null)
                mageHighlight.gameObject.SetActive(highlight == mageHighlight);

            if (warriorHighlight != null)
                warriorHighlight.gameObject.SetActive(highlight == warriorHighlight);

            if (rangerHighlight != null)
                rangerHighlight.gameObject.SetActive(highlight == rangerHighlight);

            // Update description
            UpdateClassDescription();

            // Update preview
            UpdateCharacterPreview();

            // Play sound
            if (UIAudioManager.Instance != null)
                UIAudioManager.Instance.PlayButtonClick();

            Debug.Log($"[CharacterCreationUI] Selected class: {playerClass.className}");
        }

        /// <summary>
        /// Select character gender
        /// </summary>
        private void SelectGender(bool male)
        {
            isMale = male;
            UpdateCharacterPreview();

            // Play sound
            if (UIAudioManager.Instance != null)
                UIAudioManager.Instance.PlayButtonClick();

            Debug.Log($"[CharacterCreationUI] Selected gender: {(male ? "Male" : "Female")}");
        }

        /// <summary>
        /// Update the class description text
        /// </summary>
        private void UpdateClassDescription()
        {
            if (classDescriptionText == null || selectedClass == null) return;

            string description = selectedClass.className + "\n\n";
            description += $"HP: {selectedClass.baseHP}\n";
            description += $"Attack: {selectedClass.baseAttack}\n";
            description += $"Defense: {selectedClass.baseDefense}\n";
            description += $"Resource: {selectedClass.baseResource}\n";
            description += $"Move Speed: {selectedClass.baseMoveSpeed}\n";

            classDescriptionText.text = description;
        }

        /// <summary>
        /// Update the character preview sprite
        /// </summary>
        private void UpdateCharacterPreview()
        {
            if (characterPreview == null || selectedClass == null) return;

            Sprite previewSprite = null;

            // Select sprite based on class and gender
            if (selectedClass == warriorClass)
                previewSprite = isMale ? maleWarriorSprite : femaleWarriorSprite;
            else if (selectedClass == mageClass)
                previewSprite = isMale ? maleMageSprite : femaleMageSprite;
            else if (selectedClass == rangerClass)
                previewSprite = isMale ? maleRangerSprite : femaleRangerSprite;

            if (previewSprite != null)
                characterPreview.sprite = previewSprite;
        }

        /// <summary>
        /// Handle Create button click
        /// </summary>
        private void OnCreateButtonClicked()
        {
            // Validate input
            if (selectedClass == null)
            {
                Debug.LogWarning("[CharacterCreationUI] No class selected!");
                
                if (UIAudioManager.Instance != null)
                    UIAudioManager.Instance.PlayError();
                
                return;
            }

            if (characterNameInput == null || string.IsNullOrWhiteSpace(characterNameInput.text))
            {
                Debug.LogWarning("[CharacterCreationUI] Character name is empty!");
                
                if (UIAudioManager.Instance != null)
                    UIAudioManager.Instance.PlayError();
                
                return;
            }

            if (UIAudioManager.Instance != null)
                UIAudioManager.Instance.PlayButtonClick();

            // Create character data
            CharacterCreationData characterData = new CharacterCreationData
            {
                characterName = characterNameInput.text.Trim(),
                selectedClass = selectedClass,
                isMale = isMale
            };

            // Invoke event
            OnCharacterCreated?.Invoke(characterData);
        }

        /// <summary>
        /// Handle Cancel button click
        /// </summary>
        private void OnCancelButtonClicked()
        {
            if (UIAudioManager.Instance != null)
                UIAudioManager.Instance.PlayButtonClick();

            OnCreationCancelled?.Invoke();
        }
    }
}