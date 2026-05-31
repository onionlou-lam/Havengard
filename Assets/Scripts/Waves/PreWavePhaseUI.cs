using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Havengard.Items;
using Havengard.UI;

namespace Havengard.Waves
{
    /// <summary>
    /// UI panel for the pre-wave phase with all preparation options
    /// </summary>
    public class PreWavePhaseUI : MonoBehaviour
    {
        [Header("Main Panel (Upgrades/Building Only)")]
        [SerializeField] private GameObject phasePanel;
        [SerializeField] private GameObject minimizedPanel; // Button to reopen panel
        [SerializeField] private TextMeshProUGUI waveNumberText;

        [Header("HUD Display (Wave Control)")]
        [SerializeField] private GameObject hudWaveInfoPanel; // Parent container
        [SerializeField] private TextMeshProUGUI hudWaveNumberText;
        [SerializeField] private TextMeshProUGUI hudTimerText;
        [SerializeField] private Button hudStartWaveButton;
        [SerializeField] private TextMeshProUGUI hudStartWaveButtonText;

        [Header("Sections")]
        [SerializeField] private GameObject towerShopSection;
        [SerializeField] private GameObject itemUpgradeSection;
        [SerializeField] private GameObject skillSelectionSection;

        [Header("Panel Controls")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button reopenButton;

        [Header("Tower Shop")]
        [SerializeField] private Button purchaseTowerButton;
        [SerializeField] private Button upgradeTowerButton;
        [SerializeField] private TextMeshProUGUI goldAmountText;

        [Header("Item Upgrade")]
        [SerializeField] private Button upgradeItemButton;
        [SerializeField] private TextMeshProUGUI celestiumAmountText;
        [SerializeField] private GameObject itemUpgradeUIPanel;

        [Header("Skill Selection")]
        [SerializeField] private Button selectSkillButton;
        [SerializeField] private TextMeshProUGUI availableSkillPointsText;

        private PreWavePhase preWavePhase;
        private int currentWaveNumber;
        private bool isMinimized = false;
        private bool isInitialized = false;
        private bool isWaveActive = false;
        private bool isPrepPhaseActive = false;

        private void Awake()
        {
            Debug.Log("[PreWavePhaseUI] Awake - initializing buttons");

            EnsurePreWavePhaseReference();

            // Setup HUD start wave button
            if (hudStartWaveButton != null)
            {
                hudStartWaveButton.onClick.AddListener(OnStartWaveClicked);
                //Debug.Log("[PreWavePhaseUI] HUD Start Wave button hooked up");
            }
            else
            {
                Debug.LogWarning("[PreWavePhaseUI] hudStartWaveButton is NULL - check inspector assignment!");
            }

            // Setup panel control buttons
            if (closeButton != null)
                closeButton.onClick.AddListener(ClosePanel);

            if (reopenButton != null)
                reopenButton.onClick.AddListener(OpenPanel);

            // Setup upgrade/shop buttons
            if (purchaseTowerButton != null)
                purchaseTowerButton.onClick.AddListener(OnPurchaseTowerClicked);

            if (upgradeTowerButton != null)
                upgradeTowerButton.onClick.AddListener(OnUpgradeTowerClicked);

            if (upgradeItemButton != null)
                upgradeItemButton.onClick.AddListener(OnUpgradeItemClicked);

            if (selectSkillButton != null)
                selectSkillButton.onClick.AddListener(OnSelectSkillClicked);

            isInitialized = true;

            //Debug.Log($"[PreWavePhaseUI] Awake complete. PreWavePhase found: {preWavePhase != null}");
        }

        private void EnsurePreWavePhaseReference()
        {
            if (preWavePhase != null)
                return;

            preWavePhase = GetComponentInParent<PreWavePhase>();
            if (preWavePhase == null)
            {
                preWavePhase = FindFirstObjectByType<PreWavePhase>();
            }

            if (preWavePhase != null)
            {
                //Debug.Log($"[PreWavePhaseUI] Found PreWavePhase reference: {preWavePhase.gameObject.name}");
            }
            else
            {
                //Debug.LogError("[PreWavePhaseUI] Could not find PreWavePhase reference!");
            }
        }

        private void Start()
        {
            //Debug.Log($"[PreWavePhaseUI] Start - isPrepPhaseActive: {isPrepPhaseActive}");

            EnsurePreWavePhaseReference();

            // If prep phase is already active (ShowPhase was called before Start), don't hide anything!
            if (isPrepPhaseActive)
            {
                //Debug.Log("[PreWavePhaseUI] Prep phase already active - skipping initial hide");
                return; // CRITICAL: Don't hide anything if already showing
            }

            Debug.Log("[PreWavePhaseUI] Setting initial UI state (hiding everything)");

            // Hide prep panels initially
            if (phasePanel != null)
            {
                phasePanel.SetActive(false);
                //Debug.Log("[PreWavePhaseUI] Prep panel hidden");
            }

            // Hide minimized panel initially
            if (minimizedPanel != null)
            {
                minimizedPanel.SetActive(false);
                //Debug.Log("[PreWavePhaseUI] Minimized panel hidden");
            }

            // Setup HUD parent - MUST be enabled
            if (hudWaveInfoPanel != null)
            {
                if (!hudWaveInfoPanel.activeSelf)
                {
                    hudWaveInfoPanel.SetActive(true);
                    //Debug.Log("[PreWavePhaseUI] HUD parent panel was disabled - now enabled");
                }
                else
                {
                    //Debug.Log("[PreWavePhaseUI] HUD parent panel already enabled");
                }
            }
            else
            {
                Debug.LogError("[PreWavePhaseUI] hudWaveInfoPanel is NULL - assign in inspector!");
            }

            // Hide individual HUD elements initially
            HideHUDElements();

            //Debug.Log("[PreWavePhaseUI] Start complete - all UI hidden");
        }

        /// <summary>
        /// Hide all HUD children elements
        /// </summary>
        private void HideHUDElements()
        {
            if (hudWaveNumberText != null)
            {
                hudWaveNumberText.gameObject.SetActive(false);
                //Debug.Log("[PreWavePhaseUI] HUD wave number hidden");
            }

            if (hudTimerText != null)
            {
                hudTimerText.gameObject.SetActive(false);
                //Debug.Log("[PreWavePhaseUI] HUD timer hidden");
            }

            if (hudStartWaveButton != null)
            {
                hudStartWaveButton.gameObject.SetActive(false);
                //Debug.Log("[PreWavePhaseUI] HUD start wave button hidden");
            }
        }

        /// <summary>
        /// Show all HUD elements for prep phase
        /// </summary>
        private void ShowHUDElements()
        {
            Debug.Log("[PreWavePhaseUI] ShowHUDElements called");

            // Ensure parent is enabled
            if (hudWaveInfoPanel != null)
            {
                hudWaveInfoPanel.SetActive(true);
            }

            if (hudWaveNumberText != null)
            {
                hudWaveNumberText.gameObject.SetActive(true);
                hudWaveNumberText.text = $"Wave {currentWaveNumber}";
                //Debug.Log($"[PreWavePhaseUI] HUD wave number shown: {hudWaveNumberText.text}");
            }
            else
            {
                Debug.LogError("[PreWavePhaseUI] hudWaveNumberText is NULL!");
            }

            if (hudTimerText != null)
            {
                hudTimerText.gameObject.SetActive(true);
                //Debug.Log("[PreWavePhaseUI] HUD timer shown");
            }
            else
            {
                Debug.LogError("[PreWavePhaseUI] hudTimerText is NULL!");
            }

            if (hudStartWaveButton != null)
            {
                hudStartWaveButton.gameObject.SetActive(true);
                if (hudStartWaveButtonText != null)
                {
                    hudStartWaveButtonText.gameObject.SetActive(true); // FIXED: Explicitly re-enable text
                    hudStartWaveButtonText.text = "Start Wave";
                }
                //Debug.Log("[PreWavePhaseUI] HUD start wave button shown");
            }
            else
            {
                Debug.LogError("[PreWavePhaseUI] hudStartWaveButton is NULL!");
            }
        }

        /// <summary>
        /// Show the pre-wave phase - opens prep panel and shows HUD wave control
        /// </summary>
        public void ShowPhase(int waveNumber, bool hasTimeLimit, float timeLimit)
        {
            currentWaveNumber = waveNumber;
            isMinimized = false;
            isWaveActive = false;
            isPrepPhaseActive = true;

            EnsurePreWavePhaseReference();

           // Debug.Log($"[PreWavePhaseUI] ========== ShowPhase START for Wave {waveNumber} ==========");
           // Debug.Log($"[PreWavePhaseUI] isPrepPhaseActive: {isPrepPhaseActive}, hasTimeLimit: {hasTimeLimit}, timeLimit: {timeLimit}s");

            // Show the preparation panel
            if (phasePanel != null)
            {
                phasePanel.SetActive(true);
                //Debug.Log("[PreWavePhaseUI] ✓ Prep panel activated");
            }
            else
            {
                Debug.LogError("[PreWavePhaseUI] ✗ phasePanel is NULL!");
            }

            // Hide minimized panel
            if (minimizedPanel != null)
            {
                minimizedPanel.SetActive(false);
                //Debug.Log("[PreWavePhaseUI] ✓ Minimized panel hidden");
            }

            // Show ALL HUD elements
            ShowHUDElements();

            // Initialize timer if enabled
            if (hasTimeLimit && hudTimerText != null)
            {
                string initialTime = FormatTime(timeLimit);
                hudTimerText.text = initialTime;
                //Debug.Log($"[PreWavePhaseUI] ✓ Timer initialized to: {initialTime}");
            }

            // Notify UICanvasManager
            if (UICanvasManager.Instance != null)
                UICanvasManager.Instance.ShowPreWavePhase();

            // Update prep panel wave text
            if (waveNumberText != null)
                waveNumberText.text = $"Wave {waveNumber} Preparation";

            UpdateResourceDisplays();

            //Debug.Log($"[PreWavePhaseUI] ========== ShowPhase COMPLETE ==========");
        }

        /// <summary>
        /// Close the preparation panel - HUD timer continues
        /// </summary>
        private void ClosePanel()
        {
            //Debug.Log("[PreWavePhaseUI] ClosePanel called - prep phase STAYS ACTIVE");

            // Hide the main panel
            if (phasePanel != null)
                phasePanel.SetActive(false);

            // Show minimized button (so player can reopen)
            if (minimizedPanel != null)
            {
                minimizedPanel.SetActive(true);
                //Debug.Log("[PreWavePhaseUI] ✓ Minimized panel button shown");
            }
            else
            {
                Debug.LogError("[PreWavePhaseUI] ✗ minimizedPanel is NULL - assign in inspector!");
            }

            // HUD stays visible with timer counting down
            //Debug.Log($"[PreWavePhaseUI] Panel closed. isPrepPhaseActive: {isPrepPhaseActive}");
        }

        /// <summary>
        /// Open the preparation panel again
        /// </summary>
        private void OpenPanel()
        {
            //Debug.Log("[PreWavePhaseUI] OpenPanel called");

            if (phasePanel != null)
                phasePanel.SetActive(true);

            if (minimizedPanel != null)
                minimizedPanel.SetActive(false);

            UpdateResourceDisplays();

            //Debug.Log("[PreWavePhaseUI] Panel reopened");
        }

        /// <summary>
        /// Called when the wave actually starts - hides all prep UI
        /// </summary>
        public void OnWaveStarted()
        {
            Debug.Log("[PreWavePhaseUI] ========== OnWaveStarted ==========");
            
            isPrepPhaseActive = false;
            isWaveActive = true;

            // Hide all prep panels
            if (phasePanel != null)
                phasePanel.SetActive(false);

            if (minimizedPanel != null)
                minimizedPanel.SetActive(false);

            // Hide timer and button (countdown is over)
            if (hudStartWaveButton != null)
            {
                hudStartWaveButton.gameObject.SetActive(false);
                // FIXED: Removed redundant text disable - parent button disable is sufficient
                //Debug.Log("[PreWavePhaseUI] ✓ HUD button hidden");
            }

            if (hudTimerText != null)
            {
                hudTimerText.gameObject.SetActive(false);
                //Debug.Log("[PreWavePhaseUI] ✓ HUD timer hidden");
            }

            // KEEP wave number visible during the entire wave
            if (hudWaveNumberText != null)
            {
                hudWaveNumberText.gameObject.SetActive(true);
                hudWaveNumberText.text = $"Wave {currentWaveNumber}";
                //Debug.Log($"[PreWavePhaseUI] ✓ Wave number kept visible: {hudWaveNumberText.text}");
            }

            // KEEP parent panel active so wave number stays visible
            if (hudWaveInfoPanel != null)
            {
                hudWaveInfoPanel.SetActive(true);
                //Debug.Log("[PreWavePhaseUI] ✓ HUD parent stays enabled for wave number");
            }

            Debug.Log($"[PreWavePhaseUI] isPrepPhaseActive: {isPrepPhaseActive}, isWaveActive: {isWaveActive}");
        }

        /// <summary>
        /// Called when wave completes - hides HUD info
        /// </summary>
        public void OnWaveCompleted()
        {
            //Debug.Log("[PreWavePhaseUI] ========== OnWaveCompleted ==========");
            
            isWaveActive = false;
            isPrepPhaseActive = false;

            // Now hide wave number (wave is over)
            if (hudWaveNumberText != null)
            {
                hudWaveNumberText.gameObject.SetActive(false);
                //Debug.Log("[PreWavePhaseUI] ✓ Wave number hidden (wave complete)");
            }

            // Keep other elements hidden
            if (hudTimerText != null)
                hudTimerText.gameObject.SetActive(false);

            if (hudStartWaveButton != null)
            {
                hudStartWaveButton.gameObject.SetActive(false);
                // FIXED: Removed redundant text disable - parent button disable is sufficient
            }

            // Keep parent enabled for next wave prep phase
            // (Don't disable hudWaveInfoPanel - it will be reused)

            //Debug.Log("[PreWavePhaseUI] Wave completed, wave number cleared");
        }

        // This method is now only for when phase is cancelled or game transitions
        // NOT for when wave starts
        public void HidePhase()
        {
            //Debug.Log("[PreWavePhaseUI] HidePhase called - FULL HIDE (not normal wave start)");
            
            isPrepPhaseActive = false;
            isWaveActive = false; // Reset wave active too

            if (phasePanel != null)
                phasePanel.SetActive(false);

            if (minimizedPanel != null)
                minimizedPanel.SetActive(false);

            // Hide ALL HUD elements (complete shutdown)
            HideHUDElements();

            if (UICanvasManager.Instance != null)
                UICanvasManager.Instance.HidePreWavePhase();
        }

        private string FormatTime(float timeInSeconds)
        {
            int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
            return $"{minutes:00}:{seconds:00}";
        }

        public void UpdateTimer(float remainingTime)
        {
            if (hudTimerText != null && isPrepPhaseActive)
            {
                string timeString = FormatTime(remainingTime);
                hudTimerText.text = timeString;
                // Don't need to SetActive here, already active from ShowHUDElements
            }
        }

        public void UpdateTimeLimitToggle(bool enabled)
        {
            if (hudTimerText != null && isPrepPhaseActive)
            {
                hudTimerText.gameObject.SetActive(enabled);
            }
        }

        private void UpdateResourceDisplays()
        {
            if (goldAmountText != null)
            {
                goldAmountText.text = $"Gold: {GetPlayerGold()}";
            }

            if (celestiumAmountText != null)
            {
                celestiumAmountText.text = $"Celestium: {GetPlayerCelestium()}";
            }

            if (availableSkillPointsText != null)
            {
                availableSkillPointsText.text = $"Skill Points: {GetAvailableSkillPoints()}";
            }
        }

        private void OnStartWaveClicked()
        {
            //Debug.Log("[PreWavePhaseUI] ========== START WAVE CLICKED ==========");
            
            EnsurePreWavePhaseReference();
            
            // Don't call OnWaveStarted here - PreWavePhase.EndPhase() will call it
            // OnWaveStarted(); ← REMOVE THIS LINE
            
            if (preWavePhase != null)
            {
                preWavePhase.ManuallyStartWave(); // This calls EndPhase() which calls OnWaveStarted()
            }
            else
            {
                Debug.LogWarning("[PreWavePhaseUI] PreWavePhase reference is null!");
            }
        }

        private void OnPurchaseTowerClicked()
        {
            Debug.Log("[PreWavePhaseUI] Purchase Tower clicked");
            Debug.Log($"[DEBUG] Tower purchase - Gold: {GetPlayerGold()}");
        }

        private void OnUpgradeTowerClicked()
        {
            Debug.Log("[PreWavePhaseUI] Upgrade Tower clicked");
            Debug.Log($"[DEBUG] Tower upgrade - Gold: {GetPlayerGold()}");
        }

        private void OnUpgradeItemClicked()
        {
            Debug.Log("[PreWavePhaseUI] Upgrade Item clicked");
            Debug.Log($"[DEBUG] Item upgrade - Celestium: {GetPlayerCelestium()}");

            if (itemUpgradeUIPanel != null)
            {
                bool isActive = itemUpgradeUIPanel.activeSelf;
                itemUpgradeUIPanel.SetActive(!isActive);

                if (!isActive)
                {
                    var itemUpgradeUI = itemUpgradeUIPanel.GetComponent<ItemUpgradeUI>();
                    if (itemUpgradeUI != null)
                    {
                        ItemInventory playerInventory = GetPlayerInventory();
                        itemUpgradeUI.Show(playerInventory);
                    }
                }
            }
        }

        private void OnSelectSkillClicked()
        {
            Debug.Log("[PreWavePhaseUI] Select Skill clicked");
            Debug.Log($"[DEBUG] Skill selection - Points: {GetAvailableSkillPoints()}");
        }

        private int GetPlayerGold()
        {
            if (Havengard.Resources.GoldSystem.Instance != null)
                return Havengard.Resources.GoldSystem.Instance.Current;
            return 1000;
        }

        private int GetPlayerCelestium()
        {
            if (Havengard.Resources.CelestiumSystem.Instance != null)
                return Havengard.Resources.CelestiumSystem.Instance.Current;
            return 0;
        }

        private int GetAvailableSkillPoints()
        {
            return 3;
        }

        private ItemInventory GetPlayerInventory()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                return player.GetComponent<ItemInventory>();
            return null;
        }

        private void Update()
        {
            if (isPrepPhaseActive && preWavePhase != null)
            {
                if (preWavePhase.UseTimeLimit && preWavePhase.IsPhaseActive)
                {
                    UpdateTimer(preWavePhase.RemainingTime);
                }
            }
        }
    }
}