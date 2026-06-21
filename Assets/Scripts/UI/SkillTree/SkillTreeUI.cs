using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Havengard.Abilities;
using Havengard.Core.Progression;

namespace Havengard.UI.SkillTree
{
    public class SkillTreeUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject skillTreePanel;
        [SerializeField] private Transform nodeContainer;
        [SerializeField] private SkillTreeTooltip tooltip;
        [SerializeField] private SkillTreeConnectionRenderer connectionRenderer;

        [Header("Node Prefab")]
        [SerializeField] private GameObject skillNodePrefab;

        [Header("Layout Settings")]
        [SerializeField] private Vector2 nodeSpacing = new Vector2(120f, 100f);
        [SerializeField] private Vector2 gridOffset = new Vector2(50f, 50f);

        [Header("Player Info Display")]
        [SerializeField] private TextMeshProUGUI skillPointsText;
        [SerializeField] private TextMeshProUGUI playerLevelText;

        // Runtime data
        private AbilityUser playerAbilityUser;
        private EXPSystem playerEXPSystem;
        private PlayerClass playerClass;
        private List<SkillTreeNodeUI> spawnedNodes = new List<SkillTreeNodeUI>();
        private bool isInitialized = false;

        // CRITICAL: Static constructor runs FIRST
        static SkillTreeUI()
        {
            Debug.Log("[SkillTreeUI] ★★★ STATIC CONSTRUCTOR CALLED ★★★");
        }

        private void Awake()
        {
            Debug.Log($"[SkillTreeUI] ★★★ AWAKE CALLED ★★★ enabled={enabled}, gameObject.activeInHierarchy={gameObject.activeInHierarchy}");
        }

        private void OnEnable()
        {
            Debug.Log("[SkillTreeUI] ★★★ OnEnable CALLED ★★★");
        }

        private void Start()
        {
            Debug.Log($"[SkillTreeUI] ★★★ START CALLED ★★★ enabled={enabled}");
        }

        private void OnDisable()
        {
            Debug.LogError("[SkillTreeUI] ★★★ OnDisable CALLED ★★★");
            Debug.LogError("Stack trace:\n" + System.Environment.StackTrace);
        }

        private void LateUpdate()
        {
            // Force re-enable if disabled
            if (!enabled)
            {
                Debug.LogError("[SkillTreeUI] Component disabled in LateUpdate! Re-enabling...");
                enabled = true;
            }
        }

        private void Update()
        {
            /*// Debug every 60 frames
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log($"[SkillTreeUI] Update running... Frame {Time.frameCount}, enabled={enabled}, initialized={isInitialized}");
            }
            */
            // Force re-enable if somehow disabled
            if (!enabled)
            {
                Debug.LogError("[SkillTreeUI] Component was disabled in Update! Re-enabling...");
                enabled = true;
                return;
            }

            // Toggle skill tree with K key
            if (Input.GetKeyDown(KeyCode.K))
            {
                Debug.Log($"[SkillTreeUI] ★★★★★ K KEY PRESSED ★★★★★");
                Debug.Log($"  enabled: {enabled}");
                Debug.Log($"  isInitialized: {isInitialized}");
                Debug.Log($"  skillTreePanel: {(skillTreePanel != null ? skillTreePanel.name : "NULL")}");
                Debug.Log($"  panel active: {(skillTreePanel != null ? skillTreePanel.activeSelf.ToString() : "N/A")}");
                ToggleSkillTree();
            }
        }

        public void Initialize(AbilityUser abilityUser, EXPSystem expSystem, PlayerClass playerClassData)
        {
            Debug.Log($"[SkillTreeUI] ★★★ INITIALIZE CALLED ★★★ enabled={enabled}");

            playerAbilityUser = abilityUser;
            playerEXPSystem = expSystem;
            playerClass = playerClassData;

            if (playerClass == null || playerClass.classAbilities == null)
            {
                Debug.LogError("[SkillTreeUI] No PlayerClass or abilities assigned!");
                return;
            }

            // Force enable
            if (!enabled)
            {
                Debug.LogWarning("[SkillTreeUI] Component was disabled! Re-enabling...");
                enabled = true;
            }

            BuildSkillTree();
            isInitialized = true;

            Debug.Log($"[SkillTreeUI] ✅ Initialized! enabled={enabled}, isInitialized={isInitialized}");
        }

        private void BuildSkillTree()
        {
            ClearNodes();

            if (playerClass.classAbilities.Length == 0)
            {
                Debug.LogWarning("[SkillTreeUI] No abilities in PlayerClass to display.");
                return;
            }

            for (int i = 0; i < playerClass.classAbilities.Length; i++)
            {
                ClassAbility classAbility = playerClass.classAbilities[i];
                if (classAbility.ability == null)
                {
                    Debug.LogWarning($"[SkillTreeUI] Ability at index {i} is null, skipping.");
                    continue;
                }

                GameObject nodeObj = Instantiate(skillNodePrefab, nodeContainer);
                RectTransform rectTransform = nodeObj.GetComponent<RectTransform>();

                Vector2 worldPos = new Vector2(
                    gridOffset.x + classAbility.treePosition.x * nodeSpacing.x,
                    gridOffset.y - classAbility.treePosition.y * nodeSpacing.y
                );
                rectTransform.anchoredPosition = worldPos;

                SkillTreeNodeUI nodeUI = nodeObj.GetComponent<SkillTreeNodeUI>();
                if (nodeUI != null)
                {
                    nodeUI.Initialize(i, classAbility, this);
                    spawnedNodes.Add(nodeUI);
                }
            }

            if (connectionRenderer != null)
            {
                connectionRenderer.DrawConnections(spawnedNodes, playerClass.classAbilities);
            }

            RefreshAllNodes();
        }

        public void RefreshAllNodes()
        {
            if (playerAbilityUser == null || playerEXPSystem == null)
                return;

            bool[] unlockedAbilities = playerAbilityUser.unlockedAbilities;

            foreach (SkillTreeNodeUI node in spawnedNodes)
            {
                node.RefreshState(unlockedAbilities, playerEXPSystem.AvailableSkillPoints, playerEXPSystem.CurrentLevel);
            }

            UpdatePlayerInfoDisplay();
        }

        public void OnNodeClicked(int abilityIndex)
        {
            if (playerAbilityUser == null || playerEXPSystem == null || playerClass == null)
                return;

            ClassAbility classAbility = playerClass.classAbilities[abilityIndex];

            if (playerAbilityUser.unlockedAbilities[abilityIndex])
            {
                Debug.Log($"[SkillTreeUI] {classAbility.ability.abilityName} is already unlocked.");
                return;
            }

            if (playerEXPSystem.CurrentLevel < classAbility.requiredLevel)
            {
                Debug.Log($"[SkillTreeUI] {classAbility.ability.abilityName} requires level {classAbility.requiredLevel}.");
                return;
            }

            if (playerEXPSystem.AvailableSkillPoints < classAbility.skillPointCost)
            {
                Debug.Log($"[SkillTreeUI] Not enough skill points to unlock {classAbility.ability.abilityName}.");
                return;
            }

            if (!classAbility.ArePrerequisitesMet(playerAbilityUser.unlockedAbilities))
            {
                Debug.Log($"[SkillTreeUI] Prerequisites not met for {classAbility.ability.abilityName}.");
                return;
            }

            if (!playerEXPSystem.TrySpendSkillPoints(classAbility.skillPointCost))
            {
                Debug.LogError($"[SkillTreeUI] Failed to spend skill points for {classAbility.ability.abilityName}.");
                return;
            }

            playerAbilityUser.UnlockAbility(abilityIndex, classAbility.ability);
            RefreshAllNodes();

            Debug.Log($"[SkillTreeUI] Unlocked {classAbility.ability.abilityName}! Skill Points Remaining: {playerEXPSystem.AvailableSkillPoints}");
        }

        private void UpdatePlayerInfoDisplay()
        {
            if (skillPointsText != null && playerEXPSystem != null)
            {
                skillPointsText.text = $"Skill Points: {playerEXPSystem.AvailableSkillPoints}";
            }

            if (playerLevelText != null && playerEXPSystem != null)
            {
                playerLevelText.text = $"Level: {playerEXPSystem.CurrentLevel}";
            }
        }

        private void ClearNodes()
        {
            foreach (SkillTreeNodeUI node in spawnedNodes)
            {
                if (node != null)
                    Destroy(node.gameObject);
            }
            spawnedNodes.Clear();
        }

        public void ToggleSkillTree()
        {
            Debug.Log($"[SkillTreeUI] ★★★ ToggleSkillTree CALLED ★★★");

            if (!isInitialized)
            {
                Debug.LogWarning("[SkillTreeUI] Not initialized yet!");
                return;
            }

            if (skillTreePanel == null)
            {
                Debug.LogError("[SkillTreeUI] skillTreePanel is NULL!");
                return;
            }

            bool isActive = skillTreePanel.activeSelf;
            Debug.Log($"[SkillTreeUI] Panel active before: {isActive}, will become: {!isActive}");

            if (!isActive)
            {
                if (UICanvasManager.Instance != null)
                {
                    UICanvasManager.Instance.HideAllMenuPanels();
                }
            }
            else
            {
                // CLOSING SKILL TREE - Hide tooltip
                HideTooltip(); // ADD THIS LINE
            }

            skillTreePanel.SetActive(!isActive);

            if (!isActive)
            {
                RefreshAllNodes();
                Time.timeScale = 0f;
                Debug.Log("[SkillTreeUI] ✅ SKILL TREE OPENED!");
            }
            else
            {
                Time.timeScale = 1f;
                Debug.Log("[SkillTreeUI] Skill tree closed");
            }
        }

        public void ShowTooltip(ClassAbility classAbility, Vector2 worldPosition, bool isUnlocked, bool canUnlock)
        {
            if (tooltip != null)
            {
                tooltip.ShowTooltip(classAbility, worldPosition, isUnlocked, canUnlock);
            }
        }

        public void HideTooltip()
        {
            if (tooltip != null)
            {
                tooltip.HideTooltip();
            }
        }
    }
}