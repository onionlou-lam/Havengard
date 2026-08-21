using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Havengard.Core;
using Havengard.Core.Progression;
using Havengard.Resources;
using Havengard.Abilities;
using Havengard.Units;
using Havengard.Waves;
using Havengard.Items;
using Havengard.Core.Heroes;
using Havengard.Core.HealthSystem;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

namespace Havengard.DebugTools
{
    /// <summary>
    /// In-game debug/developer menu for testing and development.
    /// Press F1 to toggle menu visibility.
    /// </summary>
    public class HavengardDebugMenu : MonoBehaviour
    {
        [Header("Toggle Key")]
        [SerializeField] private KeyCode toggleKey = KeyCode.F1;

        [Header("UI References")]
        [SerializeField] private Canvas debugCanvas;
        [SerializeField] private GameObject menuPanel;

        [Header("Player Section")]
        [SerializeField] private InputField goldAmountInput;
        [SerializeField] private InputField celestiumAmountInput;
        [SerializeField] private InputField xpAmountInput;
        [SerializeField] private InputField skillPointsAmountInput;
        [SerializeField] private InputField levelUpAmountInput;

        [Header("Combat Section")]
        [SerializeField] private Dropdown enemyUnitDropdown;
        [SerializeField] private Dropdown bossUnitDropdown;
        [SerializeField] private Transform spawnPoint;

        [Header("World Section")]
        [SerializeField] private Slider gameSpeedSlider;
        [SerializeField] private Text gameSpeedText;
        [SerializeField] private Dropdown sceneDropdown;

        [Header("Abilities Section")]
        [SerializeField] private Toggle infiniteManaToggle;
        [SerializeField] private Toggle damageNumbersToggle;
        [SerializeField] private Dropdown abilityTestDropdown;

        [Header("Debug Visualization")]
        [SerializeField] private Toggle navMeshToggle;
        [SerializeField] private Toggle aiPathsToggle;

        [Header("Prefab References")]
        [SerializeField] private List<UnitBase> enemyPrefabs = new List<UnitBase>();
        [SerializeField] private List<UnitBase> bossPrefabs = new List<UnitBase>();

        private bool isMenuOpen;
        private GoldSystem goldSystem;
        private CelestiumSystem celestiumSystem;
        private WaveManager waveManager;
        private UnitSpawner unitSpawner;
        private HeroInstance playerHero;
        private AbilityUser playerAbilityUser;
        private bool infiniteManaEnabled;
        private bool damageNumbersEnabled = true;

        private void Awake()
        {
            // Ensure the canvas exists
            if (debugCanvas == null)
            {
                debugCanvas = GetComponent<Canvas>();
            }

            // Make sure canvas is ALWAYS ACTIVE but the menu panel is hidden
            if (debugCanvas != null)
            {
                debugCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                debugCanvas.sortingOrder = 9999;
                debugCanvas.gameObject.SetActive(true); // ALWAYS ACTIVE
            }

            // Initialize default values
            InitializeDefaultValues();
            SetupDropdowns();
            SetupListeners();

            // Start with menu panel hidden (not the canvas)
            isMenuOpen = false;
            if (menuPanel != null)
            {
                menuPanel.SetActive(false);
            }
            
            Debug.Log($"[HavengardDebugMenu] Initialized. Press {toggleKey} to toggle menu.");
        }

        private void Start()
        {
            // Cache references
            CacheReferences();
        }

        private void Update()
        {
            // Toggle menu with F1
            if (Input.GetKeyDown(toggleKey))
            {
                ToggleMenu();
            }

            // Infinite mana update
            if (infiniteManaEnabled && playerAbilityUser != null)
            {
                var resourceSystem = playerAbilityUser.GetComponent<ResourceSystem>();
                if (resourceSystem != null)
                {
                    resourceSystem.SetToMax();
                }
            }
        }

        private void InitializeDefaultValues()
        {
            if (goldAmountInput != null) goldAmountInput.text = "100";
            if (celestiumAmountInput != null) celestiumAmountInput.text = "100";
            if (xpAmountInput != null) xpAmountInput.text = "100";
            if (skillPointsAmountInput != null) skillPointsAmountInput.text = "1";
            if (levelUpAmountInput != null) levelUpAmountInput.text = "1";
            if (gameSpeedSlider != null) gameSpeedSlider.value = 1f;
        }

        private void SetupDropdowns()
        {
            // Enemy dropdown
            if (enemyUnitDropdown != null)
            {
                enemyUnitDropdown.ClearOptions();
                List<string> options = new List<string>();
                foreach (var enemy in enemyPrefabs)
                {
                    options.Add(enemy != null ? enemy.name : "Unknown");
                }
                if (options.Count > 0)
                {
                    enemyUnitDropdown.AddOptions(options);
                }
                else
                {
                    enemyUnitDropdown.AddOptions(new List<string> { "No enemies configured" });
                }
            }

            // Boss dropdown
            if (bossUnitDropdown != null)
            {
                bossUnitDropdown.ClearOptions();
                List<string> options = new List<string>();
                foreach (var boss in bossPrefabs)
                {
                    options.Add(boss != null ? boss.name : "Unknown");
                }
                if (options.Count > 0)
                {
                    bossUnitDropdown.AddOptions(options);
                }
                else
                {
                    bossUnitDropdown.AddOptions(new List<string> { "No bosses configured" });
                }
            }

            // Scene dropdown
            if (sceneDropdown != null)
            {
                sceneDropdown.ClearOptions();
                List<string> sceneNames = new List<string>();
                for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
                {
                    string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                    string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                    sceneNames.Add(sceneName);
                }
                sceneDropdown.AddOptions(sceneNames);
            }

            // Ability test dropdown
            if (abilityTestDropdown != null)
            {
                abilityTestDropdown.ClearOptions();
                abilityTestDropdown.AddOptions(new List<string> { "Select ability slot", "Slot 1", "Slot 2", "Slot 3", "Slot 4" });
            }
        }

        private void SetupListeners()
        {
            // Game speed slider
            if (gameSpeedSlider != null)
            {
                gameSpeedSlider.onValueChanged.AddListener(OnGameSpeedChanged);
            }

            // Infinite mana toggle
            if (infiniteManaToggle != null)
            {
                infiniteManaToggle.onValueChanged.AddListener(OnInfiniteManaToggled);
            }

            // Damage numbers toggle
            if (damageNumbersToggle != null)
            {
                damageNumbersToggle.isOn = true;
                damageNumbersToggle.onValueChanged.AddListener(OnDamageNumbersToggled);
            }

            // NavMesh toggle
            if (navMeshToggle != null)
            {
                navMeshToggle.onValueChanged.AddListener(OnNavMeshToggled);
            }

            // AI Paths toggle
            if (aiPathsToggle != null)
            {
                aiPathsToggle.onValueChanged.AddListener(OnAIPathsToggled);
            }
        }

        private void CacheReferences()
        {
            // GameManager systems
            if (GameManager.Instance != null)
            {
                goldSystem = GameManager.Instance.goldSystem;
                celestiumSystem = GameManager.Instance.celestiumSystem;
            }

            // Find player hero
            playerHero = FindFirstObjectByType<HeroInstance>();
            if (playerHero != null)
            {
                playerAbilityUser = playerHero.GetComponent<AbilityUser>();
            }

            // Wave manager
            waveManager = FindFirstObjectByType<WaveManager>();

            // Unit spawner (create if not found)
            unitSpawner = FindFirstObjectByType<UnitSpawner>();
            if (unitSpawner == null)
            {
                GameObject spawnerObj = new GameObject("DebugUnitSpawner");
                unitSpawner = spawnerObj.AddComponent<UnitSpawner>();
            }

            Debug.Log($"[HavengardDebugMenu] References cached - Gold: {goldSystem != null}, Celestium: {celestiumSystem != null}, Player: {playerHero != null}");
        }

        private void ToggleMenu()
        {
            isMenuOpen = !isMenuOpen;
            
            if (menuPanel != null)
            {
                menuPanel.SetActive(isMenuOpen);
            }
            
            Debug.Log($"[HavengardDebugMenu] Menu {(isMenuOpen ? "OPENED" : "CLOSED")}");
        }

        #region Player Functions

        public void AddGold()
        {
            Debug.Log("[DebugMenu] AddGold() called!");
            
            if (goldSystem == null)
            {
                Debug.LogWarning("[DebugMenu] GoldSystem not found!");
                return;
            }

            int amount = ParseInputField(goldAmountInput, 100);
            goldSystem.AddGold(amount);
            Debug.Log($"[DebugMenu] Added {amount} gold (Total: {goldSystem.CurrentGold})");
        }

        public void AddCelestium()
        {
            Debug.Log("[DebugMenu] AddCelestium() called!");
            
            if (celestiumSystem == null)
            {
                Debug.LogWarning("[DebugMenu] CelestiumSystem not found!");
                return;
            }

            int amount = ParseInputField(celestiumAmountInput, 100);
            celestiumSystem.AddCelestium(amount);
            Debug.Log($"[DebugMenu] Added {amount} celestium (Total: {celestiumSystem.CurrentCelestium})");
        }

        public void AddXP()
        {
            Debug.Log("[DebugMenu] AddXP() called!");
            
            if (playerHero == null || playerHero.ExpSystem == null)
            {
                Debug.LogWarning("[DebugMenu] Player hero or EXP system not found!");
                return;
            }

            int amount = ParseInputField(xpAmountInput, 100);
            playerHero.ExpSystem.AddEXP(amount);
            Debug.Log($"[DebugMenu] Added {amount} XP (Level: {playerHero.ExpSystem.CurrentLevel}, XP: {playerHero.ExpSystem.CurrentExp})");
        }

        public void AddSkillPoints()
        {
            Debug.Log("[DebugMenu] AddSkillPoints() called!");
            
            if (playerHero == null || playerHero.ExpSystem == null)
            {
                Debug.LogWarning("[DebugMenu] Player hero or EXP system not found!");
                return;
            }

            int amount = ParseInputField(skillPointsAmountInput, 1);
            playerHero.ExpSystem.AddSkillPoints(amount);
            Debug.Log($"[DebugMenu] Added {amount} skill points (Available: {playerHero.ExpSystem.AvailableSkillPoints})");
        }

        public void LevelUp()
        {
            Debug.Log("[DebugMenu] LevelUp() called!");
            
            if (playerHero == null || playerHero.ExpSystem == null)
            {
                Debug.LogWarning("[DebugMenu] Player hero or EXP system not found!");
                return;
            }

            int levels = ParseInputField(levelUpAmountInput, 1);
            
            for (int i = 0; i < levels; i++)
            {
                int xpNeeded = playerHero.ExpSystem.ExpToNextLevel;
                playerHero.ExpSystem.AddEXP(xpNeeded);
            }
            
            Debug.Log($"[DebugMenu] Leveled up {levels} time(s) (Current Level: {playerHero.ExpSystem.CurrentLevel})");
        }

        public void ResetSkills()
        {
            Debug.Log("[DebugMenu] ResetSkills() called!");
            
            if (playerHero == null)
            {
                Debug.LogWarning("[DebugMenu] Player hero not found!");
                return;
            }

            var expSystem = playerHero.ExpSystem;
            if (expSystem != null)
            {
                // Refund all spent skill points
                expSystem.RefundSkillPoints(expSystem.SpentSkillPoints);
            }

            var abilityUser = playerHero.GetComponent<AbilityUser>();
            if (abilityUser != null)
            {
                // Reset all unlocked abilities
                if (abilityUser.unlockedAbilities != null)
                {
                    for (int i = 0; i < abilityUser.unlockedAbilities.Length; i++)
                    {
                        abilityUser.unlockedAbilities[i] = false;
                    }
                }

                // Clear all abilities
                var abilities = abilityUser.GetAbilities();
                if (abilities != null)
                {
                    abilities.Clear();
                    abilityUser.RebuildCooldownArray();
                }
            }

            Debug.Log("[DebugMenu] Skills reset");
        }

        #endregion

        #region Combat Functions

        public void SpawnEnemyUnit()
        {
            Debug.Log("[DebugMenu] SpawnEnemyUnit() called!");
            
            if (unitSpawner == null || enemyPrefabs.Count == 0)
            {
                Debug.LogWarning("[DebugMenu] Unit spawner or enemy prefabs not configured!");
                return;
            }

            int index = enemyUnitDropdown != null ? enemyUnitDropdown.value : 0;
            if (index >= 0 && index < enemyPrefabs.Count)
            {
                Vector3 spawnPos = GetSpawnPosition();
                var enemy = unitSpawner.Spawn(enemyPrefabs[index], spawnPos);
                Debug.Log($"[DebugMenu] Spawned enemy: {enemy.name} at {spawnPos}");
            }
        }

        public void SpawnBossUnit()
        {
            Debug.Log("[DebugMenu] SpawnBossUnit() called!");
            
            if (unitSpawner == null || bossPrefabs.Count == 0)
            {
                Debug.LogWarning("[DebugMenu] Unit spawner or boss prefabs not configured!");
                return;
            }

            int index = bossUnitDropdown != null ? bossUnitDropdown.value : 0;
            if (index >= 0 && index < bossPrefabs.Count)
            {
                Vector3 spawnPos = GetSpawnPosition();
                var boss = unitSpawner.Spawn(bossPrefabs[index], spawnPos);
                Debug.Log($"[DebugMenu] Spawned boss: {boss.name} at {spawnPos}");
            }
        }

        public void KillAllEnemies()
        {
            Debug.Log("[DebugMenu] KillAllEnemies() called!");
            
            var enemies = FindObjectsByType<UnitBase>(FindObjectsSortMode.None);
            int count = 0;

            foreach (var enemy in enemies)
            {
                // Only kill enemies, not allies
                if (enemy.CompareTag("Enemy"))
                {
                    var health = enemy.GetComponent<Health>();
                    if (health != null)
                    {
                        health.TakeDamage(health.CurrentHealth);
                        count++;
                    }
                }
            }

            Debug.Log($"[DebugMenu] Killed {count} enemies");
        }

        public void StartWave()
        {
            Debug.Log("[DebugMenu] StartWave() called!");
            
            if (waveManager != null)
            {
                waveManager.StartNight();
                Debug.Log("[DebugMenu] Started wave");
            }
            else
            {
                Debug.LogWarning("[DebugMenu] WaveManager not found!");
            }
        }

        public void SkipWave()
        {
            Debug.Log("[DebugMenu] SkipWave() called!");
            
            if (waveManager != null)
            {
                // Kill all enemies to complete current wave
                KillAllEnemies();
                Debug.Log("[DebugMenu] Skipped wave");
            }
            else
            {
                Debug.LogWarning("[DebugMenu] WaveManager not found!");
            }
        }

        #endregion

        #region Abilities Functions

        public void ResetCooldowns()
        {
            Debug.Log("[DebugMenu] ResetCooldowns() called!");
            
            if (playerAbilityUser != null)
            {
                // Since ResetAllCooldowns doesn't exist, we'll use reflection to reset cooldowns
                var field = typeof(AbilityUser).GetField("nextReadyTimes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    var cooldownArray = field.GetValue(playerAbilityUser) as float[];
                    if (cooldownArray != null)
                    {
                        for (int i = 0; i < cooldownArray.Length; i++)
                        {
                            cooldownArray[i] = 0f;
                        }
                    }
                }
                Debug.Log("[DebugMenu] Reset all cooldowns");
            }
            else
            {
                Debug.LogWarning("[DebugMenu] Player AbilityUser not found!");
            }
        }

        public void TestAbility()
        {
            Debug.Log("[DebugMenu] TestAbility() called!");
            
            if (playerAbilityUser == null || abilityTestDropdown == null)
            {
                Debug.LogWarning("[DebugMenu] Player AbilityUser or dropdown not found!");
                return;
            }

            int slotIndex = abilityTestDropdown.value - 1; // -1 because first option is "Select ability slot"
            if (slotIndex >= 0)
            {
                // Get player's current position/target
                Vector3 targetPos = playerHero != null ? playerHero.transform.position + playerHero.transform.forward * 5f : Vector3.zero;
                playerAbilityUser.UseAbility(slotIndex, targetPos);
                Debug.Log($"[DebugMenu] Tested ability in slot {slotIndex}");
            }
        }

        private void OnInfiniteManaToggled(bool enabled)
        {
            infiniteManaEnabled = enabled;
            Debug.Log($"[DebugMenu] Infinite mana: {enabled}");
        }

        private void OnDamageNumbersToggled(bool enabled)
        {
            damageNumbersEnabled = enabled;
            
            // Toggle DamageNumbersPro if it exists
            var damageNumbers = FindObjectsByType<DamageNumbersPro.DamageNumber>(FindObjectsSortMode.None);
            foreach (var dn in damageNumbers)
            {
                dn.enabled = enabled;
            }

            Debug.Log($"[DebugMenu] Damage numbers: {enabled}");
        }

        #endregion

        #region World Functions

        public void ChangeScene()
        {
            Debug.Log("[DebugMenu] ChangeScene() called!");
            
            if (sceneDropdown == null) return;

            string sceneName = sceneDropdown.options[sceneDropdown.value].text;
            SceneManager.LoadScene(sceneName);
            Debug.Log($"[DebugMenu] Loading scene: {sceneName}");
        }

        private void OnGameSpeedChanged(float value)
        {
            Time.timeScale = value;
            if (gameSpeedText != null)
            {
                gameSpeedText.text = $"{value:F2}x";
            }
            Debug.Log($"[DebugMenu] Game speed: {value}x");
        }

        private void OnNavMeshToggled(bool enabled)
        {
            // Toggle NavMesh visualization
            NavMesh.avoidancePredictionTime = enabled ? 2.0f : 0.5f;
            Debug.Log($"[DebugMenu] NavMesh debug: {enabled}");
        }

        private void OnAIPathsToggled(bool enabled)
        {
            // Toggle AI path visualization
            var agents = FindObjectsByType<NavMeshAgent>(FindObjectsSortMode.None);
            foreach (var agent in agents)
            {
                var renderer = agent.GetComponent<LineRenderer>();
                if (renderer != null)
                {
                    renderer.enabled = enabled;
                }
            }
            Debug.Log($"[DebugMenu] AI paths: {enabled}");
        }

        #endregion

        #region Reset Functions

        public void ResetPlayer()
        {
            Debug.Log("[DebugMenu] ResetPlayer() called!");
            
            if (playerHero == null)
            {
                Debug.LogWarning("[DebugMenu] Player hero not found!");
                return;
            }

            // Reset health
            var health = playerHero.GetComponent<Health>();
            if (health != null)
            {
                health.Heal(health.MaxHealth);
            }

            // Reset resource
            var resource = playerHero.GetComponent<ResourceSystem>();
            if (resource != null)
            {
                resource.SetToMax();
            }

            // Reset position
            if (spawnPoint != null)
            {
                playerHero.transform.position = spawnPoint.position;
            }

            Debug.Log("[DebugMenu] Player reset");
        }

        public void ResetInventory()
        {
            Debug.Log("[DebugMenu] ResetInventory() called!");
            
            if (playerHero == null)
            {
                Debug.LogWarning("[DebugMenu] Player hero not found!");
                return;
            }

            var inventory = playerHero.GetComponent<ItemInventory>();
            if (inventory != null)
            {
                var items = new List<ItemInstance>(inventory.GetAllItems());
                foreach (var item in items)
                {
                    inventory.RemoveItem(item);
                }
                Debug.Log($"[DebugMenu] Inventory reset - removed {items.Count} items");
            }
        }

        public void ResetEverything()
        {
            Debug.Log("[DebugMenu] ResetEverything() called!");
            
            ResetPlayer();
            ResetSkills();
            ResetInventory();
            
            // Reset currencies
            if (goldSystem != null)
            {
                goldSystem.SpendGold(goldSystem.CurrentGold);
            }
            if (celestiumSystem != null)
            {
                celestiumSystem.SpendCelestium(celestiumSystem.CurrentCelestium);
            }

            // Reset XP and level
            if (playerHero != null && playerHero.ExpSystem != null)
            {
                playerHero.ExpSystem.ResetEXP();
            }

            Debug.Log("[DebugMenu] Everything reset");
        }

        #endregion

        #region Helper Functions

        private int ParseInputField(InputField field, int defaultValue)
        {
            if (field == null) return defaultValue;
            
            if (int.TryParse(field.text, out int result))
            {
                return result;
            }
            
            return defaultValue;
        }

        private Vector3 GetSpawnPosition()
        {
            if (spawnPoint != null)
            {
                return spawnPoint.position;
            }

            if (playerHero != null)
            {
                // Spawn 5 units in front of player
                return playerHero.transform.position + playerHero.transform.forward * 5f;
            }

            return Vector3.zero;
        }

        #endregion
    }
}