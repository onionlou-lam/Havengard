using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Havengard.Save;
using Havengard.Audio;

namespace Havengard.UI
{
    /// <summary>
    /// Manages the load game panel with save slots
    /// </summary>
    public class SaveSlotPanelUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MainMenuUI mainMenuUI;
        [SerializeField] private SaveSlotUI[] saveSlots;
        [SerializeField] private Button backButton;

        [Header("Settings")]
        [SerializeField] private string gameSceneName = "GameScene";

        private void Start()
        {
            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBackClicked);
                AddButtonSounds(backButton);
            }

            // Find main menu UI if not assigned
            if (mainMenuUI == null)
                mainMenuUI = GetComponentInParent<MainMenuUI>();

            // Subscribe to slot events
            foreach (SaveSlotUI slot in saveSlots)
            {
                if (slot != null)
                {
                    slot.OnSlotSelected += OnSlotSelected;
                    slot.OnSlotDeleted += OnSlotDeleted;
                }
            }
        }

        private void OnEnable()
        {
            RefreshSlots();
        }

        private void OnDestroy()
        {
            // Unsubscribe from slot events
            foreach (SaveSlotUI slot in saveSlots)
            {
                if (slot != null)
                {
                    slot.OnSlotSelected -= OnSlotSelected;
                    slot.OnSlotDeleted -= OnSlotDeleted;
                }
            }
        }

        /// <summary>
        /// Add UI sounds to a button
        /// </summary>
        private void AddButtonSounds(Button button)
        {
            if (button == null) return;

            var trigger = button.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (trigger == null)
                trigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

            var hoverEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
            hoverEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
            hoverEntry.callback.AddListener((data) => {
                if (UIAudioManager.Instance != null && button.interactable)
                    UIAudioManager.Instance.PlayButtonHover();
            });
            trigger.triggers.Add(hoverEntry);
        }

        /// <summary>
        /// Refresh all save slots with current data
        /// </summary>
        public void RefreshSlots()
        {
            for (int i = 0; i < saveSlots.Length; i++)
            {
                if (saveSlots[i] != null)
                {
                    int slotIndex = i + 1;
                    GameSaveData saveData = SaveUtility.LoadFromFile<GameSaveData>($"SaveSlot{slotIndex}");
                    saveSlots[i].Initialize(slotIndex, saveData);
                }
            }
        }

        /// <summary>
        /// Handle slot selection - load the game
        /// </summary>
        private void OnSlotSelected(int slotIndex)
        {
            Debug.Log($"[SaveSlotPanelUI] Slot {slotIndex} selected");

            // Check if slot has data
            if (!SaveUtility.SaveFileExists($"SaveSlot{slotIndex}"))
            {
                Debug.LogWarning($"[SaveSlotPanelUI] Slot {slotIndex} is empty!");

                if (UIAudioManager.Instance != null)
                    UIAudioManager.Instance.PlayError();

                return;
            }

            // Set active slot and load game
            if (SaveSlotManager.Instance != null)
                SaveSlotManager.Instance.SetActiveSlot(slotIndex);

            if (UIAudioManager.Instance != null)
                UIAudioManager.Instance.PlaySuccess();

            SceneManager.LoadScene(gameSceneName);
        }

        /// <summary>
        /// Handle slot deletion
        /// </summary>
        private void OnSlotDeleted(int slotIndex)
        {
            Debug.Log($"[SaveSlotPanelUI] Delete slot {slotIndex}");

            if (SaveUtility.DeleteSaveFile($"SaveSlot{slotIndex}"))
            {
                Debug.Log($"[SaveSlotPanelUI] Slot {slotIndex} deleted");

                if (UIAudioManager.Instance != null)
                    UIAudioManager.Instance.PlaySuccess();

                RefreshSlots();
            }
            else
            {
                Debug.LogError($"[SaveSlotPanelUI] Failed to delete slot {slotIndex}");

                if (UIAudioManager.Instance != null)
                    UIAudioManager.Instance.PlayError();
            }
        }

        /// <summary>
        /// Handle back button click
        /// </summary>
        private void OnBackClicked()
        {
            if (UIAudioManager.Instance != null)
                UIAudioManager.Instance.PlayButtonClick();

            if (mainMenuUI != null)
                mainMenuUI.ShowMainMenu();
        }
    }
}