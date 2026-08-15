using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Havengard.Core.Progression;
using Havengard.Abilities;
using Havengard.Core.HealthSystem;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace Havengard.UI
{
    /// <summary>
    /// Main UI controller for the skill tree interface with 3 tabs for specializations
    /// </summary>
    public class SkillTreeUI : MonoBehaviour
    {
        // ✅ Debug toggle - set to false to disable all debug logs
        private const bool DEBUG_LOGS = false;

        [Header("UI References")]
        [SerializeField] private GameObject skillTreePanel;
        [SerializeField] private RectTransform leftPanel;
        [SerializeField] private RectTransform rightPanel;

        [Header("Tab System")]
        [SerializeField] private Button tab1Button;
        [SerializeField] private Button tab2Button;
        [SerializeField] private Button tab3Button;
        [SerializeField] private TextMeshProUGUI tab1Label;
        [SerializeField] private TextMeshProUGUI tab2Label;
        [SerializeField] private TextMeshProUGUI tab3Label;

        [Header("Scroll Views")]
        [SerializeField] private ScrollRect scrollRect1;
        [SerializeField] private ScrollRect scrollRect2;
        [SerializeField] private ScrollRect scrollRect3;
        [SerializeField] private GameObject skillTree1;
        [SerializeField] private GameObject skillTree2;
        [SerializeField] private GameObject skillTree3;

        [Header("Node Containers")]
        [SerializeField] private RectTransform nodeContainer1;
        [SerializeField] private RectTransform nodeContainer2;
        [SerializeField] private RectTransform nodeContainer3;

        [Header("Connection Renderers")]
        [SerializeField] private SkillTreeConnectionRenderer connectionRenderer1;
        [SerializeField] private SkillTreeConnectionRenderer connectionRenderer2;
        [SerializeField] private SkillTreeConnectionRenderer connectionRenderer3;

        [Header("Prefabs")]
        [SerializeField] private GameObject skillNodePrefab;
        [SerializeField] private GameObject subSkillNodePrefab;

        [Header("Info Panel")]
        [SerializeField] private TextMeshProUGUI infoTitleText;
        [SerializeField] private TextMeshProUGUI infoDescriptionText;
        [SerializeField] private TextMeshProUGUI infoRequirementsText;
        [SerializeField] private Image infoIconImage;
        [SerializeField] private Button confirmSkillButton;  // ← ADD THIS

        [Header("Player Info")]
        [SerializeField] private TextMeshProUGUI playerLevelText;
        [SerializeField] private TextMeshProUGUI skillPointsText;

        [Header("HUD Display in Skill Tree")]
        [Tooltip("Reference to Canvas_HUD to hide it when skill tree opens")]
        [SerializeField] private GameObject hudCanvas;

        [Tooltip("Optional: specific HUD elements to hide (wave counter, currency, etc)")]
        [SerializeField] private GameObject[] additionalHUDElementsToHide;

        [Header("HUD References for Display")]
        [Tooltip("Reference to the player's HealthSystem component")]
        [SerializeField] private HealthSystem playerHealthSystem;

        [Tooltip("Reference to the player's ResourceSystem component")]
        [SerializeField] private ResourceSystem playerResourceSystem;  // ← FIXED

        [Tooltip("UI elements in skill tree panel to display HUD info")]
        [SerializeField] private Image skillTreeHealthBarFill;
        [SerializeField] private Image skillTreeManaBarFill;
        [SerializeField] private Image skillTreeExpBarFill;
        [SerializeField] private TextMeshProUGUI skillTreeHealthText;
        [SerializeField] private TextMeshProUGUI skillTreeManaText;
        [SerializeField] private TextMeshProUGUI skillTreeExpText;
        [SerializeField] private Image skillTreePlayerPortrait;

        [Header("Audio")]
        [SerializeField] private AudioClip unlockSound;
        [SerializeField] private AudioClip buttonClickSound;

        [Header("Particles")]
        [SerializeField] private SkillTreeParticleManager particleManager;

        // Grid layout settings (MUST match SkillTreeGridEditor)
        private const float CELL_SIZE = 100f; // ✅ Changed from 60f to 100f
        private const int GRID_WIDTH = 15;    // ✅ Fixed grid width
        private const int GRID_HEIGHT = 12;   // ✅ Fixed grid height

        // Internal state
        private AbilityUser playerAbilityUser;
        private EXPSystem playerEXPSystem;
        private PlayerClass spec1Class;
        private PlayerClass spec2Class;
        private PlayerClass spec3Class;

        private int currentTabIndex = 0;
        private bool isInitialized = false;

        private List<SkillTreeNodeUI> nodesTab1 = new List<SkillTreeNodeUI>();
        private List<SkillTreeNodeUI> nodesTab2 = new List<SkillTreeNodeUI>();
        private List<SkillTreeNodeUI> nodesTab3 = new List<SkillTreeNodeUI>();

        private List<SubSkillNodeUI> subSkillNodesTab1 = new List<SubSkillNodeUI>();
        private List<SubSkillNodeUI> subSkillNodesTab2 = new List<SubSkillNodeUI>();
        private List<SubSkillNodeUI> subSkillNodesTab3 = new List<SubSkillNodeUI>();

        private SkillTreeNodeUI currentlySelectedNode;
        private SubSkillNodeUI currentlySelectedSubNode;

        private AudioSource audioSource;

        //-----------------------------------------------------

        private void Awake()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.ignoreListenerPause = true;

            if (particleManager == null)
            {
                particleManager = GetComponentInChildren<SkillTreeParticleManager>();
                if (particleManager == null && DEBUG_LOGS)
                {
                    Debug.LogError("[SkillTreeUI] SkillTreeParticleManager not found! Assign it in the Inspector.");
                }
            }

            // Setup confirm button
            if (confirmSkillButton != null)
            {
                confirmSkillButton.onClick.AddListener(OnConfirmSkillClicked);
            }
        }

        private SkillTreeNodeUI lastClickedNode;
        private int lastClickedAbilityIndex = -1;

        //-----------------------------------------------------

        private void Update()
        {
            if (!skillTreePanel.activeSelf)
                return;

            // Tab switching with 1, 2, 3 keys
            if (Input.GetKeyDown(KeyCode.Alpha1))
                SwitchToTab(0);
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                SwitchToTab(1);
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                SwitchToTab(2);

            // ESC to close
            if (Input.GetKeyDown(KeyCode.Escape))
                ToggleSkillTree();

            // ✅ Update HUD display every frame when skill tree is open
            UpdateHUDDisplay();
        }

        /// <summary>
        /// Update the HUD bars/text in the skill tree panel to reflect current player state
        /// </summary>
        private void UpdateHUDDisplay()
        {
            // Update health bar
            if (playerHealthSystem != null && skillTreeHealthBarFill != null)
            {
                float healthPercent = playerHealthSystem.CurrentHealth / playerHealthSystem.MaxHealth;
                skillTreeHealthBarFill.fillAmount = healthPercent;

                if (skillTreeHealthText != null)
                {
                    skillTreeHealthText.text = $"{Mathf.CeilToInt(playerHealthSystem.CurrentHealth)}/{Mathf.CeilToInt(playerHealthSystem.MaxHealth)}";
                }
            }

            // Update mana/resource bar
            if (playerResourceSystem != null && skillTreeManaBarFill != null)
            {
                float resourcePercent = (float)playerResourceSystem.CurrentResource / playerResourceSystem.MaxResource;
                skillTreeManaBarFill.fillAmount = resourcePercent;

                if (skillTreeManaText != null)
                {
                    skillTreeManaText.text = $"{playerResourceSystem.CurrentResource}/{playerResourceSystem.MaxResource}";
                }
            }

            // Update EXP bar
            if (playerEXPSystem != null && skillTreeExpBarFill != null)
            {
                float expPercent = (float)playerEXPSystem.CurrentEXP / playerEXPSystem.ExpToNextLevel;  // ← FIXED
                skillTreeExpBarFill.fillAmount = expPercent;

                if (skillTreeExpText != null)
                {
                    skillTreeExpText.text = $"{playerEXPSystem.CurrentEXP}/{playerEXPSystem.ExpToNextLevel} XP";  // ← FIXED
                }
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

            if (DEBUG_LOGS)
            {
                Debug.Log($"[SkillTreeUI] Initializing with Level: {expSystem?.CurrentLevel}, Skill Points: {expSystem?.AvailableSkillPoints}");
            }

            if (mainClass.HasSpecializations())
            {
                spec1Class = mainClass.GetSpecialization(0);
                spec2Class = mainClass.GetSpecialization(1);
                spec3Class = mainClass.GetSpecialization(2);
            }
            else
            {
                spec1Class = mainClass;
                spec2Class = mainClass;
                spec3Class = mainClass;
                if (DEBUG_LOGS)
                {
                    Debug.LogWarning($"[SkillTreeUI] {mainClass.className} has no specializations defined.");
                }
            }

            SetupTabs();
            BuildAllTabs();

            // Initialize unlock and sub-skill tracking for each specialization
            int totalAbilities = 0;
            if (spec1Class != null) totalAbilities = Mathf.Max(totalAbilities, spec1Class.GetAllAbilities().Length);
            if (spec2Class != null) totalAbilities = Mathf.Max(totalAbilities, spec2Class.GetAllAbilities().Length);
            if (spec3Class != null) totalAbilities = Mathf.Max(totalAbilities, spec3Class.GetAllAbilities().Length);

            if (totalAbilities > 0)
            {
                playerAbilityUser.InitializeUnlockTracking(totalAbilities);
                playerAbilityUser.InitializeSubSkillTracking(totalAbilities);
            }

            isInitialized = true;
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

            int totalAbilities = 0;
            if (spec1?.classAbilities != null) totalAbilities = Mathf.Max(totalAbilities, spec1.classAbilities.Length);
            if (spec2?.classAbilities != null) totalAbilities = Mathf.Max(totalAbilities, spec2.classAbilities.Length);
            if (spec3?.classAbilities != null) totalAbilities = Mathf.Max(totalAbilities, spec3.classAbilities.Length);

            if (totalAbilities > 0)
            {
                playerAbilityUser.InitializeUnlockTracking(totalAbilities);
                playerAbilityUser.InitializeSubSkillTracking(totalAbilities);
            }

            isInitialized = true;
        }

        private void AdjustLayout()
        {
            if (leftPanel == null || rightPanel == null) return;
            RectTransform canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
            float screenWidth = canvasRect.rect.width;
            float screenHeight = canvasRect.rect.height;
            
            // ✅ FIX: Reduced left panel width from 22% to 18%
            float leftPanelWidth = Mathf.Clamp(screenWidth * 0.18f, 220f, 350f);

            leftPanel.anchorMin = new Vector2(0, 0);
            leftPanel.anchorMax = new Vector2(leftPanelWidth / screenWidth, 1);
            leftPanel.offsetMin = Vector2.zero;
            leftPanel.offsetMax = Vector2.zero;

            rightPanel.anchorMin = new Vector2(leftPanel.anchorMax.x, 0);
            rightPanel.anchorMax = new Vector2(1, 1);
            rightPanel.offsetMin = new Vector2(10, 0);
            rightPanel.offsetMax = Vector2.zero;
        }

        private void SetupTabs()
        {
            tab1Label.text = spec1Class?.GetTabName() ?? "Tab 1";
            tab2Label.text = spec2Class?.GetTabName() ?? "Tab 2";
            tab3Label.text = spec3Class?.GetTabName() ?? "Tab 3";

            tab1Button.onClick.RemoveAllListeners();
            tab2Button.onClick.RemoveAllListeners();
            tab3Button.onClick.RemoveAllListeners();

            tab1Button.onClick.AddListener(() => SwitchToTab(0));
            tab2Button.onClick.AddListener(() => SwitchToTab(1));
            tab3Button.onClick.AddListener(() => SwitchToTab(2));
        }

        private void BuildAllTabs()
        {
            if (spec1Class != null) BuildTabContent(0, spec1Class, nodeContainer1, nodesTab1, subSkillNodesTab1, connectionRenderer1);
            if (spec2Class != null) BuildTabContent(1, spec2Class, nodeContainer2, nodesTab2, subSkillNodesTab2, connectionRenderer2);
            if (spec3Class != null) BuildTabContent(2, spec3Class, nodeContainer3, nodesTab3, subSkillNodesTab3, connectionRenderer3);
        }

        private void BuildTabContent(int tabIndex, PlayerClass playerClass, RectTransform container,
            List<SkillTreeNodeUI> nodeList, List<SubSkillNodeUI> subSkillList, SkillTreeConnectionRenderer renderer)
        {
            if (playerClass == null || playerClass.classAbilities == null)
                return;

            // ✅ Calculate content size
            float contentWidth = GRID_WIDTH * CELL_SIZE + 20f;   // 15 * 100 + 20 = 1520px
            float contentHeight = GRID_HEIGHT * CELL_SIZE;        // 12 * 100 = 1200px

            // ✅ Force container to stay at top-left with fixed size
            SetupContainerAnchors(container, contentWidth, contentHeight);

            // Clear existing nodes
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }
            nodeList.Clear();
            subSkillList.Clear();

            // Create main nodes
            for (int i = 0; i < playerClass.classAbilities.Length; i++)
            {
                ClassAbility classAbility = playerClass.classAbilities[i];
                GameObject nodeObj = Instantiate(skillNodePrefab, container);
                RectTransform nodeRect = nodeObj.GetComponent<RectTransform>();

                // ✅ Force anchor to top-left
                ForceTopLeftAnchor(nodeRect);

                Vector2 gridPos = classAbility.treePosition;

                // Position nodes
                nodeRect.anchoredPosition = new Vector2(
                    gridPos.x * CELL_SIZE,
                    -gridPos.y * CELL_SIZE
                );

                SkillTreeNodeUI nodeUI = nodeObj.GetComponent<SkillTreeNodeUI>();
                if (nodeUI != null)
                {
                    nodeUI.Initialize(i, classAbility, this, particleManager);
                    nodeList.Add(nodeUI);
                }

                // Create sub-skill nodes
                if (classAbility.HasSubSkills())
                {
                    for (int s = 0; s < classAbility.GetSubSkillCount(); s++)
                    {
                        SubSkillNodeData subData = classAbility.GetSubSkill(s);
                        if (subData == null) continue;

                        GameObject subNodeObj = Instantiate(subSkillNodePrefab, container);
                        RectTransform subNodeRect = subNodeObj.GetComponent<RectTransform>();

                        // ✅ Force anchor to top-left
                        ForceTopLeftAnchor(subNodeRect);

                        // Base position
                        Vector2 basePos = new Vector2(
                            gridPos.x * CELL_SIZE,
                            -gridPos.y * CELL_SIZE
                        );
                        
                        // Sub-skill offset
                        Vector2 scaledOffset = new Vector2(
                            subData.positionOffset.x * CELL_SIZE,
                            -subData.positionOffset.y * CELL_SIZE
                        );
                        
                        subNodeRect.anchoredPosition = basePos + scaledOffset;

                        SubSkillNodeUI subNodeUI = subNodeObj.GetComponent<SubSkillNodeUI>();
                        if (subNodeUI != null)
                        {
                            subNodeUI.Initialize(i, s, subData, this, particleManager);
                            subSkillList.Add(subNodeUI);
                        }
                    }
                }
            }

            // ✅ Force anchors again after all nodes created (in case Unity resets them)
            StartCoroutine(ForceAnchorsAfterFrame(container, nodeList, subSkillList));

            // Draw connections after nodes are created
            if (renderer != null)
            {
                renderer.DrawConnections(nodeList, subSkillList, playerClass.classAbilities);
            }
        }

        /// <summary>
        /// Force a RectTransform to use top-left anchoring
        /// </summary>
        private void ForceTopLeftAnchor(RectTransform rect)
        {
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0.5f, 0.5f);
        }

        /// <summary>
        /// Setup container with top-left anchoring and fixed size
        /// </summary>
        private void SetupContainerAnchors(RectTransform container, float width, float height)
        {
            container.anchorMin = new Vector2(0, 1);
            container.anchorMax = new Vector2(0, 1);
            container.pivot = new Vector2(0, 1);
            container.anchoredPosition = Vector2.zero;
            container.sizeDelta = new Vector2(width, height);
        }

        /// <summary>
        /// Force anchors to stay top-left after Unity's layout system runs
        /// </summary>
        private System.Collections.IEnumerator ForceAnchorsAfterFrame(RectTransform container, 
            List<SkillTreeNodeUI> nodes, List<SubSkillNodeUI> subNodes)
        {
            // Wait for end of frame to let Unity's layout system run
            yield return new WaitForEndOfFrame();

            // Force container
            SetupContainerAnchors(container, GRID_WIDTH * CELL_SIZE + 20f, GRID_HEIGHT * CELL_SIZE);

            // Force all nodes
            foreach (var node in nodes)
            {
                if (node != null)
                {
                    ForceTopLeftAnchor(node.RectTransform);
                }
            }

            // Force all sub-nodes
            foreach (var subNode in subNodes)
            {
                if (subNode != null)
                {
                    ForceTopLeftAnchor(subNode.GetComponent<RectTransform>());
                }
            }
        }

        public void SwitchToTab(int tabIndex)
        {
            currentTabIndex = tabIndex;

            skillTree1.SetActive(tabIndex == 0);
            skillTree2.SetActive(tabIndex == 1);
            skillTree3.SetActive(tabIndex == 2);

            UpdatePlayerInfo();
            RefreshAllNodes();
            DeselectAllNodes(); // Clear selection when switching tabs
        }

        public void RefreshAllNodes()
        {
            if (!isInitialized || playerAbilityUser == null || playerEXPSystem == null)
                return;

            bool[] unlockedAbilities = playerAbilityUser.unlockedAbilities;
            int skillPoints = playerEXPSystem.AvailableSkillPoints;
            int playerLevel = playerEXPSystem.CurrentLevel;

            List<SkillTreeNodeUI> currentNodes = currentTabIndex switch
            {
                0 => nodesTab1,
                1 => nodesTab2,
                2 => nodesTab3,
                _ => nodesTab1
            };

            List<SubSkillNodeUI> currentSubNodes = currentTabIndex switch
            {
                0 => subSkillNodesTab1,
                1 => subSkillNodesTab2,
                2 => subSkillNodesTab3,
                _ => subSkillNodesTab1
            };

            // Refresh main nodes
            foreach (var node in currentNodes)
            {
                if (node != null)
                {
                    node.RefreshState(unlockedAbilities, skillPoints, playerLevel);
                }
            }

            // Refresh sub-skill nodes
            foreach (var subNode in currentSubNodes)
            {
                if (subNode != null)
                {
                    int parentIndex = subNode.ParentAbilityIndex;
                    int subSkillIndex = subNode.SubSkillIndex;

                    bool parentUnlocked = playerAbilityUser.IsAbilityUnlocked(parentIndex);
                    bool anySubSkillUnlocked = playerAbilityUser.IsSubSkillUnlocked(parentIndex);
                    int selectedSubSkillIndex = playerAbilityUser.GetSelectedSubSkillIndex(parentIndex);

                    subNode.RefreshState(parentUnlocked, anySubSkillUnlocked, selectedSubSkillIndex,
                                        skillPoints, playerLevel);
                }
            }

            // ✅ Refresh connections after nodes update
            SkillTreeConnectionRenderer currentRenderer = currentTabIndex switch
            {
                0 => connectionRenderer1,
                1 => connectionRenderer2,
                2 => connectionRenderer3,
                _ => connectionRenderer1
            };

            PlayerClass currentClass = currentTabIndex switch
            {
                0 => spec1Class,
                1 => spec2Class,
                2 => spec3Class,
                _ => spec1Class
            };

            if (currentRenderer != null && currentClass != null)
            {
                currentRenderer.DrawConnections(currentNodes, currentSubNodes, currentClass.classAbilities);
            }
        }

        public void UpdatePlayerInfo()
        {
            if (playerEXPSystem != null)
            {
                playerLevelText.text = $"Level: {playerEXPSystem.CurrentLevel}";
                skillPointsText.text = $"Skill Points: {playerEXPSystem.AvailableSkillPoints}";
            }
        }

        public void OnNodeClicked(int abilityIndex, SkillTreeNodeUI nodeUI)
        {
            if (nodeUI == null) return;

            DeselectAllNodes();
            currentlySelectedNode = nodeUI;
            nodeUI.SetSelected(true);

            lastClickedNode = nodeUI;
            lastClickedAbilityIndex = abilityIndex;

            ClassAbility classAbility = GetClassAbilityFromNode(abilityIndex);
            if (classAbility == null || classAbility.ability == null)
                return;

            bool isAlreadyUnlocked = playerAbilityUser.IsAbilityUnlocked(abilityIndex);

            if (infoTitleText != null)
            {
                infoTitleText.text = classAbility.ability.abilityName;
            }

            if (infoIconImage != null && classAbility.ability.icon != null)
            {
                infoIconImage.sprite = classAbility.ability.icon;
                infoIconImage.enabled = true;
            }

            // ✅ IMPROVED: Better formatted description with clearer sections
            if (infoDescriptionText != null)
            {
                string description = "";
                
                // Main description
                description += classAbility.GetDescription();
                
                // Stats section
                description += "\n\n<color=#FFD700>═══════════════════</color>";
                description += "\n<b><color=#FFD700>STATS</color></b>";
                description += "\n<color=#FFD700>═══════════════════</color>";
                
                AbilityBase ability = classAbility.ability;
                
                if (ability.baseDamage > 0)
                    description += $"\n<color=#FF6B6B>► Damage:</color> {ability.baseDamage}";
                
                if (ability.baseCooldown > 0)
                    description += $"\n<color=#4ECDC4>► Cooldown:</color> {ability.baseCooldown:F1}s";
                
                if (ability.resourceCost > 0)
                    description += $"\n<color=#95E1D3>► Mana Cost:</color> {ability.resourceCost}";
                
                if (ability.range > 0)
                    description += $"\n<color=#F38181>► Range:</color> {ability.range:F0} units";

                // Investment section (if unlocked)
                if (classAbility.ability.investment != null && isAlreadyUnlocked)
                {
                    int investmentLevel = nodeUI.GetInvestmentLevel();
                    
                    description += "\n\n<color=#00FFFF>═══════════════════</color>";
                    description += "\n<b><color=#00FFFF>INVESTMENT</color></b>";
                    description += "\n<color=#00FFFF>═══════════════════</color>";
                    description += $"\n<color=yellow>Level: {investmentLevel}/{SkillTreeNodeUI.MAX_INVESTMENT}</color>";
                    
                    if (investmentLevel > 0)
                    {
                        string currentInfo = classAbility.ability.investment.GetCurrentLevelInfo(investmentLevel);
                        description += $"\n<color=lime>Current: {currentInfo}</color>";
                    }
                    
                    if (investmentLevel < SkillTreeNodeUI.MAX_INVESTMENT && playerEXPSystem.AvailableSkillPoints > 0)
                    {
                        string nextInfo = classAbility.ability.investment.GetNextLevelPreview();
                        description += $"\n<color=#FFD700>Next: {nextInfo}</color>";
                    }
                    
                    description += "\n<color=#00FFFF>═══════════════════</color>";
                }

                infoDescriptionText.text = description;
            }

            // ✅ IMPROVED: Better formatted requirements
            bool[] unlockedAbilities = playerAbilityUser.unlockedAbilities;
            bool meetsLevel = playerEXPSystem.CurrentLevel >= classAbility.requiredLevel;
            bool hasPoints = playerEXPSystem.AvailableSkillPoints >= classAbility.skillPointCost;
            bool prereqsMet = classAbility.ArePrerequisitesMet(unlockedAbilities);

            bool canUnlock = !isAlreadyUnlocked && meetsLevel && hasPoints && prereqsMet;

            int currentInvestment = nodeUI.GetInvestmentLevel();
            bool canInvest = nodeUI.CanInvest() && isAlreadyUnlocked && playerEXPSystem.AvailableSkillPoints > 0;

            if (infoRequirementsText != null)
            {
                string reqText = "";

                if (isAlreadyUnlocked)
                {
                    reqText = "<size=16><b><color=green>✓ UNLOCKED</color></b></size>\n";
                    
                    if (classAbility.ability.investment != null)
                    {
                        reqText += "\n<b>Investment Progress:</b>";
                        reqText += $"\n<color=yellow>{currentInvestment}</color> / <color=yellow>{SkillTreeNodeUI.MAX_INVESTMENT}</color>";

                        if (canInvest)
                        {
                            reqText += $"\n\n<color=lime><b>► Ready to Invest</b></color>";
                            reqText += $"\n<color=white>Cost: 1 Skill Point</color>";
                        }
                        else if (currentInvestment >= SkillTreeNodeUI.MAX_INVESTMENT)
                        {
                            reqText += $"\n\n<size=14><color=#FFD700><b>★ MAXED OUT ★</b></color></size>";
                        }
                        else if (playerEXPSystem.AvailableSkillPoints == 0)
                        {
                            reqText += $"\n\n<color=#FF6B6B>No skill points available</color>";
                        }
                    }
                }
                else
                {
                    reqText = "<size=14><b>REQUIREMENTS:</b></size>\n";
                    
                    reqText += $"\n<b>Player Level:</b> {classAbility.requiredLevel}";
                    reqText += meetsLevel ? " <color=green>✓</color>" : " <color=red>✗</color>";
                    
                    reqText += $"\n<b>Skill Points:</b> {classAbility.skillPointCost}";
                    reqText += hasPoints ? " <color=green>✓</color>" : " <color=red>✗</color>";

                    if (classAbility.HasPrerequisites())
                    {
                        reqText += $"\n<b>Prerequisites:</b>";
                        reqText += prereqsMet ? " <color=green>✓</color>" : " <color=red>✗</color>";
                    }
                    
                    if (!canUnlock)
                    {
                        reqText += "\n\n<color=#FF6B6B><b>Cannot unlock yet</b></color>";
                    }
                    else
                    {
                        reqText += "\n\n<color=lime><b>► Ready to Unlock!</b></color>";
                    }
                }

                infoRequirementsText.text = reqText;
            }

            if (confirmSkillButton != null)
            {
                bool showButton = canUnlock || canInvest;
                confirmSkillButton.gameObject.SetActive(showButton);
                
                TextMeshProUGUI buttonText = confirmSkillButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    if (canUnlock)
                    {
                        buttonText.text = $"<b>UNLOCK</b>\n<size=10>({classAbility.skillPointCost} SP)</size>";
                    }
                    else if (canInvest)
                    {
                        buttonText.text = "<b>INVEST</b>\n<size=10>(1 SP)</size>";
                    }
                }
            }

            PlaySound(buttonClickSound);
        }

        public void OnSubSkillNodeClicked(int parentIndex, int subSkillIndex, SubSkillNodeData subSkillData, SubSkillNodeUI subNodeUI)
        {
            if (subNodeUI == null || subSkillData == null) return;

            // ✅ Deselect all nodes first, then select this sub-skill
            DeselectAllNodes();
            currentlySelectedSubNode = subNodeUI;
            subNodeUI.SetSelected(true);

            if (infoTitleText != null)
            {
                infoTitleText.text = subSkillData.GetName();
            }

            if (infoIconImage != null)
            {
                Sprite icon = subSkillData.GetIcon();
                if (icon != null)
                {
                    infoIconImage.sprite = icon;
                    infoIconImage.enabled = true;
                }
            }

            if (infoDescriptionText != null)
            {
                infoDescriptionText.text = subSkillData.GetDescription();
            }

            bool isSelected = playerAbilityUser.IsSubSkillUnlocked(parentIndex) &&
                             playerAbilityUser.GetSelectedSubSkillIndex(parentIndex) == subSkillIndex;
            bool parentUnlocked = playerAbilityUser.IsAbilityUnlocked(parentIndex);
            bool meetsLevel = playerEXPSystem.CurrentLevel >= subSkillData.requiredLevel;
            bool hasPoints = playerEXPSystem.AvailableSkillPoints >= subSkillData.skillPointCost;

            if (infoRequirementsText != null)
            {
                string reqText = "";

                if (isSelected)
                {
                    reqText = "<color=green>SELECTED</color>";
                }
                else if (!parentUnlocked)
                {
                    reqText = "<color=red>Parent ability must be unlocked first</color>";
                }
                else
                {
                    reqText = $"Required Level: {subSkillData.requiredLevel}\n";
                    reqText += $"Skill Points: {subSkillData.skillPointCost}";
                }

                infoRequirementsText.text = reqText;
            }

            PlaySound(buttonClickSound);
        }

        /// <summary>
        /// Deselect all nodes in all tabs
        /// </summary>
        private void DeselectAllNodes()
        {
            foreach (var node in nodesTab1)
                if (node != null) node.SetSelected(false);
            foreach (var node in nodesTab2)
                if (node != null) node.SetSelected(false);
            foreach (var node in nodesTab3)
                if (node != null) node.SetSelected(false);

            foreach (var subNode in subSkillNodesTab1)
                if (subNode != null) subNode.SetSelected(false);
            foreach (var subNode in subSkillNodesTab2)
                if (subNode != null) subNode.SetSelected(false);
            foreach (var subNode in subSkillNodesTab3)
                if (subNode != null) subNode.SetSelected(false);

            currentlySelectedNode = null;
            currentlySelectedSubNode = null;
        }

        public void OnNodeUnlocked(int abilityIndex)
        {
            UpdatePlayerInfo();
            RefreshAllNodes();
            PlaySound(unlockSound);
        }

        public void OnSubSkillSelected(int abilityIndex, int subSkillIndex)
        {
            UpdatePlayerInfo();
            RefreshAllNodes();
            PlaySound(unlockSound);
        }

        public void PlaySound(AudioClip clip)
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        private ClassAbility GetClassAbilityFromNode(int abilityIndex)
        {
            PlayerClass currentClass = currentTabIndex switch
            {
                0 => spec1Class,
                1 => spec2Class,
                2 => spec3Class,
                _ => spec1Class
            };

            if (currentClass == null || currentClass.classAbilities == null ||
                abilityIndex < 0 || abilityIndex >= currentClass.classAbilities.Length)
                return null;

            return currentClass.classAbilities[abilityIndex];
        }

        public AbilityUser GetAbilityUser() => playerAbilityUser;
        public EXPSystem GetEXPSystem() => playerEXPSystem;

        //-----------------------------------------------------

        public void ToggleSkillTree()
        {
            bool isActive = !skillTreePanel.activeSelf;
            skillTreePanel.SetActive(isActive);

            if (isActive)
            {
                // ✅ Hide Canvas_HUD completely
                if (hudCanvas != null)
                    hudCanvas.SetActive(false);

                // ✅ Hide additional HUD elements (wave counter, currency, etc.)
                if (additionalHUDElementsToHide != null)
                {
                    foreach (var element in additionalHUDElementsToHide)
                    {
                        if (element != null)
                            element.SetActive(false);
                    }
                }

                Time.timeScale = 0f;
                RefreshAllNodes();
                UpdatePlayerInfo();
                SwitchToTab(currentTabIndex);
            }
            else
            {
                // ✅ Show Canvas_HUD again
                if (hudCanvas != null)
                    hudCanvas.SetActive(true);

                // ✅ Show additional HUD elements
                if (additionalHUDElementsToHide != null)
                {
                    foreach (var element in additionalHUDElementsToHide)
                    {
                        if (element != null)
                            element.SetActive(true);
                    }
                }

                Time.timeScale = 1f;
                DeselectAllNodes();
            }
        }

        private void OnConfirmSkillClicked()
        {
            if (lastClickedAbilityIndex < 0 || lastClickedNode == null)
                return;

            ClassAbility classAbility = GetClassAbilityFromNode(lastClickedAbilityIndex);
            if (classAbility == null || classAbility.ability == null)
                return;

            bool isAlreadyUnlocked = playerAbilityUser.IsAbilityUnlocked(lastClickedAbilityIndex);

            if (isAlreadyUnlocked)
            {
                // Handle investment
                if (classAbility.ability.investment != null && lastClickedNode.CanInvest())
                {
                    if (playerEXPSystem.AvailableSkillPoints >= 1)
                    {
                        if (playerEXPSystem.TrySpendSkillPoints(1))
                        {
                            int newLevel = lastClickedNode.GetInvestmentLevel() + 1;
                            lastClickedNode.SetInvestmentLevel(newLevel);
                            
                            // Apply investment to ability
                            if (classAbility.ability.investment != null)
                            {
                                classAbility.ability.investment.ApplyInvestment(newLevel, classAbility.ability);
                            }

                            Debug.Log($"[SkillTreeUI] Invested in {classAbility.ability.abilityName}. New level: {newLevel}");

                            RefreshAllNodes();
                            UpdatePlayerInfo();
                            OnNodeClicked(lastClickedAbilityIndex, lastClickedNode);
                            
                            PlaySound(unlockSound);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[SkillTreeUI] Not enough skill points for investment");
                    }
                }
                else
                {
                    Debug.LogWarning("[SkillTreeUI] Cannot invest more in this ability");
                    if (confirmSkillButton != null)
                        confirmSkillButton.gameObject.SetActive(false);
                }
                return;
            }

            // Handle unlock
            bool[] unlockedAbilities = playerAbilityUser.unlockedAbilities;
            bool meetsLevel = playerEXPSystem.CurrentLevel >= classAbility.requiredLevel;
            bool hasPoints = playerEXPSystem.AvailableSkillPoints >= classAbility.skillPointCost;
            bool prereqsMet = classAbility.ArePrerequisitesMet(unlockedAbilities);

            if (!meetsLevel || !hasPoints || !prereqsMet)
            {
                Debug.LogWarning("[SkillTreeUI] Cannot unlock ability - requirements not met");
                return;
            }

            if (!playerEXPSystem.TrySpendSkillPoints(classAbility.skillPointCost))
            {
                Debug.LogWarning("[SkillTreeUI] Failed to spend skill points");
                return;
            }

            playerAbilityUser.UnlockAbility(lastClickedAbilityIndex, classAbility.ability);

            if (lastClickedNode != null)
            {
                bool[] updatedUnlocked = playerAbilityUser.unlockedAbilities;
                int updatedSkillPoints = playerEXPSystem.AvailableSkillPoints;
                int updatedLevel = playerEXPSystem.CurrentLevel;

                lastClickedNode.RefreshState(updatedUnlocked, updatedSkillPoints, updatedLevel);
            }

            lastClickedNode.PlayUnlockEffects();

            RefreshAllNodes();
            UpdatePlayerInfo();
            OnNodeClicked(lastClickedAbilityIndex, lastClickedNode);

            Debug.Log($"[SkillTreeUI] Unlocked ability: {classAbility.ability.abilityName}");
        }
    }
}