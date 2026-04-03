using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Havengard.Items;
using Havengard.UI; // ADD THIS - for ItemUpgradeUI

namespace Havengard.Waves
{
    /// <summary>
    /// UI panel for the pre-wave phase with all preparation options
    /// </summary>
    public class PreWavePhaseUI : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject phasePanel;
        [SerializeField] private TextMeshProUGUI waveNumberText;
        [SerializeField] private TextMeshProUGUI timerText;

        [Header("Sections")]
        [SerializeField] private GameObject towerShopSection;
        [SerializeField] private GameObject itemUpgradeSection;
        [SerializeField] private GameObject skillSelectionSection;

        [Header("Start Wave Controls")]
        [SerializeField] private Button startWaveButton;
        [SerializeField] private Toggle timeLimitToggle;
        [SerializeField] private TextMeshProUGUI timeLimitLabel;

        [Header("Tower Shop")]
        [SerializeField] private Button purchaseTowerButton;
        [SerializeField] private Button upgradeTowerButton;
        [SerializeField] private TextMeshProUGUI goldAmountText;

        [Header("Item Upgrade")]
        [SerializeField] private Button upgradeItemButton;
        [SerializeField] private TextMeshProUGUI celestiumAmountText;
        [SerializeField] private GameObject itemUpgradeUIPanel; // Reference to ItemUpgradeUI panel

        [Header("Skill Selection")]
        [SerializeField] private Button selectSkillButton;
        [SerializeField] private TextMeshProUGUI availableSkillPointsText;

        private PreWavePhase preWavePhase;
        private int currentWaveNumber;

        private void Awake()
        {
            preWavePhase = GetComponentInParent<PreWavePhase>();

            // Setup button listeners
            if (startWaveButton != null)
                startWaveButton.onClick.AddListener(OnStartWaveClicked);

            if (timeLimitToggle != null)
                timeLimitToggle.onValueChanged.AddListener(OnTimeLimitToggled);

            if (purchaseTowerButton != null)
                purchaseTowerButton.onClick.AddListener(OnPurchaseTowerClicked);

            if (upgradeTowerButton != null)
                upgradeTowerButton.onClick.AddListener(OnUpgradeTowerClicked);

            if (upgradeItemButton != null)
                upgradeItemButton.onClick.AddListener(OnUpgradeItemClicked);

            if (selectSkillButton != null)
                selectSkillButton.onClick.AddListener(OnSelectSkillClicked);

            HidePhase();
        }

        public void ShowPhase(int waveNumber, bool hasTimeLimit, float timeLimit)
        {
            currentWaveNumber = waveNumber;

            if (phasePanel != null)
                phasePanel.SetActive(true);

            // Notify UICanvasManager if it exists
            if (UICanvasManager.Instance != null)
                UICanvasManager.Instance.ShowPreWavePhase();

            if (waveNumberText != null)
                waveNumberText.text = $"Prepare for Wave {waveNumber}";

            if (timeLimitToggle != null)
                timeLimitToggle.isOn = hasTimeLimit;

            UpdateTimeLimitToggle(hasTimeLimit);
            UpdateResourceDisplays();

            Debug.Log($"[PreWavePhaseUI] Showing phase for Wave {waveNumber}");
        }

        public void HidePhase()
        {
            if (phasePanel != null)
                phasePanel.SetActive(false);

            // Hide item upgrade panel if it's open
            if (itemUpgradeUIPanel != null)
                itemUpgradeUIPanel.SetActive(false);

            // Notify UICanvasManager if it exists
            if (UICanvasManager.Instance != null)
                UICanvasManager.Instance.HidePreWavePhase();

            Debug.Log("[PreWavePhaseUI] Hiding phase");
        }

        public void UpdateTimer(float remainingTime)
        {
            if (timerText != null && preWavePhase != null && preWavePhase.UseTimeLimit)
            {
                int minutes = Mathf.FloorToInt(remainingTime / 60f);
                int seconds = Mathf.FloorToInt(remainingTime % 60f);
                timerText.text = $"Time Remaining: {minutes:00}:{seconds:00}";
                timerText.gameObject.SetActive(true);
            }
            else if (timerText != null)
            {
                timerText.gameObject.SetActive(false);
            }
        }

        public void UpdateTimeLimitToggle(bool enabled)
        {
            if (timeLimitLabel != null)
            {
                timeLimitLabel.text = enabled ? "Time Limit: ON" : "Time Limit: OFF";
            }

            if (timerText != null)
            {
                timerText.gameObject.SetActive(enabled);
            }
        }

        private void UpdateResourceDisplays()
        {
            // Update gold display
            if (goldAmountText != null)
            {
                goldAmountText.text = $"Gold: {GetPlayerGold()}";
            }

            // Update celestium display
            if (celestiumAmountText != null)
            {
                celestiumAmountText.text = $"Celestium: {GetPlayerCelestium()}";
            }

            // Update skill points display
            if (availableSkillPointsText != null)
            {
                availableSkillPointsText.text = $"Skill Points: {GetAvailableSkillPoints()}";
            }
        }

        private void OnStartWaveClicked()
        {
            Debug.Log("[PreWavePhaseUI] Start Wave button clicked");
            if (preWavePhase != null)
            {
                preWavePhase.ManuallyStartWave();
            }
        }

        private void OnTimeLimitToggled(bool isOn)
        {
            Debug.Log($"[PreWavePhaseUI] Time limit toggled: {isOn}");
            if (preWavePhase != null)
            {
                preWavePhase.ToggleTimeLimit(isOn);
            }
        }

        private void OnPurchaseTowerClicked()
        {
            Debug.Log("[PreWavePhaseUI] Purchase Tower clicked");
            Debug.Log($"[DEBUG] Tower purchase requested - Current Gold: {GetPlayerGold()}");
            // TODO: Open tower selection/placement UI
            // TODO: Deduct gold when tower is placed
        }

        private void OnUpgradeTowerClicked()
        {
            Debug.Log("[PreWavePhaseUI] Upgrade Tower clicked");
            Debug.Log($"[DEBUG] Tower upgrade requested - Current Gold: {GetPlayerGold()}");
            // TODO: Open tower upgrade UI
            // TODO: Deduct gold when tower is upgraded
        }

        private void OnUpgradeItemClicked()
        {
            Debug.Log("[PreWavePhaseUI] Upgrade Item clicked");
            Debug.Log($"[DEBUG] Item upgrade requested - Current Celestium: {GetPlayerCelestium()}");

            // Toggle the item upgrade panel
            if (itemUpgradeUIPanel != null)
            {
                bool isActive = itemUpgradeUIPanel.activeSelf;
                itemUpgradeUIPanel.SetActive(!isActive);

                if (!isActive)
                {
                    var itemUpgradeUI = itemUpgradeUIPanel.GetComponent<ItemUpgradeUI>();
                    if (itemUpgradeUI != null)
                    {
                        // TODO: Replace this with the actual player's inventory reference
                        ItemInventory playerInventory = GetPlayerInventory();
                        itemUpgradeUI.Show(playerInventory); // Pass the required inventory argument
                        Debug.Log("[PreWavePhaseUI] Opening ItemUpgradeUI");
                    }
                }
            }
            else
            {
                Debug.LogWarning("[PreWavePhaseUI] ItemUpgradeUI panel not assigned!");
            }
        }

        private void OnSelectSkillClicked()
        {
            Debug.Log("[PreWavePhaseUI] Select Skill clicked");
            Debug.Log($"[DEBUG] Skill selection requested - Available Points: {GetAvailableSkillPoints()}");
            // TODO: Open skill tree/ability selection UI
            // TODO: Deduct skill points when skill is selected
        }

        // Placeholder methods - replace with actual economy system
        private int GetPlayerGold()
        {
            // TODO: Integrate with your economy/currency system
            return 1000; // Placeholder
        }

        private int GetPlayerCelestium()
        {
            // TODO: Integrate with your economy/currency system
            return 50; // Placeholder
        }

        private int GetAvailableSkillPoints()
        {
            // Changed: Remove reference to non-existent ExpSystem
            // TODO: Integrate with player's skill point system
            return 3; // Placeholder
        }

        private void Update()
        {
            if (preWavePhase != null && preWavePhase.IsPhaseActive && preWavePhase.UseTimeLimit)
            {
                UpdateTimer(preWavePhase.RemainingTime);
            }
        }

        // Add this placeholder method to avoid compile errors
        private ItemInventory GetPlayerInventory()
        {
            // TODO: Integrate with your actual player inventory system
            return null;
        }
    }
}