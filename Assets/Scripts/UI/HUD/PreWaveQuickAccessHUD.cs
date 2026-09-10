using UnityEngine;
using UnityEngine.UI;
using Havengard.Waves;
using Havengard.Building;
using Havengard.Expeditions.UI;
using Havengard.Items;

namespace Havengard.UI
{
    /// <summary>
    /// Manages the quick-access icon row shown during the Pre-Wave phase (top-right of the HUD,
    /// next to the currency display). Provides one-click access to Building Mode, the Expedition Map,
    /// the Skill Tree, and the Inventory. All icons are hidden automatically once a wave starts.
    /// </summary>
    public class PreWaveQuickAccessHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PreWavePhase preWavePhase;
        [SerializeField] private WaveManager waveManager;

        [Header("Panel")]
        [Tooltip("Parent container for all quick-access icons. Hidden entirely when a wave is active.")]
        [SerializeField] private GameObject quickAccessPanel;

        [Header("Building Mode")]
        [SerializeField] private Button buildButton;
        [SerializeField] private BuildingModeController buildingModeController;
        [SerializeField] private UIButtonGlowPulse buildButtonGlow;

        [Header("Expedition Map")]
        [SerializeField] private Button expeditionButton;
        [SerializeField] private ExpeditionMapController expeditionMapController;
        [SerializeField] private UIButtonGlowPulse expeditionButtonGlow;

        [Header("Skill Tree")]
        [SerializeField] private Button skillTreeButton;
        [SerializeField] private SkillTreeUI skillTreeUI;

        [Header("Inventory")]
        [SerializeField] private Button inventoryButton;
        [SerializeField] private InventoryUI inventoryUI;
        [Tooltip("Inventory to display when the inventory icon is clicked. Auto-found via Player tag if not assigned.")]
        [SerializeField] private ItemInventory playerInventory;

        [Header("Glow Pulse Settings")]
        [Tooltip("How long (seconds) the Build and Expedition icons glow-pulse for when first made available.")]
        [SerializeField] private float glowPulseDuration = 8f;

        private bool hasPulsedThisPhase = false;

        private void Awake()
        {
            if (preWavePhase == null)
                preWavePhase = FindFirstObjectByType<PreWavePhase>();

            if (waveManager == null)
                waveManager = FindFirstObjectByType<WaveManager>();

            if (buildingModeController == null)
                buildingModeController = FindFirstObjectByType<BuildingModeController>();

            if (expeditionMapController == null)
                expeditionMapController = ExpeditionMapController.Instance;

            if (skillTreeUI == null)
                skillTreeUI = FindFirstObjectByType<SkillTreeUI>();

            if (inventoryUI == null)
                inventoryUI = FindFirstObjectByType<InventoryUI>();

            if (playerInventory == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    playerInventory = player.GetComponent<ItemInventory>();
            }

            if (buildButton != null)
                buildButton.onClick.AddListener(OnBuildClicked);

            if (expeditionButton != null)
                expeditionButton.onClick.AddListener(OnExpeditionClicked);

            if (skillTreeButton != null)
                skillTreeButton.onClick.AddListener(OnSkillTreeClicked);

            if (inventoryButton != null)
                inventoryButton.onClick.AddListener(OnInventoryClicked);
        }

        private void OnEnable()
        {
            if (preWavePhase != null)
            {
                preWavePhase.OnPhaseStarted.AddListener(HandlePrepPhaseStarted);
                preWavePhase.OnPhaseEnded.AddListener(HandlePrepPhaseEnded);
            }
        }

        private void OnDisable()
        {
            if (preWavePhase != null)
            {
                preWavePhase.OnPhaseStarted.RemoveListener(HandlePrepPhaseStarted);
                preWavePhase.OnPhaseEnded.RemoveListener(HandlePrepPhaseEnded);
            }
        }

        private void HandlePrepPhaseStarted()
        {
            if (quickAccessPanel != null)
                quickAccessPanel.SetActive(true);

            // Draw attention to Build and Expedition Map on the first prep phase they appear.
            // (Remove the hasPulsedThisPhase guard below if you want the pulse to replay every wave.)
            if (!hasPulsedThisPhase)
            {
                hasPulsedThisPhase = true;

                if (buildButtonGlow != null)
                    buildButtonGlow.StartPulse(glowPulseDuration);

                if (expeditionButtonGlow != null)
                    expeditionButtonGlow.StartPulse(glowPulseDuration);
            }
        }

        private void HandlePrepPhaseEnded()
        {
            if (quickAccessPanel != null)
                quickAccessPanel.SetActive(false);

            // Stop any in-progress glow pulses immediately when the wave starts
            if (buildButtonGlow != null)
                buildButtonGlow.StopPulse();

            if (expeditionButtonGlow != null)
                expeditionButtonGlow.StopPulse();
        }

        private void OnBuildClicked()
        {
            if (buildingModeController != null)
                buildingModeController.EnterBuildingMode();
            else
                Debug.LogWarning("[PreWaveQuickAccessHUD] BuildingModeController reference is missing!");
        }

        private void OnExpeditionClicked()
        {
            if (expeditionMapController != null)
                expeditionMapController.OpenMap();
            else
                Debug.LogWarning("[PreWaveQuickAccessHUD] ExpeditionMapController reference is missing!");
        }

        private void OnSkillTreeClicked()
        {
            if (skillTreeUI != null)
                skillTreeUI.OpenSkillTree();
            else
                Debug.LogWarning("[PreWaveQuickAccessHUD] SkillTreeUI reference is missing!");
        }

        private void OnInventoryClicked()
        {
            if (inventoryUI != null && playerInventory != null)
                inventoryUI.Show(playerInventory);
            else
                Debug.LogWarning("[PreWaveQuickAccessHUD] InventoryUI or playerInventory reference is missing!");
        }
    }
}
