using UnityEngine;
using Havengard.Items;
using Havengard.Core.Heroes;

namespace Havengard.Save
{
    /// <summary>
    /// Central save/load manager - coordinates saving all game systems
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private bool autoSaveEnabled = true;
        [SerializeField] private float autoSaveInterval = 300f; // 5 minutes

        [Header("System References")]
        [Tooltip("Assign the player's ItemInventory component")]
        [SerializeField] private ItemInventory playerInventory;

        [Tooltip("Assign the parent transform containing all placed buildings")]
        [SerializeField] private Transform buildingsParent;

        [Tooltip("Assign the parent transform containing all hero instances")]
        [SerializeField] private Transform heroesParent;

        [Tooltip("Assign the player GameObject (or its transform)")]
        [SerializeField] private Transform playerTransform;

        private float autoSaveTimer;
        private GameSaveData currentSaveData;

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Debug.Log("[SaveManager] Initialized");
        }

        private void Update()
        {
            // Auto-save timer
            if (autoSaveEnabled)
            {
                autoSaveTimer += Time.deltaTime;

                if (autoSaveTimer >= autoSaveInterval)
                {
                    autoSaveTimer = 0f;
                    AutoSave();
                }
            }
        }

        #region Public API

        /// <summary>
        /// Save game to the active save slot
        /// </summary>
        public void SaveGame()
        {
            if (SaveSlotManager.Instance != null)
            {
                string saveFileName = SaveSlotManager.Instance.GetActiveSaveFileName();
                SaveGame(saveFileName);
            }
            else
            {
                // Fallback if SaveSlotManager not available
                SaveGame("GameSave");
            }
        }

        /// <summary>
        /// Save game to a specific save file
        /// </summary>
        public void SaveGame(string saveFileName)
        {
            Debug.Log($"[SaveManager] Starting save to: {saveFileName}");

            GameSaveData saveData = CollectGameData();

            if (SaveUtility.SaveToFile(saveData, saveFileName))
            {
                currentSaveData = saveData;
                Debug.Log("[SaveManager] Save successful!");
                OnGameSaved?.Invoke(saveFileName);
            }
            else
            {
                Debug.LogError("[SaveManager] Save failed!");
            }
        }

        /// <summary>
        /// Load game from the active save slot
        /// </summary>
        public void LoadGame()
        {
            if (SaveSlotManager.Instance != null)
            {
                string saveFileName = SaveSlotManager.Instance.GetActiveSaveFileName();
                LoadGame(saveFileName);
            }
            else
            {
                // Fallback if SaveSlotManager not available
                LoadGame("GameSave");
            }
        }

        /// <summary>
        /// Load game from a specific save file
        /// </summary>
        public void LoadGame(string saveFileName)
        {
            Debug.Log($"[SaveManager] Starting load from: {saveFileName}");

            GameSaveData saveData = SaveUtility.LoadFromFile<GameSaveData>(saveFileName);

            if (saveData != null)
            {
                ApplyGameData(saveData);
                currentSaveData = saveData;

                // Update playtime tracker
                if (PlaytimeTracker.Instance != null)
                    PlaytimeTracker.Instance.SetTotalPlaytime(saveData.playTime);

                Debug.Log("[SaveManager] Load successful!");
                OnGameLoaded?.Invoke(saveFileName);
            }
            else
            {
                Debug.LogError("[SaveManager] Load failed!");
            }
        }

        /// <summary>
        /// Check if a save file exists
        /// </summary>
        public bool SaveExists(string saveFileName = null)
        {
            if (saveFileName == null)
            {
                if (SaveSlotManager.Instance != null)
                    saveFileName = SaveSlotManager.Instance.GetActiveSaveFileName();
                else
                    saveFileName = "GameSave";
            }

            return SaveUtility.SaveFileExists(saveFileName);
        }

        /// <summary>
        /// Delete a save file
        /// </summary>
        public bool DeleteSave(string saveFileName)
        {
            return SaveUtility.DeleteSaveFile(saveFileName);
        }

        /// <summary>
        /// Get list of all save files
        /// </summary>
        public string[] GetAllSaveFiles()
        {
            return SaveUtility.GetAllSaveFileNames();
        }

        #endregion

        #region Auto-Save

        private void AutoSave()
        {
            Debug.Log("[SaveManager] Auto-saving...");
            SaveGame();
        }

        #endregion

        #region Events

        public delegate void SaveEvent(string saveFileName);
        public event SaveEvent OnGameSaved;
        public event SaveEvent OnGameLoaded;

        #endregion

        #region Data Collection

        /// <summary>
        /// Collect all game state into a save data structure
        /// </summary>
        private GameSaveData CollectGameData()
        {
            GameSaveData saveData = new GameSaveData();

            // Playtime
            if (PlaytimeTracker.Instance != null)
                saveData.playTime = PlaytimeTracker.Instance.GetTotalPlaytime();

            // Currency
            CollectCurrencyData(saveData);

            // Player Position
            CollectPlayerPosition(saveData);

            // Heroes
            CollectHeroData(saveData);

            // Set main character info (first hero)
            if (saveData.heroes.Count > 0)
            {
                saveData.mainCharacterName = saveData.heroes[0].heroName;
                saveData.mainCharacterClass = saveData.heroes[0].className;
                saveData.mainCharacterLevel = saveData.heroes[0].level;
            }

            // Inventory
            CollectInventoryData(saveData);

            // Buildings
            CollectBuildingData(saveData);

            return saveData;
        }

        /// <summary>
        /// Collect player position
        /// </summary>
        private void CollectPlayerPosition(GameSaveData saveData)
        {
            if (playerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    playerTransform = player.transform;
            }

            if (playerTransform != null)
            {
                saveData.SetPlayerPosition(playerTransform.position);
                Debug.Log($"[SaveManager] Saved player position: {playerTransform.position}");
            }
            else
            {
                Debug.LogWarning("[SaveManager] Player transform not assigned - skipping position save");
            }
        }

        /// <summary>
        /// Collect currency data
        /// </summary>
        private void CollectCurrencyData(GameSaveData saveData)
        {
            if (Havengard.Resources.GoldSystem.Instance != null)
                saveData.gold = Havengard.Resources.GoldSystem.Instance.CurrentGold;

            if (Havengard.Resources.CelestiumSystem.Instance != null)
                saveData.celestium = Havengard.Resources.CelestiumSystem.Instance.CurrentCelestium;

            Debug.Log($"[SaveManager] Saved currency: {saveData.gold} gold, {saveData.celestium} celestium");
        }

        /// <summary>
        /// Collect hero data from all hero instances
        /// </summary>
        private void CollectHeroData(GameSaveData saveData)
        {
            if (heroesParent == null)
            {
                Debug.LogWarning("[SaveManager] heroesParent not assigned - skipping hero save");
                return;
            }

            HeroInstance[] heroes = heroesParent.GetComponentsInChildren<HeroInstance>();

            foreach (HeroInstance hero in heroes)
            {
                if (hero.Data == null)
                {
                    Debug.LogWarning($"[SaveManager] Hero {hero.name} has no HeroData - skipping");
                    continue;
                }

                HeroSaveData heroData = new HeroSaveData
                {
                    heroName = hero.Data.heroName,
                    className = hero.Data.heroClass != null ? hero.Data.heroClass.className : "Unknown",
                    heroDataName = hero.Data.name,
                    level = hero.ExpSystem != null ? hero.ExpSystem.CurrentLevel : 1,
                    currentExp = hero.ExpSystem != null ? hero.ExpSystem.CurrentExp : 0,
                    isOnQuest = hero.IsOnQuest
                };

                var health = hero.GetComponent<Havengard.Core.HealthSystem.Health>();
                if (health != null)
                    heroData.currentHealth = health.CurrentHealth;

                var resourceSystem = hero.GetComponent<Havengard.Abilities.ResourceSystem>();
                if (resourceSystem != null)
                    heroData.currentResource = resourceSystem.CurrentResource;

                saveData.heroes.Add(heroData);
            }

            Debug.Log($"[SaveManager] Saved {saveData.heroes.Count} heroes");
        }

        /// <summary>
        /// Collect inventory data
        /// </summary>
        private void CollectInventoryData(GameSaveData saveData)
        {
            if (playerInventory == null)
            {
                Debug.LogWarning("[SaveManager] playerInventory not assigned - skipping inventory save");
                return;
            }

            var items = playerInventory.Items;

            foreach (var itemSlot in items)
            {
                if (itemSlot.itemData == null) continue;

                ItemSaveData itemData = new ItemSaveData(
                    itemSlot.itemData.name,
                    itemSlot.currentLevel
                );

                saveData.inventoryItems.Add(itemData);
            }

            Debug.Log($"[SaveManager] Saved {saveData.inventoryItems.Count} items");
        }

        /// <summary>
        /// Collect building placement data
        /// </summary>
        private void CollectBuildingData(GameSaveData saveData)
        {
            if (buildingsParent == null)
            {
                Debug.LogWarning("[SaveManager] buildingsParent not assigned - skipping building save");
                return;
            }

            int childCount = buildingsParent.childCount;

            for (int i = 0; i < childCount; i++)
            {
                Transform building = buildingsParent.GetChild(i);

                if (!building.gameObject.activeInHierarchy)
                    continue;

                string prefabName = building.name.Replace("(Clone)", "").Trim();

                BuildingSaveData buildingData = new BuildingSaveData(
                    prefabName,
                    building.position,
                    building.eulerAngles.y
                );

                saveData.placedBuildings.Add(buildingData);
            }

            Debug.Log($"[SaveManager] Saved {saveData.placedBuildings.Count} buildings");
        }

        #endregion

        #region Data Application

        /// <summary>
        /// Apply loaded save data to all game systems
        /// </summary>
        private void ApplyGameData(GameSaveData saveData)
        {
            Debug.Log($"[SaveManager] Applying save data (Version: {saveData.saveVersion}, Date: {saveData.saveDate})");

            ApplyCurrencyData(saveData);
            ApplyPlayerPosition(saveData);
            ApplyHeroData(saveData);
            ApplyInventoryData(saveData);
            ApplyBuildingData(saveData);
        }

        private void ApplyPlayerPosition(GameSaveData saveData)
        {
            if (playerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    playerTransform = player.transform;
            }

            if (playerTransform != null)
            {
                Vector3 savedPosition = saveData.GetPlayerPosition();
                playerTransform.position = savedPosition;
                Debug.Log($"[SaveManager] Restored player position: {savedPosition}");
            }
        }

        private void ApplyCurrencyData(GameSaveData saveData)
        {
            if (Havengard.Resources.GoldSystem.Instance != null)
                Havengard.Resources.GoldSystem.Instance.SetGold(saveData.gold);

            if (Havengard.Resources.CelestiumSystem.Instance != null)
                Havengard.Resources.CelestiumSystem.Instance.SetCelestium(saveData.celestium);

            Debug.Log($"[SaveManager] Loaded currency: {saveData.gold} gold, {saveData.celestium} celestium");
        }

        private void ApplyHeroData(GameSaveData saveData)
        {
            if (heroesParent == null)
            {
                Debug.LogWarning("[SaveManager] heroesParent not assigned - skipping hero load");
                return;
            }

            HeroInstance[] existingHeroes = heroesParent.GetComponentsInChildren<HeroInstance>();

            foreach (HeroSaveData heroSaveData in saveData.heroes)
            {
                HeroInstance hero = System.Array.Find(existingHeroes,
                    h => h.Data != null && h.Data.name == heroSaveData.heroDataName);

                if (hero == null)
                {
                    Debug.LogWarning($"[SaveManager] Could not find hero with HeroData: {heroSaveData.heroDataName}");
                    continue;
                }

                if (hero.ExpSystem != null)
                    hero.ExpSystem.SetEXPAndLevel(heroSaveData.currentExp, heroSaveData.level);

                var health = hero.GetComponent<Havengard.Core.HealthSystem.Health>();
                if (health != null)
                    health.SetHealth(heroSaveData.currentHealth);

                var resourceSystem = hero.GetComponent<Havengard.Abilities.ResourceSystem>();
                if (resourceSystem != null)
                    resourceSystem.SetCurrentResource(heroSaveData.currentResource);
            }

            Debug.Log($"[SaveManager] Loaded {saveData.heroes.Count} heroes");
        }

        private void ApplyInventoryData(GameSaveData saveData)
        {
            if (playerInventory == null)
            {
                Debug.LogWarning("[SaveManager] playerInventory not assigned - skipping inventory load");
                return;
            }

            playerInventory.ClearInventory();

            foreach (ItemSaveData itemData in saveData.inventoryItems)
            {
                ItemData itemAsset = UnityEngine.Resources.Load<ItemData>($"Items/{itemData.itemDataName}");

                if (itemAsset == null)
                {
                    Debug.LogWarning($"[SaveManager] Could not find ItemData: {itemData.itemDataName}");
                    continue;
                }

                ItemInstance itemInstance = new ItemInstance(itemAsset, itemData.currentLevel);
                playerInventory.TryAddItem(itemInstance);
            }

            Debug.Log($"[SaveManager] Loaded {saveData.inventoryItems.Count} items");
        }

        private void ApplyBuildingData(GameSaveData saveData)
        {
            if (buildingsParent == null)
            {
                Debug.LogWarning("[SaveManager] buildingsParent not assigned - skipping building load");
                return;
            }

            foreach (Transform child in buildingsParent)
                Destroy(child.gameObject);

            foreach (BuildingSaveData buildingData in saveData.placedBuildings)
            {
                GameObject buildingPrefab = UnityEngine.Resources.Load<GameObject>($"Buildings/{buildingData.buildingPrefabName}");

                if (buildingPrefab == null)
                {
                    Debug.LogWarning($"[SaveManager] Could not find building prefab: {buildingData.buildingPrefabName}");
                    continue;
                }

                GameObject building = Instantiate(
                    buildingPrefab,
                    buildingData.GetPosition(),
                    buildingData.GetRotation(),
                    buildingsParent
                );

                building.name = buildingData.buildingPrefabName;
            }

            Debug.Log($"[SaveManager] Loaded {saveData.placedBuildings.Count} buildings");
        }

        #endregion
    }
}