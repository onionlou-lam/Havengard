using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Havengard.Resources;
using Havengard.Audio;
using System.Collections;

namespace Havengard.Building
{
    /// <summary>
    /// Main UI panel for building mode
    /// Contains tower selection, context panel, and building controls
    /// Visual polish (pulses/flashes) is handled locally.
    /// Generic UI chrome audio (clicks, panel open/close) uses UIAudioManager.
    /// Domain-specific gameplay audio (placement, upgrade, sell, undo, insufficient gold)
    /// is owned by BuildingModeController via BuildingAudioManager.
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

        [Header("Dialogs")]
        [SerializeField] private BuildingConfirmationDialog confirmationDialog;

        [Header("Feedback Settings")]
        [SerializeField] private float uiPulseScale = 1.08f;
        [SerializeField] private float uiPulseDuration = 0.14f;
        [SerializeField] private Color textFlashColor = new Color(1f, 0.9f, 0.4f);
        [SerializeField] private float textFlashDuration = 0.25f;

        private GameObject selectedTower;

        // Track last shown investment values so we can flash on change
        private int lastWaveInvestment = -1;
        private int lastTotalInvestment = -1;

        private void Awake()
        {
            if (towerSelectionPanel == null)
                towerSelectionPanel = GetComponentInChildren<TowerSelectionPanel>();

            if (towerContextPanel == null)
                towerContextPanel = GetComponentInChildren<TowerContextPanel>();

            if (undoButton != null)
                undoButton.onClick.AddListener(OnUndoClicked);

            if (resetButton != null)
                resetButton.onClick.AddListener(OnResetClicked);

            if (exitButton != null)
                exitButton.onClick.AddListener(OnExitClicked);
        }

        private void Update()
        {
            if (mainPanel != null && mainPanel.activeSelf)
            {
                UpdateInvestmentDisplays();
                UpdateButtonStates();
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

            if (towerSelectionPanel != null)
                towerSelectionPanel.gameObject.SetActive(true);

            if (towerContextPanel != null)
                towerContextPanel.gameObject.SetActive(false);

            UpdateButtonStates();

            // Generic UI chrome feedback for opening panel
            if (UIAudioManager.Instance != null)
                UIAudioManager.Instance.PlayPanelOpen();

            if (mainPanel != null)
                StartCoroutine(PulseTransformOnce(mainPanel.transform, uiPulseScale, uiPulseDuration));
        }

        public void Hide()
        {
            if (mainPanel != null)
                mainPanel.SetActive(false);

            DeselectTower();

            // Generic UI chrome feedback for closing panel
            if (UIAudioManager.Instance != null)
                UIAudioManager.Instance.PlayPanelClose();
        }

        public void UpdateGoldDisplay()
        {
            if (goldText != null && GoldSystem.Instance != null)
            {
                goldText.text = $"{GoldSystem.Instance.Current}";
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
                // Visual flash on change (audio is triggered by the domain action itself)
                if (waveInvestment != lastWaveInvestment && lastWaveInvestment != -1)
                {
                    StartCoroutine(FlashText(waveInvestmentText, textFlashColor, textFlashDuration));
                }

                waveInvestmentText.text = $"Wave Investment: {waveInvestment}g";
                lastWaveInvestment = waveInvestment;
            }

            if (totalInvestmentText != null)
            {
                int totalInvestment = controller.GetTotalInvestment();
                // Visual flash on change (audio is triggered by the domain action itself)
                if (totalInvestment != lastTotalInvestment && lastTotalInvestment != -1)
                {
                    StartCoroutine(FlashText(totalInvestmentText, textFlashColor, textFlashDuration));
                }

                totalInvestmentText.text = $"Total Investment: {totalInvestment}g";
                lastTotalInvestment = totalInvestment;
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

            if (towerSelectionPanel != null)
                towerSelectionPanel.gameObject.SetActive(false);

            UpdateButtonStates();

            // Generic UI chrome feedback
            if (UIAudioManager.Instance != null)
                UIAudioManager.Instance.PlayPanelOpen();

            if (towerContextPanel != null)
                StartCoroutine(PulseTransformOnce(towerContextPanel.transform, uiPulseScale, uiPulseDuration));
        }

        public void DeselectTower()
        {
            selectedTower = null;

            if (towerContextPanel != null)
                towerContextPanel.gameObject.SetActive(false);

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

            if (undoButton != null)
                undoButton.interactable = controller.CanUndo();

            if (resetButton != null)
                resetButton.interactable = controller.GetCurrentPhaseTowerCount() > 0;

            // Visual indicator: pulse interactables slightly when they become enabled
            if (undoButton != null && undoButton.interactable)
                StartCoroutine(PulseTransformOnce(undoButton.transform, uiPulseScale, uiPulseDuration));

            if (resetButton != null && resetButton.interactable)
                StartCoroutine(PulseTransformOnce(resetButton.transform, uiPulseScale, uiPulseDuration));
        }

        #region Button Handlers

        private void OnUndoClicked()
        {
            // Generic click chrome + pulse
            if (UIAudioManager.Instance != null)
                UIAudioManager.Instance.PlayButtonClick();

            if (undoButton != null)
                StartCoroutine(PulseTransformOnce(undoButton.transform, uiPulseScale, uiPulseDuration));

            var controller = BuildingModeController.Instance;
            if (controller != null)
            {
                if (!controller.CanUndo())
                    return;

                controller.Undo();
                UpdateGoldDisplay();
                UpdateInvestmentDisplays();
                DeselectTower();

                // Domain audio (PlayUndo) is triggered inside controller.Undo()

                if (goldText != null)
                    StartCoroutine(FlashText(goldText, textFlashColor, textFlashDuration));
            }
        }

        private void OnResetClicked()
        {
            if (UIAudioManager.Instance != null)
                UIAudioManager.Instance.PlayButtonClick();

            if (resetButton != null)
                StartCoroutine(PulseTransformOnce(resetButton.transform, uiPulseScale, uiPulseDuration));

            var controller = BuildingModeController.Instance;
            if (controller != null)
            {
                ShowResetConfirmation();
            }
        }

        private void OnExitClicked()
        {
            if (UIAudioManager.Instance != null)
                UIAudioManager.Instance.PlayButtonClick();

            if (exitButton != null)
                StartCoroutine(PulseTransformOnce(exitButton.transform, uiPulseScale, uiPulseDuration));

            var controller = BuildingModeController.Instance;
            if (controller != null)
            {
                controller.ExitBuildingMode();
            }

            // Domain audio (PlayExitBuildingMode) is triggered inside controller.ExitBuildingMode()
        }

        private void ShowResetConfirmation()
        {
            var controller = BuildingModeController.Instance;
            if (controller == null)
                return;

            int towerCount = controller.GetCurrentPhaseTowerCount();
            int refundAmount = controller.GetCurrentPhaseInvestment();

            if (towerCount == 0)
                return;

            if (confirmationDialog != null)
            {
                confirmationDialog.ShowResetConfirmation(
                    towerCount,
                    refundAmount,
                    onConfirm: () =>
                    {
                        controller.ResetCurrentPhase();
                        UpdateGoldDisplay();
                        UpdateInvestmentDisplays();
                        DeselectTower();

                        // Domain audio (PlayReset) is triggered inside controller.ResetCurrentPhase()

                        if (goldText != null)
                            StartCoroutine(FlashText(goldText, textFlashColor, textFlashDuration));
                    }
                );
            }
            else
            {
                controller.ResetCurrentPhase();
                UpdateGoldDisplay();
                UpdateInvestmentDisplays();
                DeselectTower();

                if (goldText != null)
                    StartCoroutine(FlashText(goldText, textFlashColor, textFlashDuration));
            }
        }

        #endregion

        #region Visual Feedback Coroutines

        private IEnumerator PulseTransformOnce(Transform target, float scale, float duration)
        {
            if (target == null || duration <= 0f)
                yield break;

            Vector3 original = target.localScale;
            Vector3 targetScale = original * scale;
            float half = duration * 0.5f;
            float t = 0f;

            // Scale up
            while (t < half)
            {
                t += Time.deltaTime;
                float f = Mathf.Clamp01(t / half);
                target.localScale = Vector3.Lerp(original, targetScale, Mathf.SmoothStep(0f, 1f, f));
                yield return null;
            }

            // Scale down
            t = 0f;
            while (t < half)
            {
                t += Time.deltaTime;
                float f = Mathf.Clamp01(t / half);
                target.localScale = Vector3.Lerp(targetScale, original, Mathf.SmoothStep(0f, 1f, f));
                yield return null;
            }

            target.localScale = original;
        }

        private IEnumerator FlashText(TextMeshProUGUI text, Color flashColor, float duration)
        {
            if (text == null || duration <= 0f)
                yield break;

            Color original = text.color;
            float half = duration * 0.5f;
            float t = 0f;

            // Lerp to flash color
            while (t < half)
            {
                t += Time.deltaTime;
                float f = Mathf.Clamp01(t / half);
                text.color = Color.Lerp(original, flashColor, f);
                yield return null;
            }

            // Lerp back
            t = 0f;
            while (t < half)
            {
                t += Time.deltaTime;
                float f = Mathf.Clamp01(t / half);
                text.color = Color.Lerp(flashColor, original, f);
                yield return null;
            }

            text.color = original;
        }

        #endregion
    }
}