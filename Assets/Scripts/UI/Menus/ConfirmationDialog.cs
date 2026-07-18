using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Havengard.Audio;
using System;

namespace Havengard.UI
{
    /// <summary>
    /// Reusable confirmation dialog for delete operations and other confirmations
    /// </summary>
    public class ConfirmationDialog : MonoBehaviour
    {
        public static ConfirmationDialog Instance { get; private set; }

        [Header("Panel")]
        [SerializeField] private GameObject dialogPanel;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private TextMeshProUGUI titleText;

        [Header("Buttons")]
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private TextMeshProUGUI confirmButtonText;
        [SerializeField] private TextMeshProUGUI cancelButtonText;

        // Callbacks
        private Action onConfirm;
        private Action onCancel;

        private void Awake()
        {
            // Singleton pattern (don't use DontDestroyOnLoad for scene-specific dialogs)
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(OnConfirmClicked);
                AddButtonSounds(confirmButton);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(OnCancelClicked);
                AddButtonSounds(cancelButton);
            }

            // Hide by default
            Hide();
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

            var hoverEntry = new EventTrigger.Entry();
            hoverEntry.eventID = EventTriggerType.PointerEnter;
            hoverEntry.callback.AddListener((data) => {
                if (UIAudioManager.Instance != null && button.interactable)
                    UIAudioManager.Instance.PlayButtonHover();
            });
            trigger.triggers.Add(hoverEntry);
        }

        /// <summary>
        /// Show confirmation dialog with custom message
        /// </summary>
        public void Show(string message, Action onConfirmCallback, Action onCancelCallback = null,
                         string title = "Confirm", string confirmText = "Yes", string cancelText = "No")
        {
            if (messageText != null)
                messageText.text = message;

            if (titleText != null)
                titleText.text = title;

            if (confirmButtonText != null)
                confirmButtonText.text = confirmText;

            if (cancelButtonText != null)
                cancelButtonText.text = cancelText;

            onConfirm = onConfirmCallback;
            onCancel = onCancelCallback;

            if (dialogPanel != null)
                dialogPanel.SetActive(true);

            if (UIAudioManager.Instance != null)
                UIAudioManager.Instance.PlayPanelOpen();
        }

        /// <summary>
        /// Show delete confirmation dialog
        /// </summary>
        public void ShowDeleteConfirmation(string itemName, Action onConfirmCallback, Action onCancelCallback = null)
        {
            string message = $"Are you sure you want to delete '{itemName}'?\n\nThis action cannot be undone.";
            Show(message, onConfirmCallback, onCancelCallback, "Delete Save", "Delete", "Cancel");
        }

        /// <summary>
        /// Hide the dialog
        /// </summary>
        public void Hide()
        {
            if (dialogPanel != null)
                dialogPanel.SetActive(false);

            onConfirm = null;
            onCancel = null;

            if (UIAudioManager.Instance != null)
                UIAudioManager.Instance.PlayPanelClose();
        }

        /// <summary>
        /// Handle confirm button click
        /// </summary>
        private void OnConfirmClicked()
        {
            if (UIAudioManager.Instance != null)
                UIAudioManager.Instance.PlayButtonClick();

            onConfirm?.Invoke();
            Hide();
        }

        /// <summary>
        /// Handle cancel button click
        /// </summary>
        private void OnCancelClicked()
        {
            if (UIAudioManager.Instance != null)
                UIAudioManager.Instance.PlayButtonClick();

            onCancel?.Invoke();
            Hide();
        }
    }
}