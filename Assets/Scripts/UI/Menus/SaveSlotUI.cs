using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Havengard.Save;
using Havengard.Audio;
using System;

namespace Havengard.UI
{
    /// <summary>
    /// Individual save slot UI element - displays save info or empty slot
    /// </summary>
    public class SaveSlotUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button slotButton;
        [SerializeField] private GameObject emptySlotGroup;
        [SerializeField] private GameObject filledSlotGroup;

        [Header("Empty Slot")]
        [SerializeField] private TextMeshProUGUI emptySlotText;

        [Header("Filled Slot Info")]
        [SerializeField] private TextMeshProUGUI characterNameText;
        [SerializeField] private TextMeshProUGUI classText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI playtimeText;
        [SerializeField] private TextMeshProUGUI lastPlayedText;

        [Header("Delete Button")]
        [SerializeField] private Button deleteButton;

        // Events
        public event Action<int> OnSlotSelected;
        public event Action<int> OnSlotDeleted;

        private int slotIndex;
        private bool isEmpty;
        private string characterName; // Store for delete confirmation

        private void Start()
        {
            if (slotButton != null)
            {
                slotButton.onClick.AddListener(OnSlotClicked);
                AddButtonSounds(slotButton);
            }

            if (deleteButton != null)
            {
                deleteButton.onClick.AddListener(OnDeleteClicked);
                AddButtonSounds(deleteButton);
            }
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
        /// Initialize the save slot with data
        /// </summary>
        public void Initialize(int index, GameSaveData saveData)
        {
            slotIndex = index;
            isEmpty = (saveData == null);

            if (isEmpty)
            {
                ShowEmptySlot();
            }
            else
            {
                ShowFilledSlot(saveData);
            }
        }

        /// <summary>
        /// Show empty slot UI
        /// </summary>
        private void ShowEmptySlot()
        {
            if (emptySlotGroup != null)
                emptySlotGroup.SetActive(true);

            if (filledSlotGroup != null)
                filledSlotGroup.SetActive(false);

            if (emptySlotText != null)
                emptySlotText.text = $"Empty Slot {slotIndex}";

            if (deleteButton != null)
                deleteButton.gameObject.SetActive(false);

            characterName = null;
        }

        /// <summary>
        /// Show filled slot UI with save data
        /// </summary>
        private void ShowFilledSlot(GameSaveData saveData)
        {
            if (emptySlotGroup != null)
                emptySlotGroup.SetActive(false);

            if (filledSlotGroup != null)
                filledSlotGroup.SetActive(true);

            characterName = saveData.mainCharacterName ?? "Unknown Hero";

            if (characterNameText != null)
                characterNameText.text = characterName;

            if (classText != null)
                classText.text = saveData.mainCharacterClass ?? "Adventurer";

            if (levelText != null)
                levelText.text = $"Level {saveData.mainCharacterLevel}";

            if (playtimeText != null)
            {
                float hours = Mathf.Floor(saveData.playTime / 3600f);
                float minutes = Mathf.Floor((saveData.playTime % 3600f) / 60f);
                playtimeText.text = $"{hours}h {minutes}m";
            }

            if (lastPlayedText != null)
            {
                try
                {
                    DateTime saveDateTime = DateTime.Parse(saveData.saveDate);
                    lastPlayedText.text = saveDateTime.ToString("MMM dd, yyyy");
                }
                catch
                {
                    lastPlayedText.text = saveData.saveDate;
                }
            }

            if (deleteButton != null)
                deleteButton.gameObject.SetActive(true);
        }

        /// <summary>
        /// Handle slot button click
        /// </summary>
        private void OnSlotClicked()
        {
            if (UIAudioManager.Instance != null)
                UIAudioManager.Instance.PlayButtonClick();

            OnSlotSelected?.Invoke(slotIndex);
        }

        /// <summary>
        /// Handle delete button click - show confirmation dialog
        /// </summary>
        private void OnDeleteClicked()
        {
            if (UIAudioManager.Instance != null)
                UIAudioManager.Instance.PlayButtonClick();

            // Use shared confirmation dialog
            if (ConfirmationDialog.Instance != null)
            {
                ConfirmationDialog.Instance.ShowDeleteConfirmation(
                    characterName ?? $"Save Slot {slotIndex}",
                    OnConfirmDelete,
                    null // No special cancel action needed
                );
            }
            else
            {
                // Fallback if dialog not available
                Debug.LogWarning("[SaveSlotUI] ConfirmationDialog not found - deleting without confirmation");
                OnConfirmDelete();
            }
        }

        /// <summary>
        /// Confirm deletion callback
        /// </summary>
        private void OnConfirmDelete()
        {
            OnSlotDeleted?.Invoke(slotIndex);
        }
    }
}