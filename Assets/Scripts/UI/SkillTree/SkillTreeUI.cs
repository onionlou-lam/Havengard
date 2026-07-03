using Havengard.Abilities;
using Havengard.Core;
using Havengard.Core.Progression;
using Havengard.UI.SkillTree;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Havengard.UI
{
    /// <summary>
    /// Enhanced skill tree UI with tabs, fixed info panel, confirmation system, and VFX/SFX.
    /// </summary>
    public class SkillTreeUI : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject skillTreePanel;

        [Header("Specialization Tabs")]
        [SerializeField] private Button tab1Button;
        [SerializeField] private Button tab2Button;
        [SerializeField] private Button tab3Button;
        [SerializeField] private TextMeshProUGUI tab1Label;
        [SerializeField] private TextMeshProUGUI tab2Label;
        [SerializeField] private TextMeshProUGUI tab3Label;

        [Header("Tab Content Containers")]
        [SerializeField] private GameObject tab1Content;
        [SerializeField] private GameObject tab2Content;
        [SerializeField] private GameObject tab3Content;

        [Header("Node Containers (per tab)")]
        [SerializeField] private Transform nodeContainer1;
        [SerializeField] private Transform nodeContainer2;
        [SerializeField] private Transform nodeContainer3;

        [Header("Fixed Info Panel")]
        [SerializeField] private GameObject infoPanelObject;
        [SerializeField] private TextMeshProUGUI infoAbilityNameText;
        [SerializeField] private Image infoAbilityIcon;
        [SerializeField] private TextMeshProUGUI infoDescriptionText;
        [SerializeField] private TextMeshProUGUI infoRequirementsText;
        [SerializeField] private TextMeshProUGUI infoStatusText;
        [SerializeField] private Button confirmUnlockButton;
        [SerializeField] private TextMeshProUGUI confirmButtonText;

        [Header("Node Prefab")]
        [SerializeField] private GameObject skillNodePrefab;

        [Header("Layout Settings")]
        [SerializeField] private Vector2 nodeSpacing = new Vector2(120f, 100f);
        [SerializeField] private Vector2 gridOffset = new Vector2(50f, 50f);

        [Header("Player Info Display")]
        [SerializeField] private TextMeshProUGUI skillPointsText;
        [SerializeField] private TextMeshProUGUI playerLevelText;

        [Header("Connection Renderers (per tab)")]
        [SerializeField] private SkillTreeConnectionRenderer connectionRenderer1;
        [SerializeField] private SkillTreeConnectionRenderer connectionRenderer2;
        [SerializeField] private SkillTreeConnectionRenderer connectionRenderer3;

        [Header("Tab Colors")]
        [SerializeField] private Color activeTabColor = new Color(1f, 1f, 1f, 1f);
        [SerializeField] private Color inactiveTabColor = new Color(0.6f, 0.6f, 0.6f, 1f);

        [Header("UI Audio")]
        [SerializeField] private AudioClip tabSwitchSound;
        [SerializeField] private AudioClip skillUnlockedSound;
        [SerializeField] private AudioClip panelOpenSound;
        [SerializeField] private AudioClip panelCloseSound;
        [SerializeField] private AudioClip confirmButtonSound;
        [Range(0f, 1f)]
        [SerializeField] private float uiSfxVolume = 0.8f;

        [Header("VFX/SFX")]
        [SerializeField] private SkillTreeParticleManager particleManager;
        [SerializeField] private AudioSource audioSource;

        // Runtime data
        private AbilityUser playerAbilityUser;
        private EXPSystem playerEXPSystem;

        private PlayerClass spec1Class;
        private PlayerClass spec2Class;
        private PlayerClass spec3Class;

        private List<SkillTreeNodeUI> nodesTab1 = new List<SkillTreeNodeUI>();
        private List<SkillTreeNodeUI> nodesTab2 = new List<SkillTreeNodeUI>();
        private List<SkillTreeNodeUI> nodesTab3 = new List<SkillTreeNodeUI>();

        private int currentTabIndex = 0;
        private int lastAccessedTab = 0;

        private SkillTreeNodeUI selectedNode;
        private ClassAbility selectedAbility;
        private int selectedAbilityIndex;
        private PlayerClass selectedSpecialization;

        private bool isInitialized = false;

        //-----------------------------------------------------

        private void Awake()
        {
            // Hide skill tree at start
            if (skillTreePanel != null)
                skillTreePanel.SetActive(false);

            // Setup tab buttons
            if (tab1Button != null) tab1Button.onClick.AddListener(() => SwitchToTab(0));
            if (tab2Button != null) tab2Button.onClick.AddListener(() => SwitchToTab(1));
            if (tab3Button != null) tab3Button.onClick.AddListener(() => SwitchToTab(2));

            // Setup confirm button
            if (confirmUnlockButton != null)
            {
                confirmUnlockButton.onClick.AddListener(ConfirmUnlockSelectedAbility);
                confirmUnlockButton.interactable = false;
            }

            // Get audio source
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            audioSource.ignoreListenerPause = true; // ✅ UI sounds work during pause

            // Get particle manager (must be assigned in Inspector or exist in hierarchy)
            if (particleManager == null)
            {
                particleManager = GetComponentInChildren<SkillTreeParticleManager>();
                if (particleManager == null)
                {
                    Debug.LogError("[SkillTreeUI] SkillTreeParticleManager not found! Assign it in the Inspector.");
                }
            }
        }

        //-----------------------------------------------------

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                ToggleSkillTree();
            }
        }

        //-----------------------------------------------------

        public void Initialize(AbilityUser abilityUser, EXPSystem expSystem, PlayerClass mainClass)
        {
            if (mainClass == null)
            {
                Debug.LogError("[SkillTreeUI] Cannot initialize with null PlayerClass");
                return;
            }

            playerAbilityUser = abilityUser;
            playerEXPSystem = expSystem;

            if (mainClass.HasSpecializations())
            {
                spec1Class = mainClass.GetSpecialization(0);
                spec2Class = mainClass.GetSpecialization(1);
                spec3Class = mainClass.GetSpecialization(2);
                Debug.Log($"[SkillTreeUI] Initialized with specializations from {mainClass.className}");
            }
            else
            {
                spec1Class = mainClass;
                spec2Class = mainClass;
                spec3Class = mainClass;
                Debug.LogWarning($"[SkillTreeUI] {mainClass.className} has no specializations defined.");
            }

            SetupTabs();
            BuildAllTabs();
            isInitialized = true;

            Debug.Log("[SkillTreeUI] Initialized successfully");
        }

        //-----------------------------------------------------

        public void Initialize(AbilityUser abilityUser, EXPSystem expSystem, PlayerClass spec1, PlayerClass spec2, PlayerClass spec3)
        {
            playerAbilityUser = abilityUser;
            playerEXPSystem = expSystem;

            spec1Class = spec1;
            spec2Class = spec2;
            spec3Class = spec3;

            SetupTabs();
            BuildAllTabs();
            isInitialized = true;

            Debug.Log("[SkillTreeUI] Initialized with 3 separate specializations");
        }

        //-----------------------------------------------------

        private void SetupTabs()
        {
            if (tab1Label != null && spec1Class != null)
                tab1Label.text = spec1Class.GetTabName();
            if (tab2Label != null && spec2Class != null)
                tab2Label.text = spec2Class.GetTabName();
            if (tab3Label != null && spec3Class != null)
                tab3Label.text = spec3Class.GetTabName();
        }

        //-----------------------------------------------------

        private void BuildAllTabs()
        {
            BuildTabContent(0, spec1Class, nodeContainer1, connectionRenderer1, nodesTab1);
            BuildTabContent(1, spec2Class, nodeContainer2, connectionRenderer2, nodesTab2);
            BuildTabContent(2, spec3Class, nodeContainer3, connectionRenderer3, nodesTab3);
        }

        //-----------------------------------------------------

        private void BuildTabContent(int tabIndex, PlayerClass playerClass, Transform container,
    SkillTreeConnectionRenderer renderer, List<SkillTreeNodeUI> nodeList)
        {
            if (playerClass == null || playerClass.classAbilities == null || container == null)
            {
                Debug.LogWarning($"[SkillTreeUI] Tab {tabIndex} has missing data, skipping build");
                return;
            }

            // Clear existing nodes
            foreach (SkillTreeNodeUI node in nodeList)
            {
                if (node != null) Destroy(node.gameObject);
            }
            nodeList.Clear();

            Debug.Log($"[SkillTreeUI] Building Tab {tabIndex} with {playerClass.classAbilities.Length} abilities");

            // Create nodes
            for (int i = 0; i < playerClass.classAbilities.Length; i++)
            {
                ClassAbility classAbility = playerClass.classAbilities[i];
                if (classAbility.ability == null)
                {
                    Debug.LogWarning($"[SkillTreeUI] Tab {tabIndex}, ability {i} is NULL, skipping");
                    continue;
                }

                // Instantiate node prefab
                GameObject nodeObj = Instantiate(skillNodePrefab, container);

                if (nodeObj == null)
                {
                    Debug.LogError($"[SkillTreeUI] Tab {tabIndex}, failed to instantiate node prefab!");
                    continue;
                }

                RectTransform rectTransform = nodeObj.GetComponent<RectTransform>();

                Vector2 worldPos = new Vector2(
                    gridOffset.x + classAbility.treePosition.x * nodeSpacing.x,
                    gridOffset.y - classAbility.treePosition.y * nodeSpacing.y
                );
                rectTransform.anchoredPosition = worldPos;

                SkillTreeNodeUI nodeUI = nodeObj.GetComponent<SkillTreeNodeUI>();
                if (nodeUI != null)
                {
                    nodeUI.Initialize(i, classAbility, this, particleManager); // Add particleManager parameter
                    nodeList.Add(nodeUI);

                    // DEBUG: Check if particles are assigned after instantiation
                    if (nodeUI.GetComponentInChildren<ParticleSystem>() == null)
                    {
                        Debug.LogWarning($"[SkillTreeUI] Tab {tabIndex}, Node {i} ({classAbility.ability.abilityName}) has NO particle systems!");
                    }
                    else
                    {
                        int particleCount = nodeUI.GetComponentsInChildren<ParticleSystem>(true).Length;
                        Debug.Log($"[SkillTreeUI] Tab {tabIndex}, Node {i} ({classAbility.ability.abilityName}) has {particleCount} particle systems");
                    }
                }
                else
                {
                    Debug.LogError($"[SkillTreeUI] Tab {tabIndex}, Node {i} has no SkillTreeNodeUI component!");
                }
            }

            // Draw connections
            if (renderer != null)
            {
                renderer.DrawConnections(nodeList, playerClass.classAbilities);
            }

            Debug.Log($"[SkillTreeUI] ✅ Built tab {tabIndex} with {nodeList.Count} nodes");
        }

        //-----------------------------------------------------

        private void SwitchToTab(int tabIndex)
        {
            currentTabIndex = tabIndex;
            lastAccessedTab = tabIndex;

            if (tab1Content != null) tab1Content.SetActive(false);
            if (tab2Content != null) tab2Content.SetActive(false);
            if (tab3Content != null) tab3Content.SetActive(false);

            switch (tabIndex)
            {
                case 0:
                    if (tab1Content != null) tab1Content.SetActive(true);
                    selectedSpecialization = spec1Class;
                    break;
                case 1:
                    if (tab2Content != null) tab2Content.SetActive(true);
                    selectedSpecialization = spec2Class;
                    break;
                case 2:
                    if (tab3Content != null) tab3Content.SetActive(true);
                    selectedSpecialization = spec3Class;
                    break;
            }

            UpdateTabButtonVisuals();
            ClearSelection();
            RefreshCurrentTab();

            Debug.Log($"[SkillTreeUI] Switched to tab {tabIndex}");
        }

        //-----------------------------------------------------

        private void UpdateTabButtonVisuals()
        {
            if (tab1Button != null)
            {
                var colors = tab1Button.colors;
                colors.normalColor = (currentTabIndex == 0) ? activeTabColor : inactiveTabColor;
                tab1Button.colors = colors;
            }

            if (tab2Button != null)
            {
                var colors = tab2Button.colors;
                colors.normalColor = (currentTabIndex == 1) ? activeTabColor : inactiveTabColor;
                tab2Button.colors = colors;
            }

            if (tab3Button != null)
            {
                var colors = tab3Button.colors;
                colors.normalColor = (currentTabIndex == 2) ? activeTabColor : inactiveTabColor;
                tab3Button.colors = colors;
            }
        }

        //-----------------------------------------------------

        private void RefreshCurrentTab()
        {
            if (playerAbilityUser == null || playerEXPSystem == null) return;

            bool[] unlockedAbilities = playerAbilityUser.unlockedAbilities;
            List<SkillTreeNodeUI> currentNodes = GetCurrentTabNodes();

            foreach (SkillTreeNodeUI node in currentNodes)
            {
                node.RefreshState(unlockedAbilities, playerEXPSystem.AvailableSkillPoints,
                    playerEXPSystem.CurrentLevel);
            }

            UpdatePlayerInfoDisplay();
        }

        //-----------------------------------------------------

        public void RefreshAllNodes()
        {
            RefreshCurrentTab();
        }

        //-----------------------------------------------------

        private List<SkillTreeNodeUI> GetCurrentTabNodes()
        {
            return currentTabIndex switch
            {
                0 => nodesTab1,
                1 => nodesTab2,
                2 => nodesTab3,
                _ => nodesTab1
            };
        }

        //-----------------------------------------------------

        public void OnNodeClicked(int abilityIndex, SkillTreeNodeUI nodeUI)
        {
            // Deselect previous node
            if (selectedNode != null)
            {
                selectedNode.SetSelected(false);
            }

            if (selectedSpecialization == null || selectedSpecialization.classAbilities == null)
                return;

            if (abilityIndex < 0 || abilityIndex >= selectedSpecialization.classAbilities.Length)
                return;

            ClassAbility classAbility = selectedSpecialization.classAbilities[abilityIndex];
            selectedNode = nodeUI;
            selectedAbility = classAbility;
            selectedAbilityIndex = abilityIndex;

            // Highlight new node
            selectedNode.SetSelected(true);

            bool isAlreadyUnlocked = playerAbilityUser.unlockedAbilities[abilityIndex];
            ShowInfoPanel(classAbility, abilityIndex, nodeUI, isAlreadyUnlocked);
        }

        //-----------------------------------------------------

        private void ShowInfoPanel(ClassAbility classAbility, int abilityIndex,
            SkillTreeNodeUI nodeUI, bool isAlreadyUnlocked)
        {
            if (infoPanelObject != null)
                infoPanelObject.SetActive(true);

            if (infoAbilityNameText != null)
                infoAbilityNameText.text = classAbility.ability.abilityName;

            if (infoAbilityIcon != null && classAbility.ability.icon != null)
            {
                infoAbilityIcon.sprite = classAbility.ability.icon;
                infoAbilityIcon.enabled = true;
            }

            if (infoDescriptionText != null)
                infoDescriptionText.text = classAbility.GetDescription();

            if (infoRequirementsText != null)
            {
                string reqText = $"Required Level: {classAbility.requiredLevel}\n";
                reqText += $"Skill Points: {classAbility.skillPointCost}";

                if (classAbility.HasPrerequisites())
                {
                    reqText += "\n[Prerequisites Required]";
                }

                infoRequirementsText.text = reqText;
            }

            bool isUnlocked = playerAbilityUser.unlockedAbilities[abilityIndex];
            bool meetsLevel = playerEXPSystem.CurrentLevel >= classAbility.requiredLevel;
            bool hasPoints = playerEXPSystem.AvailableSkillPoints >= classAbility.skillPointCost;
            bool prereqsMet = classAbility.ArePrerequisitesMet(playerAbilityUser.unlockedAbilities);
            bool canUnlock = meetsLevel && hasPoints && prereqsMet && !isUnlocked;

            if (infoStatusText != null)
            {
                if (isUnlocked)
                    infoStatusText.text = "<color=green>[UNLOCKED]</color>";
                else if (canUnlock)
                    infoStatusText.text = "<color=yellow>[READY TO UNLOCK]</color>";
                else if (!meetsLevel)
                    infoStatusText.text = $"<color=red>[Requires Level {classAbility.requiredLevel}]</color>";
                else if (!hasPoints)
                    infoStatusText.text = "<color=red>[Not Enough Skill Points]</color>";
                else if (!prereqsMet)
                    infoStatusText.text = "<color=red>[Prerequisites Not Met]</color>";
                else
                    infoStatusText.text = "<color=red>[LOCKED]</color>";
            }

            if (confirmUnlockButton != null)
            {
                confirmUnlockButton.gameObject.SetActive(!isUnlocked);
                confirmUnlockButton.interactable = canUnlock;

                if (confirmButtonText != null)
                {
                    confirmButtonText.text = canUnlock ?
                        $"Unlock (-{classAbility.skillPointCost} SP)" :
                        "Cannot Unlock";
                }
            }
        }

        //-----------------------------------------------------

        private void ClearSelection()
        {
            if (selectedNode != null)
            {
                selectedNode.SetSelected(false);
            }

            selectedNode = null;
            selectedAbility = null;
            selectedAbilityIndex = -1;

            if (infoPanelObject != null)
                infoPanelObject.SetActive(false);
        }

        //-----------------------------------------------------

        private void ConfirmUnlockSelectedAbility()
        {
            if (selectedAbility == null || selectedSpecialization == null)
            {
                Debug.LogWarning("[SkillTreeUI] No ability selected to unlock");
                return;
            }

            if (playerAbilityUser == null || playerEXPSystem == null)
                return;

            // Validation checks
            if (playerAbilityUser.unlockedAbilities[selectedAbilityIndex])
            {
                Debug.Log($"[SkillTreeUI] {selectedAbility.ability.abilityName} is already unlocked.");
                ClearSelection();
                return;
            }

            if (playerEXPSystem.CurrentLevel < selectedAbility.requiredLevel ||
                playerEXPSystem.AvailableSkillPoints < selectedAbility.skillPointCost ||
                !selectedAbility.ArePrerequisitesMet(playerAbilityUser.unlockedAbilities))
            {
                Debug.Log("[SkillTreeUI] Requirements not met");
                return;
            }

            if (!playerEXPSystem.TrySpendSkillPoints(selectedAbility.skillPointCost))
            {
                Debug.LogError("[SkillTreeUI] Failed to spend skill points");
                return;
            }

            // UNLOCK THE ABILITY
            playerAbilityUser.UnlockAbility(selectedAbilityIndex, selectedAbility.ability);

            // PLAY UNLOCK EFFECTS
            if (selectedNode != null)
            {
                selectedNode.PlayUnlockEffects();
            }

            // Play global unlock sound
            PlayUISound(skillUnlockedSound);

            // Refresh and clear
            RefreshCurrentTab();

            // Delay clear selection to show unlock effects
            StartCoroutine(DelayedClearSelection(0.5f));

            Debug.Log($"[SkillTreeUI] ✅ Unlocked {selectedAbility.ability.abilityName}! " +
                $"Remaining SP: {playerEXPSystem.AvailableSkillPoints}");
        }

        //-----------------------------------------------------

        private IEnumerator DelayedClearSelection(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            ClearSelection();
        }

        //-----------------------------------------------------

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

        //-----------------------------------------------------

        public void ToggleSkillTree()
        {
            bool isActive = skillTreePanel.activeSelf;
            
            if (isActive)
            {
                // Closing skill tree
                skillTreePanel.SetActive(false);
                PlayUISound(panelCloseSound);
                
                // Unpause the game
                Time.timeScale = 1f;
                
                // Let UICanvasManager handle HUD visibility
                if (UICanvasManager.Instance != null)
                {
                    UICanvasManager.Instance.HideSkillTree();
                }
            }
            else
            {
                // Opening skill tree
                skillTreePanel.SetActive(true);
                PlayUISound(panelOpenSound);
                SwitchToTab(lastAccessedTab);
                
                // Pause the game
                Time.timeScale = 0f;
                
                // Let UICanvasManager handle HUD visibility
                if (UICanvasManager.Instance != null)
                {
                    UICanvasManager.Instance.ShowSkillTree();
                }
            }
        }

        //-----------------------------------------------------

        private void PlayUISound(AudioClip clip)
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip, uiSfxVolume);
            }
        }

        [ContextMenu("Debug All Tabs Particles")]
private void DebugAllTabsParticles()
{
    Debug.Log("=== CHECKING PARTICLES ON ALL TABS ===");
    
    CheckTabParticles("Tab 1 (Arcane)", nodesTab1);
    CheckTabParticles("Tab 2 (Fire)", nodesTab2);
    CheckTabParticles("Tab 3 (Frost)", nodesTab3);
    
    Debug.Log("=== CHECK COMPLETE ===");
}

private void CheckTabParticles(string tabName, List<SkillTreeNodeUI> nodes)
{
    Debug.Log($"\n--- {tabName} ---");
    Debug.Log($"Total nodes: {nodes.Count}");
    
    int nodesWithParticles = 0;
    int totalParticles = 0; int count = 0;
    
    foreach (var node in nodes)
    {
        if (node == null) continue;
        
        ParticleSystem[] particles = node.GetComponentsInChildren<ParticleSystem>(true);
        if (particles.Length > 0)
        {
            nodesWithParticles++;
            totalParticles += particles.Length;
            
            foreach (var ps in particles)
            {
                Debug.Log($"  Node {node.gameObject.name}: Particle '{ps.gameObject.name}' - Active: {ps.gameObject.activeInHierarchy}, Material: {(ps.GetComponent<ParticleSystemRenderer>()?.sharedMaterial?.name ?? "NULL")}");
            }
        }
        else
        {
            Debug.LogWarning($"  Node {node.gameObject.name}: NO PARTICLES!");
        }
    }
    
    Debug.Log($"{tabName} Summary: {nodesWithParticles}/{nodes.Count} nodes have particles ({totalParticles} total)");
}
    }
}