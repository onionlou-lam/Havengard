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
        [SerializeField] private BuildingModeUIHelper uiHelper;
        [SerializeField] private TowerTooltip towerTooltip;

        [Header("UI to Hide")]
        [SerializeField] private GameObject canvasHUD;

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
        private GameObject currentlySelectedTower = null;

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

            if (towerTooltip == null)
                towerTooltip = FindFirstObjectByType<TowerTooltip>();

            if (canvasHUD == null)
            {
                canvasHUD = GameObject.Find("CANVAS_HUD");
                if (canvasHUD == null)
                    canvasHUD = GameObject.Find("Canvas_HUD");
            }

            // Initialize systems
            placementValidator = new PlacementValidator(buildGrid);
            placementSystem = new TowerPlacementSystem(buildGrid, towerDatabase);
            actionHistory = new BuildingActionHistory(this, placementSystem);

            // Subscribe to wave events
            SubscribeToWaveEvents();
        }

        private void SubscribeToWaveEvents()
        {
            if (waveManager != null)
            {
                var preWavePhase = waveManager.GetComponent<PreWavePhase>();
                if (preWavePhase == null)
                    preWavePhase = FindFirstObjectByType<PreWavePhase>();

                if (preWavePhase != null)
                {
                    preWavePhase.OnPhaseStarted.RemoveListener(OnDefencePhaseStarted);
                    preWavePhase.OnPhaseEnded.RemoveListener(OnDefencePhaseEnded);
                    preWavePhase.OnPhaseStarted.AddListener(OnDefencePhaseStarted);
                    preWavePhase.OnPhaseEnded.AddListener(OnDefencePhaseEnded);
                }
            }
        }

        private void Start()
        {
            // Initially hide building systems
            if (buildingCamera != null)
                buildingCamera.gameObject.SetActive(false);

            if (buildingHUD != null)
                buildingHUD.gameObject.SetActive(false);

            if (gridVisual != null)
                gridVisual.gameObject.SetActive(false);
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

            // Building mode input
            if (isBuildingMode)
            {
                // Left click handling - ignore if over UI
                if (Input.GetMouseButtonDown(0))
                {
                    if (IsPointerOverUIElement())
                        return;

                    if (isPlacingTower)
                        TryPlaceTower();
                    else
                        TrySelectExistingTower();
                }

                // Right click to cancel placement
                if (Input.GetMouseButtonDown(1) && isPlacingTower)
                {
                    CancelTowerPlacement();
                }

                // Arrow key movement (only when placing)
                if (isPlacingTower)
                {
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
        }

        private bool IsPointerOverUIElement()
        {
            var eventSystem = UnityEngine.EventSystems.EventSystem.current;
            if (eventSystem == null)
                return false;

            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                return true;

            var pointerData = new UnityEngine.EventSystems.PointerEventData(eventSystem);
            pointerData.position = Input.mousePosition;

            var results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
            eventSystem.RaycastAll(pointerData, results);

            foreach (var result in results)
            {
                if (result.gameObject.name == "BuildingHUD" || result.gameObject.name == "TowerContextPanel")
                    continue;

                if (result.gameObject.GetComponent<UnityEngine.UI.Graphic>() != null ||
                    result.gameObject.GetComponent<UnityEngine.UI.Button>() != null)
                {
                    return true;
                }
            }

            return false;
        }

        #region Building Mode State

        public void EnterBuildingMode()
        {
            if (isBuildingMode)
                return;

            if (!canEnterBuildingMode && !debugAlwaysAllowBuildingMode)
                return;

            isBuildingMode = true;

            // Hide PausePanel if it exists
            GameObject pausePanel = GameObject.Find("PausePanel");
            if (pausePanel != null)
                pausePanel.SetActive(false);

            // Hide player HUD
            if (canvasHUD != null)
                canvasHUD.SetActive(false);

            // Switch camera
            if (playerCamera != null)
                playerCamera.gameObject.SetActive(false);

            if (buildingCamera != null)
            {
                buildingCamera.gameObject.SetActive(true);
                buildingCamera.FocusOnGrid();
            }

            // Disable player movement
            if (playerCharacter != null)
            {
                var playerController = playerCharacter.GetComponent<PlayerController2D>();
                if (playerController != null)
                    playerController.enabled = false;
            }

            // Show UI
            if (buildingHUD != null)
            {
                buildingHUD.gameObject.SetActive(true);

                // Ensure BuildingHUD has its own canvas on top
                Canvas dedicatedCanvas = buildingHUD.GetComponent<Canvas>();
                if (dedicatedCanvas == null)
                {
                    dedicatedCanvas = buildingHUD.gameObject.AddComponent<Canvas>();
                    if (buildingHUD.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
                        buildingHUD.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                }
                dedicatedCanvas.overrideSorting = true;
                dedicatedCanvas.sortingOrder = 9999;

                buildingHUD.Show();
                buildingHUD.UpdateGoldDisplay();
            }

            // Show grid
            if (gridVisual != null)
            {
                gridVisual.gameObject.SetActive(true);
                gridVisual.ShowGrid();
            }

            // Hide gameplay UI
            if (uiHelper != null)
                uiHelper.HideUIElements();

            if (BuildingAudioManager.Instance != null)
                BuildingAudioManager.Instance.PlayEnterBuildingMode();
        }

        public void ExitBuildingMode()
        {
            if (!isBuildingMode)
                return;

            if (isPlacingTower)
                CancelTowerPlacement();

            if (currentlySelectedTower != null)
                currentlySelectedTower = null;

            isBuildingMode = false;

            // Show player HUD again
            if (canvasHUD != null)
                canvasHUD.SetActive(true);

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

            // Restore gameplay UI
            if (uiHelper != null)
                uiHelper.RestoreUIElements();

            if (BuildingAudioManager.Instance != null)
                BuildingAudioManager.Instance.PlayExitBuildingMode();
        }

        #endregion

        #region Tower Selection & Placement

        public void SelectTower(TowerBuildData towerData)
        {
            if (towerData == null)
                return;

            selectedTowerData = towerData;
            StartTowerPlacement();
        }

        private void StartTowerPlacement()
        {
            if (selectedTowerData == null || ghostPrefab == null)
                return;

            if (currentGhost != null)
                Destroy(currentGhost.gameObject);

            currentGhost = Instantiate(ghostPrefab);
            currentGhost.Initialize(selectedTowerData, buildGrid, placementValidator);
            currentGhost.gameObject.SetActive(true);

            isPlacingTower = true;
            UpdateGhostPlacement();
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

            if (towerTooltip != null)
                towerTooltip.HideTooltip();
        }

        private void UpdateGhostPlacement()
        {
            if (currentGhost == null)
                return;

            Vector3 mouseWorldPos = GetMouseWorldPosition();
            Vector2Int gridPos = buildGrid.WorldToGrid(mouseWorldPos);
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

            // Check if clicking on existing tower
            GameObject existingTower = buildGrid.GetTowerAtCell(gridPos);
            if (existingTower != null)
            {
                var tracker = existingTower.GetComponent<TowerInvestmentTracker>();
                if (tracker != null)
                {
                    CancelTowerPlacement();
                    SelectExistingTower(existingTower);
                    return;
                }
            }

            // Validate placement
            var validationResult = placementValidator.ValidatePlacement(selectedTowerData, gridPos, 0);
            if (validationResult != PlacementValidationResult.Valid)
            {
                if (BuildingAudioManager.Instance != null)
                    BuildingAudioManager.Instance.PlayPlacementInvalid();

                return;
            }

            var levelData = selectedTowerData.GetLevelData(0);
            if (levelData == null)
                return;

            // Spend gold
            if (GoldSystem.Instance != null)
            {
                if (!GoldSystem.Instance.SpendGold(levelData.buildCost))
                {
                    if (BuildingAudioManager.Instance != null)
                        BuildingAudioManager.Instance.PlayInsufficientGold();

                    return;
                }
            }

            // Place tower
            GameObject placedTower = placementSystem.PlaceTower(selectedTowerData, gridPos, currentWaveNumber);
            if (placedTower != null)
            {
                actionHistory.RecordPlacement(placedTower, selectedTowerData, gridPos, levelData.buildCost);

                if (buildingHUD != null)
                    buildingHUD.UpdateGoldDisplay();

                if (BuildingAudioManager.Instance != null)
                    BuildingAudioManager.Instance.PlayTowerPlaced();
            }
            else
            {
                // Refund gold if placement failed
                if (GoldSystem.Instance != null)
                    GoldSystem.Instance.AddGold(levelData.buildCost);

                if (BuildingAudioManager.Instance != null)
                    BuildingAudioManager.Instance.PlayPlacementInvalid();
            }
        }

        private void TrySelectExistingTower()
        {
            if (buildingCamera == null || buildGrid == null)
                return;

            Vector3 mouseWorldPos = GetMouseWorldPosition();
            Vector2Int gridPos = buildGrid.WorldToGrid(mouseWorldPos);
            GameObject tower = buildGrid.GetTowerAtCell(gridPos);

            if (tower != null)
            {
                var tracker = tower.GetComponent<TowerInvestmentTracker>();
                if (tracker != null)
                {
                    SelectExistingTower(tower);
                    return;
                }
            }

            // Clicked empty space - deselect
            if (buildingHUD != null)
                buildingHUD.DeselectTower();
        }

        public void SelectExistingTower(GameObject towerObject)
        {
            if (towerObject == null)
                return;

            currentlySelectedTower = towerObject;

            if (buildingHUD != null)
                buildingHUD.ShowTowerContextPanel(towerObject);
        }

        #endregion

        #region Tower Management

        public void UpgradeTower(GameObject towerObject)
        {
            if (towerObject == null)
                return;

            var tracker = towerObject.GetComponent<TowerInvestmentTracker>();
            if (tracker == null)
                return;

            var towerData = towerDatabase.GetTowerByID(tracker.towerID);
            if (towerData == null)
                return;

            int nextLevel = tracker.currentLevel + 1;
            if (nextLevel >= towerData.MaxLevel)
            {
                if (BuildingAudioManager.Instance != null)
                    BuildingAudioManager.Instance.PlayMaxLevelReached();

                return;
            }

            var levelData = towerData.GetLevelData(nextLevel);
            if (levelData == null)
                return;

            if (GoldSystem.Instance != null)
            {
                if (!GoldSystem.Instance.SpendGold(levelData.upgradeCost))
                {
                    if (BuildingAudioManager.Instance != null)
                        BuildingAudioManager.Instance.PlayInsufficientGold();

                    return;
                }
            }

            tracker.AddUpgradeCost(levelData.upgradeCost);

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

            actionHistory.RecordUpgrade(towerObject, levelData.upgradeCost);

            if (buildingHUD != null)
            {
                buildingHUD.UpdateGoldDisplay();
                buildingHUD.RefreshTowerContextPanel(towerObject);
            }

            if (BuildingAudioManager.Instance != null)
                BuildingAudioManager.Instance.PlayTowerUpgraded();
        }

        public void SellTower(GameObject towerObject)
        {
            if (towerObject == null)
                return;

            var tracker = towerObject.GetComponent<TowerInvestmentTracker>();
            if (tracker == null)
                return;

            int sellValue = tracker.GetSellValue();

            if (currentlySelectedTower == towerObject)
                currentlySelectedTower = null;

            if (GoldSystem.Instance != null)
                GoldSystem.Instance.AddGold(sellValue);

            buildGrid.FreeCells(tracker.gridPosition, tracker.gridWidth, tracker.gridHeight);
            placementSystem.RemoveTower(towerObject);
            Destroy(towerObject);

            if (buildingHUD != null)
            {
                buildingHUD.UpdateGoldDisplay();
                buildingHUD.DeselectTower();
            }

            if (BuildingAudioManager.Instance != null)
                BuildingAudioManager.Instance.PlayTowerSold();
        }

        #endregion

        #region Undo & Reset

        public void Undo()
        {
            if (actionHistory.CanUndo())
            {
                actionHistory.Undo();

                if (buildingHUD != null)
                {
                    buildingHUD.UpdateGoldDisplay();
                    buildingHUD.DeselectTower();
                }

                if (BuildingAudioManager.Instance != null)
                    BuildingAudioManager.Instance.PlayUndo();
            }
        }

        public void ResetCurrentPhase()
        {
            actionHistory.ResetCurrentPhase();

            if (buildingHUD != null)
            {
                buildingHUD.UpdateGoldDisplay();
                buildingHUD.DeselectTower();
            }

            if (BuildingAudioManager.Instance != null)
                BuildingAudioManager.Instance.PlayReset();
        }

        #endregion

        #region Wave Integration

        private void OnDefencePhaseStarted()
        {
            canEnterBuildingMode = true;
            currentWaveNumber++;
            actionHistory.StartNewPhase();

            var allTowers = FindObjectsByType<TowerInvestmentTracker>(FindObjectsSortMode.None);
            foreach (var tracker in allTowers)
            {
                tracker.OnWaveStarted();
            }
        }

        private void OnDefencePhaseEnded()
        {
            canEnterBuildingMode = false;

            if (isBuildingMode)
                ExitBuildingMode();
        }

        #endregion

        #region Public Accessors

        public bool CanUndo()
        {
            return actionHistory != null && actionHistory.CanUndo();
        }

        public int GetCurrentPhaseInvestment()
        {
            return actionHistory != null ? actionHistory.GetCurrentPhaseInvestment() : 0;
        }

        public int GetCurrentPhaseTowerCount()
        {
            return actionHistory != null ? actionHistory.GetCurrentPhaseTowerCount() : 0;
        }

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
            if (currentGhost != null)
                Destroy(currentGhost.gameObject);
        }
    }
}