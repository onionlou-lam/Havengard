using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Havengard.Items;
using Havengard.Stats;
using Havengard.Resources;

namespace Havengard.UI
{
    /// <summary>
    /// Manages HUD buttons that appear between waves
    /// </summary>
    public class WaveHUDButtons : MonoBehaviour
    {
        [Header("Button References")]
        [SerializeField] private GameObject buttonPanel;
        [SerializeField] private Button upgradeItemButton;
        [SerializeField] private Button levelUpButton;
        [SerializeField] private Button buildButton;
        [SerializeField] private Button startWaveButton;

        [Header("Highlight Settings")]
        [SerializeField] private bool enableHighlights = true;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color highlightColor = new Color(1f, 1f, 0.5f, 1f); // Yellowish
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseIntensity = 0.3f;

        [Header("Start Wave Timer")]
        [SerializeField] private float autoHighlightDelay = 30f;
        [SerializeField] private TextMeshProUGUI startWaveTimerText;
        [SerializeField] private TextMeshProUGUI startWaveText;

        [Header("References")]
        [SerializeField] private ItemInventory playerInventory;
        [SerializeField] private PlayerStatAllocator statAllocator;
        [SerializeField] private Havengard.Waves.WaveManager waveManager;

        private bool shouldHighlightUpgrade;
        private bool shouldHighlightLevelUp;
        private bool shouldHighlightBuild;
        private bool shouldHighlightStartWave;

        private float wavePhaseStartTime;
        private bool inWavePhase;
        private int nextWaveIndex;

        // Button images for highlighting
        private Image upgradeImage;
        private Image levelUpImage;
        private Image buildImage;
        private Image startWaveImage;

        private void Awake()
        {
            // Get button images
            if (upgradeItemButton != null) upgradeImage = upgradeItemButton.GetComponent<Image>();
            if (levelUpButton != null) levelUpImage = levelUpButton.GetComponent<Image>();
            if (buildButton != null) buildImage = buildButton.GetComponent<Image>();
            if (startWaveButton != null) startWaveImage = startWaveButton.GetComponent<Image>();

            // Find references if not assigned
            if (playerInventory == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) playerInventory = player.GetComponent<ItemInventory>();
            }

            if (statAllocator == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) statAllocator = player.GetComponent<PlayerStatAllocator>();
            }

            if (waveManager == null)
                waveManager = FindFirstObjectByType<Havengard.Waves.WaveManager>();

            // Setup button listeners
            if (upgradeItemButton != null)
                upgradeItemButton.onClick.AddListener(OnUpgradeItemClicked);

            if (levelUpButton != null)
                levelUpButton.onClick.AddListener(OnLevelUpClicked);

            if (buildButton != null)
                buildButton.onClick.AddListener(OnBuildClicked);

            if (startWaveButton != null)
                startWaveButton.onClick.AddListener(OnStartWaveClicked);

            // Subscribe to events
            if (playerInventory != null)
                playerInventory.OnInventoryChanged += CheckHighlightConditions;

            if (statAllocator != null)
            {
                statAllocator.OnStatPointsChanged += (_) => CheckHighlightConditions();
                statAllocator.OnPowerPointsChanged += (_) => CheckHighlightConditions();
            }

            if (CelestiumSystem.Instance != null)
                CelestiumSystem.Instance.OnCelestiumChanged += (_) => CheckHighlightConditions();
        }

        private void OnDestroy()
        {
            if (playerInventory != null)
                playerInventory.OnInventoryChanged -= CheckHighlightConditions;

            if (statAllocator != null)
            {
                statAllocator.OnStatPointsChanged -= (_) => CheckHighlightConditions();
                statAllocator.OnPowerPointsChanged -= (_) => CheckHighlightConditions();
            }

            if (CelestiumSystem.Instance != null)
                CelestiumSystem.Instance.OnCelestiumChanged -= (_) => CheckHighlightConditions();
        }

        private void Update()
        {
            UpdateHighlights();
            UpdateStartWaveTimer();
        }

        #region Wave Phase Management

        public void StartWavePhase(int waveIndex)
        {
            inWavePhase = true;
            nextWaveIndex = waveIndex + 1;
            HideButtons();
            Debug.Log($"[WaveHUDButtons] Wave {waveIndex} started - buttons hidden");
        }

        public void EndWavePhase()
        {
            inWavePhase = false;
            wavePhaseStartTime = Time.time;
            ShowButtons();
            CheckHighlightConditions();
            Debug.Log("[WaveHUDButtons] Wave ended - buttons shown");
        }

        private void ShowButtons()
        {
            if (buttonPanel != null)
                buttonPanel.SetActive(true);
        }

        private void HideButtons()
        {
            if (buttonPanel != null)
                buttonPanel.SetActive(false);
        }

        #endregion

        #region Highlight Conditions

        private void CheckHighlightConditions()
        {
            if (!enableHighlights)
            {
                shouldHighlightUpgrade = false;
                shouldHighlightLevelUp = false;
                shouldHighlightBuild = false;
                return;
            }

            CheckUpgradeHighlight();
            CheckLevelUpHighlight();
            CheckBuildHighlight();
        }

        private void CheckUpgradeHighlight()
        {
            // Highlight if player has items AND enough celestium to upgrade at least one
            shouldHighlightUpgrade = false;

            if (playerInventory == null || CelestiumSystem.Instance == null)
                return;

            var items = playerInventory.GetAllItems();
            if (items == null || items.Count == 0)
                return;

            foreach (var item in items)
            {
                if (item.itemData != null && item.level < item.itemData.maxLevel)
                {
                    // Check if player has enough celestium (example: 50 per upgrade)
                    int upgradeCost = GetItemUpgradeCost(item);
                    if (CelestiumSystem.Instance.Current >= upgradeCost)
                    {
                        shouldHighlightUpgrade = true;
                        return;
                    }
                }
            }
        }

        private void CheckLevelUpHighlight()
        {
            // Highlight if player has unspent stat or power points
            shouldHighlightLevelUp = false;

            if (statAllocator == null)
                return;

            shouldHighlightLevelUp = statAllocator.HasUnspentPoints;
        }

        private void CheckBuildHighlight()
        {
            // Highlight if player has enough gold to build (example: 100 gold)
            shouldHighlightBuild = false;

            if (GoldSystem.Instance == null)
                return;

            int buildCost = 100; // Placeholder cost
            shouldHighlightBuild = GoldSystem.Instance.Current >= buildCost;
        }

        private void UpdateStartWaveTimer()
        {
            if (inWavePhase)
            {
                shouldHighlightStartWave = false;
                if (startWaveTimerText != null)
                    startWaveTimerText.gameObject.SetActive(false);
                return;
            }

            float elapsed = Time.time - wavePhaseStartTime;
            float remaining = Mathf.Max(0f, autoHighlightDelay - elapsed);

            shouldHighlightStartWave = elapsed >= autoHighlightDelay;

            if (startWaveTimerText != null)
            {
                if (remaining > 0f)
                {
                    startWaveTimerText.gameObject.SetActive(true);
                    startWaveTimerText.text = $"Auto-start in {Mathf.CeilToInt(remaining)}s";
                }
                else
                {
                    startWaveTimerText.gameObject.SetActive(false);
                }
            }

            // Update start wave button text
            if (startWaveText != null && waveManager != null)
            {
                int displayWave = waveManager.CurrentWaveIndex + 2; // Next wave
                startWaveText.text = $"Start Wave {displayWave}";
            }
        }

        #endregion

        #region Highlight Rendering

        private void UpdateHighlights()
        {
            if (!enableHighlights)
            {
                SetButtonColor(upgradeImage, normalColor);
                SetButtonColor(levelUpImage, normalColor);
                SetButtonColor(buildImage, normalColor);
                SetButtonColor(startWaveImage, normalColor);
                return;
            }

            float pulse = Mathf.PingPong(Time.time * pulseSpeed, 1f) * pulseIntensity;
            Color pulseColor = Color.Lerp(highlightColor, normalColor, pulse);

            SetButtonColor(upgradeImage, shouldHighlightUpgrade ? pulseColor : normalColor);
            SetButtonColor(levelUpImage, shouldHighlightLevelUp ? pulseColor : normalColor);
            SetButtonColor(buildImage, shouldHighlightBuild ? pulseColor : normalColor);
            SetButtonColor(startWaveImage, shouldHighlightStartWave ? pulseColor : normalColor);
        }

        private void SetButtonColor(Image image, Color color)
        {
            if (image != null)
                image.color = color;
        }

        #endregion

        #region Button Handlers

        private void OnUpgradeItemClicked()
        {
            Debug.Log("[WaveHUDButtons] Upgrade Item button clicked");

            // Open item upgrade UI
            var upgradeUI = FindFirstObjectByType<ItemUpgradeUI>();
            if (upgradeUI != null)
            {
                upgradeUI.Show(playerInventory);
            }
            else
            {
                Debug.LogWarning("[WaveHUDButtons] ItemUpgradeUI not found in scene");
            }
        }

        private void OnLevelUpClicked()
        {
            Debug.Log("[WaveHUDButtons] Level Up button clicked");

            // Open stat allocation UI
            var statUI = FindFirstObjectByType<StatAllocationUI>();
            if (statUI != null)
            {
                statUI.Show(statAllocator);
            }
            else
            {
                Debug.LogWarning("[WaveHUDButtons] StatAllocationUI not found in scene");
            }
        }

        private void OnBuildClicked()
        {
            Debug.Log("[WaveHUDButtons] Build button clicked - Building system not yet implemented");
            // TODO: Open building UI when implemented
        }

        private void OnStartWaveClicked()
        {
            Debug.Log($"[WaveHUDButtons] Start Wave button clicked - Starting wave {nextWaveIndex}");

            if (waveManager != null)
            {
                // This will need to be adjusted based on your WaveManager API
                // For now, just restart the night
                waveManager.StartNight();
                StartWavePhase(nextWaveIndex);
            }
        }

        #endregion

        #region Helper Methods

        private int GetItemUpgradeCost(ItemInstance item)
        {
            // Example: base cost * current level
            int baseCost = 50;
            return baseCost * item.level;
        }

        #endregion
    }
}