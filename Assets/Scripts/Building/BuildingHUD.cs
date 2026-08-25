using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Havengard.Resources;

namespace Havengard.Building
{
    /// <summary>
    /// Main UI panel for building mode
    /// Contains tower selection, context panel, and building controls
    /// </summary>
    public class BuildingHUD : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private TowerSelectionPanel towerSelectionPanel;
        [SerializeField] private TowerContextPanel towerContextPanel;

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private TextMeshProUGUI waveInvestmentText;
        [SerializeField] private TextMeshProUGUI totalInvestmentText;

        [Header("Control Buttons")]
        [SerializeField] private Button undoButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private Button exitButton;

        [Header("Labels")]
        [SerializeField] private TextMeshProUGUI headerText;

        private GameObject selectedTower;

        private void Awake()
        {
            // Auto-find panels
            if (towerSelectionPanel == null)
                towerSelectionPanel = GetComponentInChildren<TowerSelectionPanel>();

            if (towerContextPanel == null)
                towerContextPanel = GetComponentInChildren<TowerContextPanel>();

            // Setup button listeners
            if (undoButton != null)
                undoButton.onClick.AddListener(OnUndoClicked);

            if (resetButton != null)
                resetButton.onClick.AddListener(OnResetClicked);

            if (exitButton != null)
                exitButton.onClick.AddListener(OnExitClicked);
        }

        private void Update()
        {
            // Update investment displays periodically
            if (mainPanel != null && mainPanel.activeSelf)
            {
                UpdateInvestmentDisplays();
            }
        }

        public void Show()
        {
            if (mainPanel != null)
                mainPanel.SetActive(true);

            if (headerText != null)
                headerText.text = "BUILDING MODE";

            UpdateGoldDisplay();
            UpdateInvestmentDisplays();

            // Show tower selection panel
            if (towerSelectionPanel != null)
                towerSelectionPanel.gameObject.SetActive(true);

            // Hide context panel initially
            if (towerContextPanel != null)
                towerContextPanel.gameObject.SetActive(false);

            UpdateButtonStates();
        }

        public void Hide()
        {
            if (mainPanel != null)
                mainPanel.SetActive(false);

            DeselectTower();
        }

        public void UpdateGoldDisplay()
        {
            if (goldText != null && GoldSystem.Instance != null)
            {
                goldText.text = $"Gold: {GoldSystem.Instance.Current}";
            }
        }

        private void UpdateInvestmentDisplays()
        {
            var controller = BuildingModeController.Instance;
            if (controller == null)
                return;

            if (waveInvestmentText != null)
            {
                int waveInvestment = controller.GetCurrentPhaseInvestment();
                waveInvestmentText.text = $"Wave Investment: {waveInvestment}g";
            }

            if (totalInvestmentText != null)
            {
                int totalInvestment = controller.GetTotalInvestment();
                totalInvestmentText.text = $"Total Investment: {totalInvestment}g";
            }
        }

        public void ShowTowerContextPanel(GameObject tower)
        {
            selectedTower = tower;

            if (towerContextPanel != null)
            {
                towerContextPanel.gameObject.SetActive(true);
                towerContextPanel.ShowTowerInfo(tower);
            }

            // Hide tower selection
            if (towerSelectionPanel != null)
                towerSelectionPanel.gameObject.SetActive(false);

            UpdateButtonStates();
        }

        public void DeselectTower()
        {
            selectedTower = null;

            if (towerContextPanel != null)
                towerContextPanel.gameObject.SetActive(false);

            // Show tower selection again
            if (towerSelectionPanel != null)
                towerSelectionPanel.gameObject.SetActive(true);

            UpdateButtonStates();
        }

        public void RefreshTowerContextPanel(GameObject tower)
        {
            if (towerContextPanel != null && towerContextPanel.gameObject.activeSelf)
            {
                towerContextPanel.ShowTowerInfo(tower);
            }

            UpdateGoldDisplay();
            UpdateInvestmentDisplays();
        }

        private void UpdateButtonStates()
        {
            var controller = BuildingModeController.Instance;
            if (controller == null)
                return;

            // Update undo button
            if (undoButton != null)
            {
                undoButton.interactable = controller.CanUndo();
            }

            // Update reset button
            if (resetButton != null)
            {
                resetButton.interactable = controller.GetCurrentPhaseTowerCount() > 0;
            }
        }

        #region Button Handlers

        private void OnUndoClicked()
        {
            var controller = BuildingModeController.Instance;
            if (controller != null)
            {
                controller.Undo();
                UpdateGoldDisplay();
                UpdateInvestmentDisplays();
                DeselectTower();
            }
        }

        private void OnResetClicked()
        {
            var controller = BuildingModeController.Instance;
            if (controller != null)
            {
                // Show confirmation dialog first
                ShowResetConfirmation();
            }
        }

        private void OnExitClicked()
        {
            var controller = BuildingModeController.Instance;
            if (controller != null)
            {
                controller.ExitBuildingMode();
            }
        }

        private void ShowResetConfirmation()
        {
            // TODO: Integrate with existing ConfirmationDialog system
            // For now, just execute directly
            var controller = BuildingModeController.Instance;
            if (controller != null)
            {
                controller.ResetCurrentPhase();
                UpdateGoldDisplay();
                UpdateInvestmentDisplays();
                DeselectTower();
            }
        }

        #endregion
    }
}