using UnityEngine;
using Havengard.Resources;
using Havengard.Waves;

namespace Havengard.Building
{
    /// <summary>
    /// Main controller for building mode.
    /// Coordinates camera, input, placement, and UI.
    /// </summary>
    public class BuildingModeController : MonoBehaviour
    {
        public static BuildingModeController Instance { get; private set; }

        [Header("References")]
        [SerializeField] private BuildGrid buildGrid;
        [SerializeField] private TowerBuildDatabase towerDatabase;
        [SerializeField] private WaveManager waveManager;
        [SerializeField] private BuildingCameraController buildingCamera;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private GameObject playerCharacter;

        [Header("UI")]
        [SerializeField] private BuildingHUD buildingHUD;
        [SerializeField] private BuildingGridVisual gridVisual;

        [Header("Placement")]
        [SerializeField] private TowerPlacementGhost ghostPrefab;

        [Header("Input")]
        [SerializeField] private KeyCode toggleBuildingModeKey = KeyCode.B;
        [SerializeField] private KeyCode exitBuildingModeKey = KeyCode.Escape;
        [SerializeField] private KeyCode undoKey = KeyCode.Z;

        [Header("Debug")]
        [SerializeField] private bool debugAlwaysAllowBuildingMode = false;

        // State
        private bool isBuildingMode = false;
        private bool isPlacingTower = false;
        private TowerBuildData selectedTowerData;
        private TowerPlacementGhost currentGhost;
        private PlacementValidator placementValidator;
        private TowerPlacementSystem placementSystem;
        private BuildingActionHistory actionHistory;

        // Current phase tracking
        private int currentWaveNumber = 0;
        private bool canEnterBuildingMode = false;

        // Properties
        public bool IsBuildingMode => isBuildingMode;
        public bool IsPlacingTower => isPlacingTower;
        public TowerBuildData SelectedTowerData => selectedTowerData;
        public BuildGrid Grid => buildGrid;
        public TowerBuildDatabase TowerDatabase => towerDatabase;

        private void Awake()
        {
            // Singleton setup
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Auto-find references if not assigned
            if (buildGrid == null)
                buildGrid = FindFirstObjectByType<BuildGrid>();

            if (waveManager == null)
                waveManager = FindFirstObjectByType<WaveManager>();

            if (buildingCamera == null)
                buildingCamera = FindFirstObjectByType<BuildingCameraController>();

            if (playerCamera == null)
                playerCamera = Camera.main;

            if (playerCharacter == null)
            {
                var playerController = FindFirstObjectByType<PlayerController2D>();
                if (playerController != null)
                    playerCharacter = playerController.gameObject;
            }

            if (buildingHUD == null)
                buildingHUD = FindFirstObjectByType<BuildingHUD>();

            if (gridVisual == null)
                gridVisual = FindFirstObjectByType<BuildingGridVisual>();

            // Initialize systems
            placementValidator = new PlacementValidator(buildGrid);
            placementSystem = new TowerPlacementSystem(buildGrid, towerDatabase);
            actionHistory = new BuildingActionHistory(this, placementSystem);

            // MOVE EVENT SUBSCRIPTION HERE FROM Start():
            SubscribeToWaveEvents();

            Debug.Log("[BuildingMode] Controller initialized");
        }

        // ADD THIS NEW METHOD:
        private void SubscribeToWaveEvents()
        {
            if (waveManager != null)
            {
                var preWavePhase = waveManager.GetComponent<PreWavePhase>();
                if (preWavePhase == null)
                {
                    preWavePhase = FindFirstObjectByType<PreWavePhase>();
                }

                if (preWavePhase != null)
                {
                    // Make sure we're not already subscribed
                    preWavePhase.OnPhaseStarted.RemoveListener(OnDefencePhaseStarted);
                    preWavePhase.OnPhaseEnded.RemoveListener(OnDefencePhaseEnded);
                    
                    // Subscribe
                    preWavePhase.OnPhaseStarted.AddListener(OnDefencePhaseStarted);
                    preWavePhase.OnPhaseEnded.AddListener(OnDefencePhaseEnded);
                    
                    Debug.Log("[BuildingMode] ✓ Subscribed to PreWavePhase events");
                    Debug.Log($"[BuildingMode] PreWavePhase active: {preWavePhase.IsPhaseActive}");
                }
                else
                {
                    Debug.LogError("[BuildingMode] ✗ PreWavePhase not found!");
                }
            }
            else
            {
                Debug.LogError("[BuildingMode] ✗ WaveManager not found!");
            }
        }

        private void Start()
        {
            // Event subscription now happens in Awake()
            
            // Initially hide building systems
            if (buildingCamera != null)
                buildingCamera.gameObject.SetActive(false);

            if (buildingHUD != null)
                buildingHUD.gameObject.SetActive(false);

            if (gridVisual != null)
                gridVisual.gameObject.SetActive(false);

            Debug.Log("[BuildingMode] Start complete - systems hidden");
            Debug.Log($"[BuildingMode] Can enter building mode: {canEnterBuildingMode}");
        }

        private void Update()
        {
            HandleInput();

            if (isBuildingMode && isPlacingTower && currentGhost != null)
            {
                UpdateGhostPlacement();
            }
        }

        private void HandleInput()
        {
            // Toggle building mode with B key
            if (Input.GetKeyDown(toggleBuildingModeKey))
            {
                if (isBuildingMode)
                    ExitBuildingMode();
                else
                    EnterBuildingMode();
            }

            // Exit building mode with Escape
            if (Input.GetKeyDown(exitBuildingModeKey) && isBuildingMode)
            {
                if (isPlacingTower)
                    CancelTowerPlacement();
                else
                    ExitBuildingMode();
            }

            // Undo with Ctrl+Z
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(undoKey))
            {
                if (isBuildingMode)
                    Undo();
            }

            // Placement input
            if (isBuildingMode && isPlacingTower)
            {
                // Mouse click to place
                if (Input.GetMouseButtonDown(0))
                {
                    TryPlaceTower();
                }

                // Arrow key movement
                if (Input.GetKeyDown(KeyCode.UpArrow))
                    MoveGhost(Vector2Int.up);
                else if (Input.GetKeyDown(KeyCode.DownArrow))
                    MoveGhost(Vector2Int.down);
                else if (Input.GetKeyDown(KeyCode.LeftArrow))
                    MoveGhost(Vector2Int.left);
                else if (Input.GetKeyDown(KeyCode.RightArrow))
                    MoveGhost(Vector2Int.right);
            }
        }

        #region Building Mode State

        public void EnterBuildingMode()
        {
            if (isBuildingMode)
            {
                Debug.LogWarning("[BuildingMode] Already in building mode");
                return;
            }

            // ADD DEBUG OVERRIDE:
            if (!canEnterBuildingMode && !debugAlwaysAllowBuildingMode)
            {
                Debug.LogWarning("[BuildingMode] Cannot enter building mode - not in defence phase");
                Debug.Log($"[BuildingMode] canEnterBuildingMode = {canEnterBuildingMode}");
                Debug.Log($"[BuildingMode] debugAlwaysAllowBuildingMode = {debugAlwaysAllowBuildingMode}");
                return;
            }

            Debug.Log("[BuildingMode] Entering building mode...");

            isBuildingMode = true;

            // Switch camera
            if (playerCamera != null)
            {
                playerCamera.gameObject.SetActive(false);
                Debug.Log("[BuildingMode] Player camera deactivated");
            }
            else
            {
                Debug.LogWarning("[BuildingMode] Player camera is null!");
            }

            if (buildingCamera != null)
            {
                buildingCamera.gameObject.SetActive(true);
                buildingCamera.FocusOnGrid();
                Debug.Log("[BuildingMode] Building camera activated");
            }
            else
            {
                Debug.LogWarning("[BuildingMode] Building camera is null!");
            }

            // Disable player movement
            if (playerCharacter != null)
            {
                var playerController = playerCharacter.GetComponent<PlayerController2D>();
                if (playerController != null)
                {
                    playerController.enabled = false;
                    Debug.Log("[BuildingMode] Player controller disabled");
                }
            }

            // Show UI
            if (buildingHUD != null)
            {
                buildingHUD.gameObject.SetActive(true);
                buildingHUD.Show();
                buildingHUD.UpdateGoldDisplay();
                Debug.Log("[BuildingMode] Building HUD shown");
            }
            else
            {
                Debug.LogWarning("[BuildingMode] Building HUD is null!");
            }

            // Show grid
            if (gridVisual != null)
            {
                gridVisual.gameObject.SetActive(true);
                gridVisual.ShowGrid();
                Debug.Log("[BuildingMode] Grid visual shown");
            }
            else
            {
                Debug.LogWarning("[BuildingMode] Grid visual is null!");
            }

            Debug.Log("[BuildingMode] ✓ Entered Building Mode successfully");
        }

        public void ExitBuildingMode()
        {
            if (!isBuildingMode)
                return;

            // Cancel any active placement
            if (isPlacingTower)
                CancelTowerPlacement();

            isBuildingMode = false;

            // Switch camera back
            if (buildingCamera != null)
                buildingCamera.gameObject.SetActive(false);

            if (playerCamera != null)
                playerCamera.gameObject.SetActive(true);

            // Re-enable player movement
            if (playerCharacter != null)
            {
                var playerController = playerCharacter.GetComponent<PlayerController2D>();
                if (playerController != null)
                    playerController.enabled = true;
            }

            // Hide UI
            if (buildingHUD != null)
            {
                buildingHUD.Hide();
                buildingHUD.gameObject.SetActive(false);
            }

            // Hide grid
            if (gridVisual != null)
            {
                gridVisual.HideGrid();
                gridVisual.gameObject.SetActive(false);
            }

            Debug.Log("[BuildingMode] Exited Building Mode");
        }

        public void ToggleBuildingMode()
        {
            if (isBuildingMode)
                ExitBuildingMode();
            else
                EnterBuildingMode();
        }

        #endregion

        #region Tower Selection & Placement

        public void SelectTower(TowerBuildData towerData)
        {
            if (towerData == null)
            {
                Debug.LogWarning("[BuildingMode] Cannot select null tower data");
                return;
            }

            selectedTowerData = towerData;
            StartTowerPlacement();

            Debug.Log($"[BuildingMode] Selected Tower: {towerData.displayName}");
        }

        private void StartTowerPlacement()
        {
            if (selectedTowerData == null || ghostPrefab == null)
                return;

            // Clean up existing ghost
            if (currentGhost != null)
                Destroy(currentGhost.gameObject);

            // Create new ghost
            currentGhost = Instantiate(ghostPrefab);
            currentGhost.Initialize(selectedTowerData, buildGrid, placementValidator);
            currentGhost.gameObject.SetActive(true);

            isPlacingTower = true;

            // Update ghost position immediately
            UpdateGhostPlacement();

            Debug.Log($"[BuildingMode] Started placement for: {selectedTowerData.displayName}");
        }

        public void CancelTowerPlacement()
        {
            if (currentGhost != null)
            {
                Destroy(currentGhost.gameObject);
                currentGhost = null;
            }

            isPlacingTower = false;
            selectedTowerData = null;

            Debug.Log("[BuildingMode] Cancelled tower placement");
        }

        private void UpdateGhostPlacement()
        {
            if (currentGhost == null)
                return;

            // Get mouse position in world space
            Vector3 mouseWorldPos = GetMouseWorldPosition();

            // Convert to grid position
            Vector2Int gridPos = buildGrid.WorldToGrid(mouseWorldPos);

            // Update ghost
            currentGhost.UpdatePosition(gridPos);
        }

        private void MoveGhost(Vector2Int direction)
        {
            if (currentGhost == null)
                return;

            Vector2Int currentGridPos = currentGhost.CurrentGridPosition;
            Vector2Int newGridPos = currentGridPos + direction;

            currentGhost.UpdatePosition(newGridPos);
        }

        private void TryPlaceTower()
        {
            if (!isPlacingTower || selectedTowerData == null || currentGhost == null)
                return;

            Vector2Int gridPos = currentGhost.CurrentGridPosition;

            // Validate placement
            var validationResult = placementValidator.ValidatePlacement(selectedTowerData, gridPos, 0);

            if (validationResult != PlacementValidationResult.Valid)
            {
                Debug.Log($"[BuildingMode] Placement Invalid: {validationResult}");
                PlayInvalidPlacementFeedback();
                return;
            }

            // Place tower
            var levelData = selectedTowerData.GetLevelData(0);
            if (levelData == null)
            {
                Debug.LogError("[BuildingMode] No level data for tower");
                return;
            }

            // Spend gold
            if (GoldSystem.Instance != null)
            {
                if (!GoldSystem.Instance.SpendGold(levelData.buildCost))
                {
                    Debug.LogWarning("[BuildingMode] Failed to spend gold");
                    return;
                }
            }

            // Place the tower
            GameObject placedTower = placementSystem.PlaceTower(selectedTowerData, gridPos, currentWaveNumber);

            if (placedTower != null)
            {
                // Record action for undo
                actionHistory.RecordPlacement(placedTower, selectedTowerData, gridPos, levelData.buildCost);

                // Update economy display
                if (buildingHUD != null)
                    buildingHUD.UpdateGoldDisplay();

                Debug.Log($"[BuildingMode] Built {selectedTowerData.displayName} at {gridPos} for {levelData.buildCost} Gold");
                Debug.Log($"[Building] Wave Investment: {actionHistory.GetCurrentPhaseInvestment()}");
                Debug.Log($"[Building] Total Tower Investment: {placementSystem.GetTotalInvestment()}");

                // Continue placing same tower type
                // User can click Escape or select another tower to change
            }
            else
            {
                Debug.LogError("[BuildingMode] Failed to place tower");

                // Refund gold
                if (GoldSystem.Instance != null)
                    GoldSystem.Instance.AddGold(levelData.buildCost);
            }
        }

        private void PlayInvalidPlacementFeedback()
        {
            // TODO: Add audio/visual feedback for invalid placement
            // For now, just a log
        }

        #endregion

        #region Tower Management

        public void UpgradeTower(GameObject towerObject)
        {
            if (towerObject == null)
                return;

            var tracker = towerObject.GetComponent<TowerInvestmentTracker>();
            if (tracker == null)
            {
                Debug.LogWarning("[BuildingMode] Tower has no investment tracker");
                return;
            }

            var towerData = towerDatabase.GetTowerByID(tracker.towerID);
            if (towerData == null)
            {
                Debug.LogWarning($"[BuildingMode] Tower data not found for ID: {tracker.towerID}");
                return;
            }

            int nextLevel = tracker.currentLevel + 1;
            if (nextLevel >= towerData.MaxLevel)
            {
                Debug.Log("[BuildingMode] Tower is already max level");
                return;
            }

            var levelData = towerData.GetLevelData(nextLevel);
            if (levelData == null)
            {
                Debug.LogWarning($"[BuildingMode] No level data for level {nextLevel}");
                return;
            }

            // Check gold
            if (GoldSystem.Instance != null)
            {
                if (GoldSystem.Instance.Current < levelData.upgradeCost)
                {
                    Debug.Log("[BuildingMode] Insufficient gold for upgrade");
                    return;
                }

                // Spend gold
                if (!GoldSystem.Instance.SpendGold(levelData.upgradeCost))
                    return;
            }

            // Apply upgrade
            tracker.AddUpgradeCost(levelData.upgradeCost);

            // Update tower stats
            var towerUnit = towerObject.GetComponent<Havengard.Units.TowerUnit>();
            if (towerUnit != null)
            {
                towerUnit.ApplyLevelStats(
                    levelData.damage,
                    levelData.attackRange,
                    levelData.attackSpeed,
                    levelData.projectileSpeed
                );
            }

            // Record action for undo
            actionHistory.RecordUpgrade(towerObject, levelData.upgradeCost);

            // Update UI
            if (buildingHUD != null)
            {
                buildingHUD.UpdateGoldDisplay();
                buildingHUD.RefreshTowerContextPanel(towerObject);
            }

            Debug.Log($"[BuildingMode] Upgraded {towerData.displayName} to Level {nextLevel + 1} for {levelData.upgradeCost} Gold");
            Debug.Log($"[Building] Total Tower Investment: {placementSystem.GetTotalInvestment()}");
        }

        public void SellTower(GameObject towerObject)
        {
            if (towerObject == null)
                return;

            var tracker = towerObject.GetComponent<TowerInvestmentTracker>();
            if (tracker == null)
            {
                Debug.LogWarning("[BuildingMode] Tower has no investment tracker");
                return;
            }

            int sellValue = tracker.GetSellValue();

            // Refund gold
            if (GoldSystem.Instance != null)
                GoldSystem.Instance.AddGold(sellValue);

            // Free grid cells
            buildGrid.FreeCells(tracker.gridPosition, tracker.gridWidth, tracker.gridHeight);

            // Remove from placement system tracking
            placementSystem.RemoveTower(towerObject);

            // Destroy tower
            Destroy(towerObject);

            // Update UI
            if (buildingHUD != null)
            {
                buildingHUD.UpdateGoldDisplay();
                buildingHUD.DeselectTower();
            }

            Debug.Log($"[BuildingMode] Sold tower for {sellValue} Gold");
            Debug.Log($"[Building] Total Tower Investment: {placementSystem.GetTotalInvestment()}");
        }

        public void SelectExistingTower(GameObject towerObject)
        {
            if (buildingHUD != null)
            {
                buildingHUD.ShowTowerContextPanel(towerObject);
            }
        }

        #endregion

        #region Undo & Reset

        public void Undo()
        {
            if (actionHistory.CanUndo())
            {
                actionHistory.Undo();

                // Update UI
                if (buildingHUD != null)
                {
                    buildingHUD.UpdateGoldDisplay();
                    buildingHUD.DeselectTower();
                }

                Debug.Log("[BuildingMode] Undo: Removed last action");
            }
            else
            {
                Debug.Log("[BuildingMode] Nothing to undo");
            }
        }

        public void ResetCurrentPhase()
        {
            int towersRemoved = actionHistory.GetCurrentPhaseTowerCount();
            int refundAmount = actionHistory.GetCurrentPhaseInvestment();

            if (towersRemoved == 0)
            {
                Debug.Log("[BuildingMode] No towers to reset");
                return;
            }

            // Show confirmation dialog (for now, just execute)
            // TODO: Integrate with confirmation dialog UI

            actionHistory.ResetCurrentPhase();

            // Update UI
            if (buildingHUD != null)
            {
                buildingHUD.UpdateGoldDisplay();
                buildingHUD.DeselectTower();
            }

            Debug.Log($"[BuildingMode] Reset current defence-phase placements");
            Debug.Log($"[BuildingMode] Removed {towersRemoved} towers, refunded {refundAmount} Gold");
        }

        #endregion

        #region Wave Integration

        private void OnDefencePhaseStarted()
        {
            Debug.Log("[BuildingMode] ========== DEFENCE PHASE STARTED EVENT RECEIVED ==========");
            
            canEnterBuildingMode = true;
            currentWaveNumber++;

            // Reset action history for new phase
            actionHistory.StartNewPhase();

            // Notify all existing towers of new wave
            var allTowers = FindObjectsByType<TowerInvestmentTracker>(FindObjectsSortMode.None);
            foreach (var tracker in allTowers)
            {
                tracker.OnWaveStarted();
            }

            Debug.Log($"[BuildingMode] ✓ Defence Phase started for wave {currentWaveNumber}");
            Debug.Log($"[BuildingMode] ✓ canEnterBuildingMode = {canEnterBuildingMode}");
        }

        private void OnDefencePhaseEnded()
        {
            Debug.Log("[BuildingMode] ========== DEFENCE PHASE ENDED EVENT RECEIVED ==========");
            
            canEnterBuildingMode = false;

            // Auto-exit building mode if active
            if (isBuildingMode)
            {
                ExitBuildingMode();
                Debug.Log("[BuildingMode] Wave starting - auto-exited building mode");
            }
            
            Debug.Log($"[BuildingMode] ✓ canEnterBuildingMode = {canEnterBuildingMode}");
        }

        public void OnWaveCompleted()
        {
            // Called when wave ends (not defence phase)
            var allTowers = FindObjectsByType<TowerInvestmentTracker>(FindObjectsSortMode.None);
            foreach (var tracker in allTowers)
            {
                tracker.OnWaveEnded();
            }

            Debug.Log("[BuildingMode] Wave completed - tower stats updated");
        }

        #endregion

        #region Public Accessors for UI

        /// <summary>
        /// Check if undo is available
        /// </summary>
        public bool CanUndo()
        {
            return actionHistory != null && actionHistory.CanUndo();
        }

        /// <summary>
        /// Get current phase investment amount
        /// </summary>
        public int GetCurrentPhaseInvestment()
        {
            return actionHistory != null ? actionHistory.GetCurrentPhaseInvestment() : 0;
        }

        /// <summary>
        /// Get current phase tower count
        /// </summary>
        public int GetCurrentPhaseTowerCount()
        {
            return actionHistory != null ? actionHistory.GetCurrentPhaseTowerCount() : 0;
        }

        /// <summary>
        /// Get total investment across all towers
        /// </summary>
        public int GetTotalInvestment()
        {
            return placementSystem != null ? placementSystem.GetTotalInvestment() : 0;
        }

        #endregion

        #region Utility

        private Vector3 GetMouseWorldPosition()
        {
            if (buildingCamera == null)
                return Vector3.zero;

            Vector3 mouseScreenPos = Input.mousePosition;
            mouseScreenPos.z = Mathf.Abs(buildingCamera.transform.position.z);

            return buildingCamera.GetComponent<Camera>().ScreenToWorldPoint(mouseScreenPos);
        }

        #endregion

        private void OnDestroy()
        {
            // Cleanup
            if (currentGhost != null)
                Destroy(currentGhost.gameObject);
        }
    }
}