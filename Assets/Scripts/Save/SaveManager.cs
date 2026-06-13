using UnityEngine;
using System.Collections.Generic;
using Havengard.Core.Heroes;
using Havengard.Items;
using Havengard.Resources;  // CHANGED: This is the correct namespace for currency

namespace Havengard.Save
{
    /// <summary>
    /// Central save/load manager - coordinates saving all game systems
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private string defaultSaveFileName = "GameSave";
        [SerializeField] private bool autoSaveEnabled = true;
        [SerializeField] private float autoSaveInterval = 300f; // 5 minutes

        [Header("System References")]
        [Tooltip("Assign the player's ItemInventory component")]
        [SerializeField] private ItemInventory playerInventory;
        
        [Tooltip("Assign the parent transform containing all placed buildings")]
        [SerializeField] private Transform buildingsParent;
        
        [Tooltip("Assign the parent transform containing all hero instances")]
        [SerializeField] private Transform heroesParent;
        
        // NEW: Add this field
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
        /// Save game to the default save file
        /// </summary>
        public void SaveGame()
        {
            SaveGame(defaultSaveFileName);
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
        /// Load game from the default save file
        /// </summary>
        public void LoadGame()
        {
            LoadGame(defaultSaveFileName);
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
            saveFileName ??= defaultSaveFileName;
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
            SaveGame("AutoSave");
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
            
            // Currency
            CollectCurrencyData(saveData);
            
            // Player Position (NEW)
            CollectPlayerPosition(saveData);
            
            // Heroes
            CollectHeroData(saveData);
            
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
                // Try to find player automatically
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerTransform = player.transform;
                }
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
            if (GoldSystem.Instance != null)
            {
                saveData.gold = GoldSystem.Instance.CurrentGold;
            }

            if (CelestiumSystem.Instance != null)
            {
                saveData.celestium = CelestiumSystem.Instance.CurrentCelestium;
            }

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
                    heroDataName = hero.Data.name,
                    level = hero.ExpSystem != null ? hero.ExpSystem.CurrentLevel : 1,
                    currentExp = hero.ExpSystem != null ? hero.ExpSystem.CurrentExp : 0,
                    isOnQuest = hero.IsOnQuest
                };

                // Save health/resource state
                var health = hero.GetComponent<Havengard.Core.HealthSystem.Health>();
                if (health != null)
                {
                    heroData.currentHealth = health.CurrentHealth;
                }

                var resourceSystem = hero.GetComponent<Havengard.Abilities.ResourceSystem>();
                if (resourceSystem != null)
                {
                    heroData.currentResource = resourceSystem.CurrentResource;
                }

                // Future: Save unlocked abilities (Phase 1)
                // heroData.unlockedAbilityIndices = GetUnlockedAbilityIndices(hero);

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
        /// Collect building placement data - only saves direct children of buildingsParent
        /// </summary>
        private void CollectBuildingData(GameSaveData saveData)
        {
            if (buildingsParent == null)
            {
                Debug.LogWarning("[SaveManager] buildingsParent not assigned - skipping building save");
                return;
            }
            
            // Get only DIRECT children (not nested children like particle effects)
            int childCount = buildingsParent.childCount;
            
            Debug.Log($"[SaveManager] Found {childCount} direct children of buildingsParent");
            
            for (int i = 0; i < childCount; i++)
            {
                Transform building = buildingsParent.GetChild(i);
                
                // Skip inactive objects
                if (!building.gameObject.activeInHierarchy)
                {
                    Debug.Log($"[SaveManager] Skipping inactive: {building.name}");
                    continue;
                }
                
                // Get the prefab name (clean up Unity's (Clone) suffix)
                string prefabName = building.name.Replace("(Clone)", "").Trim();
                
                BuildingSaveData buildingData = new BuildingSaveData(
                    prefabName,
                    building.position,
                    building.eulerAngles.y
                );
                
                saveData.placedBuildings.Add(buildingData);
                
                Debug.Log($"[SaveManager] Saving building: {prefabName} at {building.position}");
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
            
            // Currency
            ApplyCurrencyData(saveData);
            
            // Player Position (NEW)
            ApplyPlayerPosition(saveData);
            
            // Heroes
            ApplyHeroData(saveData);

            // Inventory
            ApplyInventoryData(saveData);

            // Buildings
            ApplyBuildingData(saveData);
        }
        
        /// <summary>
        /// Apply player position
        /// </summary>
        private void ApplyPlayerPosition(GameSaveData saveData)
        {
            if (playerTransform == null)
            {
                // Try to find player automatically
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerTransform = player.transform;
                }
            }
            
            if (playerTransform != null)
            {
                Vector3 savedPosition = saveData.GetPlayerPosition();
                playerTransform.position = savedPosition;
                Debug.Log($"[SaveManager] Restored player position: {savedPosition}");
            }
            else
            {
                Debug.LogWarning("[SaveManager] Player transform not assigned - skipping position load");
            }
        }

        /// <summary>
        /// Apply currency data
        /// </summary>
        private void ApplyCurrencyData(GameSaveData saveData)
        {
            if (GoldSystem.Instance != null)
            {
                GoldSystem.Instance.SetGold(saveData.gold);
            }
            
            if (CelestiumSystem.Instance != null)
            {
                CelestiumSystem.Instance.SetCelestium(saveData.celestium);
            }
            
            Debug.Log($"[SaveManager] Loaded currency: {saveData.gold} gold, {saveData.celestium} celestium");
        }
        
        /// <summary>
        /// Apply hero data - restore hero states
        /// </summary>
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
                // Find matching hero by HeroData name
                HeroInstance hero = System.Array.Find(existingHeroes, 
                    h => h.Data != null && h.Data.name == heroSaveData.heroDataName);
                
                if (hero == null)
                {
                    Debug.LogWarning($"[SaveManager] Could not find hero with HeroData: {heroSaveData.heroDataName}");
                    continue;
                }
                
                // Restore EXP and level
                if (hero.ExpSystem != null)
                {
                    hero.ExpSystem.SetEXPAndLevel(heroSaveData.currentExp, heroSaveData.level);
                }
                
                // Restore health
                var health = hero.GetComponent<Havengard.Core.HealthSystem.Health>();
                if (health != null)
                {
                    health.SetHealth(heroSaveData.currentHealth);
                }
                
                // Restore resource
                var resourceSystem = hero.GetComponent<Havengard.Abilities.ResourceSystem>();
                if (resourceSystem != null)
                {
                    resourceSystem.SetCurrentResource(heroSaveData.currentResource);
                }
                
                Debug.Log($"[SaveManager] Restored hero: {heroSaveData.heroDataName} (Level {heroSaveData.level}, {heroSaveData.currentHealth} HP)");
            }
            
            Debug.Log($"[SaveManager] Loaded {saveData.heroes.Count} heroes");
        }
        
        /// <summary>
        /// Apply inventory data
        /// </summary>
        private void ApplyInventoryData(GameSaveData saveData)
        {
            if (playerInventory == null)
            {
                Debug.LogWarning("[SaveManager] playerInventory not assigned - skipping inventory load");
                return;
            }
            
            // Clear existing inventory (this should trigger OnInventoryChanged)
            playerInventory.ClearInventory();
            
            // Load items from Resources folder
            foreach (ItemSaveData itemData in saveData.inventoryItems)
            {
                // Load the ItemData asset by name
                ItemData itemAsset = UnityEngine.Resources.Load<ItemData>($"Items/{itemData.itemDataName}");
                
                if (itemAsset == null)
                {
                    Debug.LogWarning($"[SaveManager] Could not find ItemData: {itemData.itemDataName}");
                    continue;
                }
                
                // Create ItemInstance and add to inventory
                ItemInstance itemInstance = new ItemInstance(itemAsset, itemData.currentLevel);
                bool added = playerInventory.TryAddItem(itemInstance);
                
                if (added)
                {
                    Debug.Log($"[SaveManager] Restored item: {itemData.itemDataName} (Level {itemData.currentLevel})");
                }
                else
                {
                    Debug.LogWarning($"[SaveManager] Failed to add item: {itemData.itemDataName}");
                }
            }
            
            Debug.Log($"[SaveManager] Loaded {saveData.inventoryItems.Count} items");
        }

        /// <summary>
        /// Apply building data - restore placed buildings
        /// </summary>
        private void ApplyBuildingData(GameSaveData saveData)
        {
            if (buildingsParent == null)
            {
                Debug.LogWarning("[SaveManager] buildingsParent not assigned - skipping building load");
                return;
            }

            // Clear existing buildings
            foreach (Transform child in buildingsParent)
            {
                Destroy(child.gameObject);
            }

            // Spawn saved buildings
            foreach (BuildingSaveData buildingData in saveData.placedBuildings)
            {
                // Load building prefab from Resources
                GameObject buildingPrefab = UnityEngine.Resources.Load<GameObject>($"Buildings/{buildingData.buildingPrefabName}");

                if (buildingPrefab == null)
                {
                    Debug.LogWarning($"[SaveManager] Could not find building prefab: {buildingData.buildingPrefabName}");
                    continue;
                }

                // Instantiate building
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