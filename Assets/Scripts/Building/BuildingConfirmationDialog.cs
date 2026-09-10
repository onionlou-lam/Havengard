using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Havengard.Building
{
    /// <summary>
    /// Confirmation dialog for building mode actions (sell, reset, etc.)
    /// </summary>
    public class BuildingConfirmationDialog : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject dialogPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private TextMeshProUGUI detailsText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private TextMeshProUGUI confirmButtonText;
        [SerializeField] private TextMeshProUGUI cancelButtonText;

        [Header("Colors")]
        [SerializeField] private Color warningColor = new Color(1f, 0.5f, 0f);
        [SerializeField] private Color dangerColor = Color.red;

        private Action onConfirmAction;
        private Action onCancelAction;

        private void Awake()
        {
            // Setup button listeners
            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnConfirmClicked);

            if (cancelButton != null)
                cancelButton.onClick.AddListener(OnCancelClicked);

            // Initially hide
            Hide();
        }

        /// <summary>
        /// Show confirmation dialog for selling a tower
        /// </summary>
        public void ShowSellConfirmation(string towerName, int refundAmount, Action onConfirm, Action onCancel = null)
        {
            ShowDialog(
                "Sell Tower",
                $"Are you sure you want to sell this {towerName}?",
                $"Refund: {refundAmount} Gold",
                "SELL",
                "CANCEL",
                onConfirm,
                onCancel,
                dangerColor
            );
        }

        /// <summary>
        /// Show confirmation dialog for resetting current phase
        /// </summary>
        public void ShowResetConfirmation(int towerCount, int refundAmount, Action onConfirm, Action onCancel = null)
        {
            ShowDialog(
                "Reset Current Phase",
                "This will remove all towers placed during this defence phase.",
                $"Towers to remove: {towerCount}\nTotal refund: {refundAmount} Gold",
                "RESET",
                "CANCEL",
                onConfirm,
                onCancel,
                warningColor
            );
        }

        /// <summary>
        /// Show generic confirmation dialog
        /// </summary>
        public void ShowDialog(
            string title,
            string message,
            string details,
            string confirmText,
            string cancelText,
            Action onConfirm,
            Action onCancel = null,
            Color? titleColor = null)
        {
            // Set title
            if (titleText != null)
            {
                titleText.text = title;
                if (titleColor.HasValue)
                    titleText.color = titleColor.Value;
            }

            // Set message
            if (messageText != null)
                messageText.text = message;

            // Set details
            if (detailsText != null)
            {
                detailsText.text = details;
                detailsText.gameObject.SetActive(!string.IsNullOrEmpty(details));
            }

            // Set button text
            if (confirmButtonText != null)
                confirmButtonText.text = confirmText;

            if (cancelButtonText != null)
                cancelButtonText.text = cancelText;

            // Store callbacks
            onConfirmAction = onConfirm;
            onCancelAction = onCancel;

            // Show dialog
            Show();
        }

        private void Show()
        {
            if (dialogPanel != null)
                dialogPanel.SetActive(true);

            Debug.Log("[BuildingConfirmationDialog] Dialog shown");
        }

        public void Hide()
        {
            if (dialogPanel != null)
                dialogPanel.SetActive(false);

            onConfirmAction = null;
            onCancelAction = null;

            Debug.Log("[BuildingConfirmationDialog] Dialog hidden");
        }

        private void OnConfirmClicked()
        {
            Debug.Log("[BuildingConfirmationDialog] Confirm clicked");

            onConfirmAction?.Invoke();
            Hide();
        }

        private void OnCancelClicked()
        {
            Debug.Log("[BuildingConfirmationDialog] Cancel clicked");

            onCancelAction?.Invoke();
            Hide();
        }

        private void Update()
        {
            // Allow ESC to cancel
            if (dialogPanel != null && dialogPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            {
                OnCancelClicked();
            }
        }
    }
}