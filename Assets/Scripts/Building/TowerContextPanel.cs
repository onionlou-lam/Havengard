using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Havengard.Resources;

namespace Havengard.Building
{
    /// <summary>
    /// UI panel showing selected tower info, upgrade, and sell options
    /// </summary>
    public class TowerContextPanel : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI towerNameText;
        [SerializeField] private Image towerIcon;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private TextMeshProUGUI damageStatsText;

        [Header("Upgrade Section")]
        [SerializeField] private GameObject upgradeSection;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private TextMeshProUGUI upgradeButtonText;
        [SerializeField] private TextMeshProUGUI upgradeStatsPreviewText;

        [Header("Sell Section")]
        [SerializeField] private Button sellButton;
        [SerializeField] private TextMeshProUGUI sellButtonText;

        [Header("Close")]
        [SerializeField] private Button closeButton;

        [Header("Dialogs")]
        [SerializeField] private BuildingConfirmationDialog confirmationDialog;

        private GameObject currentTower;
        private TowerBuildData currentTowerData;

        private void Start()
        {
            SetupButtonListeners();

            if (confirmationDialog == null)
            {
                var hud = GetComponentInParent<BuildingHUD>();
                if (hud != null)
                    confirmationDialog = hud.GetComponentInChildren<BuildingConfirmationDialog>();
            }
        }

        private void SetupButtonListeners()
        {
            if (upgradeButton != null)
            {
                upgradeButton.onClick.RemoveAllListeners();
                upgradeButton.onClick.AddListener(OnUpgradeClicked);
            }

            if (sellButton != null)
            {
                sellButton.onClick.RemoveAllListeners();
                sellButton.onClick.AddListener(OnSellClicked);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(OnCloseClicked);
            }
        }

        public void ShowTowerInfo(GameObject tower)
        {
            if (tower == null)
                return;

            currentTower = tower;
            SetupButtonListeners();

            var tracker = tower.GetComponent<TowerInvestmentTracker>();
            if (tracker == null)
                return;

            var database = GetTowerDatabase();
            if (database != null)
            {
                currentTowerData = database.GetTowerByID(tracker.towerID);
            }

            // Update icon
            if (towerIcon != null && currentTowerData != null && currentTowerData.icon != null)
            {
                towerIcon.sprite = currentTowerData.icon;
                towerIcon.enabled = true;
            }
            else if (towerIcon != null)
            {
                towerIcon.enabled = false;
            }

            // Display basic info
            if (towerNameText != null)
                towerNameText.text = currentTowerData != null ? currentTowerData.displayName : tracker.towerID;

            if (levelText != null)
                levelText.text = $"Level {tracker.currentLevel + 1}";

            // Display stats
            var towerUnit = tower.GetComponent<Havengard.Units.TowerUnit>();
            if (towerUnit != null && statsText != null)
                statsText.text = BuildStatsText(towerUnit, tracker);

            // Display damage statistics
            if (damageStatsText != null)
                damageStatsText.text = BuildDamageStatsText(tracker);

            // Update sections
            UpdateUpgradeSection(tracker);
            UpdateSellSection(tracker);
        }

        private string BuildStatsText(Havengard.Units.TowerUnit towerUnit, TowerInvestmentTracker tracker)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.AppendLine("<b>Current Stats:</b>");

            if (currentTowerData != null)
            {
                var levelData = currentTowerData.GetLevelData(tracker.currentLevel);
                if (levelData != null)
                {
                    sb.AppendLine($"Damage: {levelData.damage}");
                    sb.AppendLine($"Range: {levelData.attackRange}");
                    sb.AppendLine($"Attack Speed: {levelData.attackSpeed}/s");
                }
            }

            return sb.ToString();
        }

        private string BuildDamageStatsText(TowerInvestmentTracker tracker)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.AppendLine($"<b>Total Damage:</b> {tracker.totalDamageDealt:F0}");

            if (tracker.previousWaveDamageDealt > 0)
                sb.AppendLine($"<b>Previous Wave:</b> {tracker.previousWaveDamageDealt:F0}");

            if (tracker.currentWaveDamageDealt > 0)
                sb.AppendLine($"<b>This Wave:</b> {tracker.currentWaveDamageDealt:F0}");

            sb.AppendLine();
            sb.AppendLine($"<b>Total Investment:</b> {tracker.totalInvestment} Gold");

            return sb.ToString();
        }

        private void UpdateUpgradeSection(TowerInvestmentTracker tracker)
        {
            if (upgradeSection == null || upgradeButton == null)
                return;

            var controller = BuildingModeController.Instance;
            if (controller == null)
            {
                upgradeSection.SetActive(false);
                return;
            }

            TowerBuildDatabase database = GetTowerDatabase();

            if (database != null)
                currentTowerData = database.GetTowerByID(tracker.towerID);

            if (currentTowerData == null)
            {
                upgradeSection.SetActive(false);
                return;
            }

            int nextLevel = tracker.currentLevel + 1;

            // Check if max level
            if (nextLevel >= currentTowerData.MaxLevel)
            {
                upgradeSection.SetActive(true);
                upgradeButton.interactable = false;

                if (upgradeButtonText != null)
                    upgradeButtonText.text = "MAX LEVEL";

                if (upgradeStatsPreviewText != null)
                    upgradeStatsPreviewText.text = "";

                return;
            }

            var nextLevelData = currentTowerData.GetLevelData(nextLevel);
            if (nextLevelData == null)
            {
                upgradeSection.SetActive(false);
                return;
            }

            upgradeSection.SetActive(true);

            // Check affordability
            bool canAfford = true;
            if (GoldSystem.Instance != null)
                canAfford = GoldSystem.Instance.Current >= nextLevelData.upgradeCost;

            upgradeButton.interactable = canAfford;

            // Update button text
            if (upgradeButtonText != null)
                upgradeButtonText.text = $"Upgrade to Level {nextLevel + 1}\nCost: {nextLevelData.upgradeCost} Gold";

            // Show stat preview
            if (upgradeStatsPreviewText != null)
            {
                var currentLevelData = currentTowerData.GetLevelData(tracker.currentLevel);
                upgradeStatsPreviewText.text = BuildUpgradePreviewText(currentLevelData, nextLevelData);
            }
        }

        private string BuildUpgradePreviewText(TowerLevelData current, TowerLevelData next)
        {
            if (current == null || next == null)
                return "";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.AppendLine("<b>Stat Changes:</b>");
            sb.AppendLine($"Damage: {current.damage} → <color=green>{next.damage}</color>");
            sb.AppendLine($"Range: {current.attackRange} → <color=green>{next.attackRange}</color>");
            sb.AppendLine($"Attack Speed: {current.attackSpeed}/s → <color=green>{next.attackSpeed}/s</color>");

            return sb.ToString();
        }

        private void UpdateSellSection(TowerInvestmentTracker tracker)
        {
            if (sellButton == null)
                return;

            int sellValue = tracker.GetSellValue();

            if (sellButtonText != null)
                sellButtonText.text = $"Sell Tower\nRefund: {sellValue} Gold";

            sellButton.interactable = true;
        }

        private TowerBuildDatabase GetTowerDatabase()
        {
            var controller = BuildingModeController.Instance;
            if (controller != null)
                return controller.TowerDatabase;

            return null;
        }

        private void OnUpgradeClicked()
        {
            if (currentTower == null)
                return;

            var controller = BuildingModeController.Instance;
            if (controller != null)
            {
                controller.UpgradeTower(currentTower);
                ShowTowerInfo(currentTower);
            }
        }

        private void OnSellClicked()
        {
            if (currentTower == null)
                return;

            var tracker = currentTower.GetComponent<TowerInvestmentTracker>();
            if (tracker == null)
                return;

            int sellValue = tracker.GetSellValue();
            string towerName = currentTowerData != null ? currentTowerData.displayName : tracker.towerID;

            if (confirmationDialog != null)
            {
                confirmationDialog.ShowSellConfirmation(
                    towerName,
                    sellValue,
                    onConfirm: () =>
                    {
                        var controller = BuildingModeController.Instance;
                        if (controller != null)
                            controller.SellTower(currentTower);
                    }
                );
            }
            else
            {
                var controller = BuildingModeController.Instance;
                if (controller != null)
                    controller.SellTower(currentTower);
            }
        }

        private void OnCloseClicked()
        {
            var hud = GetComponentInParent<BuildingHUD>();
            if (hud != null)
                hud.DeselectTower();
        }
    }
}