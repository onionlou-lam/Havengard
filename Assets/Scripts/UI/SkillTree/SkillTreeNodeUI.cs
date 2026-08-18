using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Havengard.Abilities;
using System.Collections;

namespace Havengard.UI
{
    /// <summary>
    /// Handles the visual behaviour of a main skill node with VFX/SFX feedback.
    /// </summary>
    public class SkillTreeNodeUI : MonoBehaviour,
        IPointerClickHandler,
        IPointerEnterHandler,
        IPointerExitHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        [Header("UI")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image borderImage;
        [SerializeField] private Image glowBorder;
        [SerializeField] private GameObject lockedOverlay;

        [Header("Visual Effects - Prefab References")]
        [SerializeField] private ParticleSystem clickParticlePrefab;
        [SerializeField] private ParticleSystem hoverParticlePrefab;
        [SerializeField] private ParticleSystem unlockParticlePrefab;
        [SerializeField] private ParticleSystem pulseParticlePrefab;

        [Header("Audio Effects")]
        [SerializeField] private AudioClip clickSound;
        [SerializeField] private AudioClip hoverSound;
        [SerializeField] private AudioClip unlockSound;
        [SerializeField] private AudioClip lockedSound;
        [Range(0f, 1f)]
        [SerializeField] private float sfxVolume = 0.7f;

        [Header("Colours")]
        [SerializeField] private Color unlockedColor = Color.white;
        [SerializeField] private Color lockedColor = Color.gray;
        [SerializeField] private Color availableColor = new Color(1f, 1f, 0.5f);

        [Header("Border")]
        [SerializeField] private Color normalBorderColor = Color.white;
        [SerializeField] private Color hoverBorderColor = new Color(1f, 0.9f, 0.4f);

        [Header("Glow")]
        [SerializeField] private Color glowColour = new Color(1f, 0.95f, 0.35f);
        [SerializeField] private float glowPulseSpeed = 4f;
        [SerializeField] private float glowMinAlpha = 0.2f;
        [SerializeField] private float glowMaxAlpha = 0.9f;

        [Header("Animation")]
        [SerializeField] private bool enableScaleAnimation = true;
        [SerializeField] private float scaleAmount = 1.1f;
        [SerializeField] private float scaleDuration = 0.15f;

        [Header("Tooltip")]
        [SerializeField] private bool enableTooltip = true;
        [Tooltip("If true, uses TooltipManager for ability tooltips. If false, uses SkillTreeTooltip.")]
        [SerializeField] private bool useTooltipManager = true;
        [Tooltip("Keep tooltip visible when node is selected")]
        [SerializeField] private bool keepTooltipOnSelect = true;
        [Tooltip("Show small hover tooltip with just name and level")]
        [SerializeField] private bool enableHoverTooltip = true;

        [Header("Investment Display")]
        [SerializeField] private GameObject investmentCounterObject;
        [SerializeField] private TextMeshProUGUI investmentCountText;
        [SerializeField] private TextMeshProUGUI investmentLevelText; // ✅ NEW: For display below node
        [SerializeField] private Color maxInvestmentColor = new Color(1f, 0.84f, 0f);

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = false;

        private Coroutine pulseRoutine;
        private Coroutine scaleRoutine;

        private int abilityIndex;
        private ClassAbility classAbility;
        private SkillTreeUI parentUI;
        private SkillTreeParticleManager particleManager;

        private bool isSelected;
        private bool isUnlocked;
        private bool canUnlock;
        private bool isHovering;
        private AudioSource audioSource;

        private ParticleSystem activePulseParticle;

        private int investmentLevel = 0;
        public const int MAX_INVESTMENT = 20;

        public int AbilityIndex => abilityIndex;
        public RectTransform RectTransform => GetComponent<RectTransform>();

        // Drag support
        private Canvas dragCanvas;
        private bool isDragging = false;
        private GameObject dragPreview; // Preview object for drag and drop

        //-----------------------------------------------------

        private void Awake()
        {
            if (glowBorder != null)
            {
                glowBorder.enabled = true;
                Color c = glowColour;
                c.a = 0f;
                glowBorder.color = c;
            }

            if (borderImage != null)
            {
                borderImage.color = normalBorderColor;
            }

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = sfxVolume;

            // ✅ Add this line
            dragCanvas = GetComponentInParent<Canvas>();
        }

        //-----------------------------------------------------

        public void Initialize(int index, ClassAbility ability, SkillTreeUI parent, SkillTreeParticleManager manager)
        {
            abilityIndex = index;
            classAbility = ability;
            parentUI = parent;
            particleManager = manager;

            if (iconImage != null && ability.ability != null && ability.ability.icon != null)
            {
                iconImage.sprite = ability.ability.icon;
            }

            UpdateInvestmentDisplay();
        }

        public void SetInvestmentLevel(int level)
        {
            investmentLevel = Mathf.Clamp(level, 0, MAX_INVESTMENT);
            UpdateInvestmentDisplay();
        }

        public int GetInvestmentLevel() => investmentLevel;
        public bool CanInvest() => investmentLevel < MAX_INVESTMENT;

        private void UpdateInvestmentDisplay()
        {
            // ✅ Update counter badge (optional - top right of node showing "5/20")
            if (investmentCounterObject != null)
            {
                bool shouldShow = isUnlocked && investmentLevel > 0;
                investmentCounterObject.SetActive(shouldShow);
            }

            if (investmentCountText != null && investmentLevel > 0)
            {
                investmentCountText.text = $"{investmentLevel}/{MAX_INVESTMENT}";
                
                if (investmentLevel >= MAX_INVESTMENT)
                {
                    investmentCountText.color = maxInvestmentColor;
                }
                else
                {
                    investmentCountText.color = Color.white;
                }
            }

            // ✅ Update investment level display below node ("+5" style)
            if (investmentLevelText != null)
            {
                if (isUnlocked && investmentLevel > 0)
                {
                    investmentLevelText.text = $"+{investmentLevel}";
                    investmentLevelText.gameObject.SetActive(true);
                    
                    // Color based on investment level
                    if (investmentLevel >= MAX_INVESTMENT)
                    {
                        investmentLevelText.color = maxInvestmentColor; // Gold
                    }
                    else if (investmentLevel >= 10)
                    {
                        investmentLevelText.color = new Color(0.5f, 1f, 0.5f); // Bright green
                    }
                    else if (investmentLevel >= 5)
                    {
                        investmentLevelText.color = new Color(0.8f, 1f, 0.8f); // Light green
                    }
                    else
                    {
                        investmentLevelText.color = Color.white; // White for 1-4
                    }
                }
                else
                {
                    investmentLevelText.gameObject.SetActive(false);
                }
            }
        }

        //-----------------------------------------------------

        public void RefreshState(bool[] unlockedAbilities, int availableSkillPoints, int playerLevel)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[SkillTreeNodeUI {abilityIndex}] RefreshState called");
            }

            if (unlockedAbilities == null)
            {
                Debug.LogError($"[SkillTreeNodeUI {abilityIndex}] unlockedAbilities is NULL!");
                SetVisualState(lockedColor, true);
                canUnlock = false;
                return;
            }

            if (abilityIndex < 0 || abilityIndex >= unlockedAbilities.Length)
            {
                Debug.LogError($"[SkillTreeNodeUI {abilityIndex}] Invalid index! Array length: {unlockedAbilities.Length}");
                SetVisualState(lockedColor, true);
                canUnlock = false;
                return;
            }

            isUnlocked = unlockedAbilities[abilityIndex];

            if (isUnlocked)
            {
                SetVisualState(unlockedColor, false);
                
                canUnlock = investmentLevel < MAX_INVESTMENT && availableSkillPoints > 0;
                
                UpdateInvestmentDisplay();
                
                if (!canUnlock)
                {
                    StopPulseParticle();
                }
                else if (availableSkillPoints > 0)
                {
                    if (activePulseParticle == null && pulseParticlePrefab != null && particleManager != null)
                    {
                        activePulseParticle = particleManager.PlayContinuousParticle(pulseParticlePrefab, RectTransform);
                    }
                }
                
                if (isSelected && enableTooltip)
                {
                    RefreshTooltipForSelectedNode();
                }
                
                return;
            }

            bool meetsLevel = playerLevel >= classAbility.requiredLevel;
            bool hasPoints = availableSkillPoints >= classAbility.skillPointCost;
            bool prereqsMet = classAbility.ArePrerequisitesMet(unlockedAbilities);

            canUnlock = meetsLevel && hasPoints && prereqsMet;

            if (canUnlock)
            {
                SetVisualState(availableColor, true);

                if (activePulseParticle == null && pulseParticlePrefab != null && particleManager != null)
                {
                    activePulseParticle = particleManager.PlayContinuousParticle(pulseParticlePrefab, RectTransform);
                }
            }
            else
            {
                SetVisualState(lockedColor, true);
                StopPulseParticle();
            }

            if (isSelected && enableTooltip)
            {
                RefreshTooltipForSelectedNode();
            }
        }

        //-----------------------------------------------------

        private void StopPulseParticle()
        {
            if (activePulseParticle != null && particleManager != null)
            {
                particleManager.StopContinuousParticle(activePulseParticle, pulseParticlePrefab);
                activePulseParticle = null;
            }
        }

        //-----------------------------------------------------

        private void SetVisualState(Color tint, bool showLocked)
        {
            if (iconImage != null)
                iconImage.color = tint;

            if (backgroundImage != null)
                backgroundImage.color = tint;

            if (lockedOverlay != null)
                lockedOverlay.SetActive(showLocked);
        }

        //-----------------------------------------------------

        public void SetSelected(bool selected)
        {
            isSelected = selected;

            if (borderImage != null)
            {
                borderImage.color = selected ? hoverBorderColor : normalBorderColor;
            }

            if (glowBorder == null)
                return;

            if (pulseRoutine != null)
            {
                StopCoroutine(pulseRoutine);
            }

            if (selected)
            {
                pulseRoutine = StartCoroutine(PulseGlow());
                
                if (enableTooltip && keepTooltipOnSelect)
                {
                    ShowTooltipInternal();
                }
            }
            else
            {
                Color c = glowBorder.color;
                c.a = 0;
                glowBorder.color = c;
                
                if (enableTooltip && !isHovering)
                {
                    HideTooltipInternal();
                }
            }
        }

        //-----------------------------------------------------

        public void OnPointerEnter(PointerEventData eventData)
        {
            // ✅ Don't show hover effects while dragging
            if (isDragging || AbilityDragHandler.IsDragging())
                return;

            // ✅ Don't show hover tooltip at all - only selected node shows tooltip
            if (isSelected)
                return;

            isHovering = true;

            if (!isSelected && borderImage != null)
            {
                borderImage.color = hoverBorderColor;
            }

            if (hoverParticlePrefab != null && particleManager != null)
            {
                particleManager.PlayParticleAtUI(hoverParticlePrefab, RectTransform);
            }

            PlaySound(hoverSound);

            if (enableScaleAnimation)
            {
                if (scaleRoutine != null)
                    StopCoroutine(scaleRoutine);
                scaleRoutine = StartCoroutine(ScaleAnimation(scaleAmount));
            }

            // ✅ Removed hover tooltip - only selected nodes show tooltips
        }

        //-----------------------------------------------------

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovering = false;

            if (!isSelected && borderImage != null)
            {
                borderImage.color = normalBorderColor;
            }

            if (enableScaleAnimation && !isSelected)
            {
                if (scaleRoutine != null)
                    StopCoroutine(scaleRoutine);
                scaleRoutine = StartCoroutine(ScaleAnimation(1f));
            }

            // ✅ No hover tooltip to hide
        }

        //-----------------------------------------------------

        public void OnPointerClick(PointerEventData eventData)
        {
            if (clickParticlePrefab != null && particleManager != null)
            {
                particleManager.PlayParticleAtUI(clickParticlePrefab, RectTransform);
            }

            if (isUnlocked)
                PlaySound(clickSound);
            else if (canUnlock)
                PlaySound(clickSound);
            else
                PlaySound(lockedSound);

            parentUI?.OnNodeClicked(abilityIndex, this);
        }

        //-----------------------------------------------------

        private void ShowHoverTooltip()
        {
            // Small tooltip implementation - can be added later
        }

        private void HideHoverTooltip()
        {
            // Hide hover tooltip implementation
        }

        private void RefreshTooltipForSelectedNode()
        {
            if (isSelected && keepTooltipOnSelect)
            {
                parentUI?.OnNodeClicked(abilityIndex, this);
            }
        }

        private void ShowTooltipInternal()
        {
            if (classAbility == null || classAbility.ability == null)
                return;

            if (useTooltipManager && TooltipManager.Instance != null)
            {
                TooltipManager.Instance.ShowAbilityTooltip(classAbility.ability);
            }
            else
            {
                var tooltip = FindObjectOfType<SkillTreeTooltip>();
                if (tooltip != null)
                {
                    Vector2 nodeWorldPos = RectTransform.position;
                    tooltip.ShowTooltip(classAbility, nodeWorldPos, isUnlocked, canUnlock);
                }
            }
        }

        //-----------------------------------------------------

        private void HideTooltipInternal()
        {
            if (useTooltipManager && TooltipManager.Instance != null)
            {
                TooltipManager.Instance.HideAbilityTooltip();
            }
            else
            {
                var tooltip = FindObjectOfType<SkillTreeTooltip>();
                if (tooltip != null)
                {
                    tooltip.HideTooltip();
                }
            }
        }

        //-----------------------------------------------------

        public void PlayUnlockEffects()
        {
            if (unlockParticlePrefab != null && particleManager != null)
            {
                particleManager.PlayParticleAtUI(unlockParticlePrefab, RectTransform);
            }

            PlaySound(unlockSound);

            if (scaleRoutine != null)
                StopCoroutine(scaleRoutine);
            scaleRoutine = StartCoroutine(UnlockScaleAnimation());
        }

        //-----------------------------------------------------

        private void PlaySound(AudioClip clip)
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip, sfxVolume);
            }
        }

        //-----------------------------------------------------

        private IEnumerator PulseGlow()
        {
            while (isSelected)
            {
                float t = Mathf.PingPong(Time.unscaledTime * glowPulseSpeed, 1f);
                float alpha = Mathf.Lerp(glowMinAlpha, glowMaxAlpha, t);

                Color c = glowColour;
                c.a = alpha;
                glowBorder.color = c;

                yield return null;
            }
        }

        //-----------------------------------------------------

        private IEnumerator ScaleAnimation(float targetScale)
        {
            Vector3 startScale = transform.localScale;
            Vector3 endScale = Vector3.one * targetScale;
            float elapsed = 0f;

            while (elapsed < scaleDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / scaleDuration;
                transform.localScale = Vector3.Lerp(startScale, endScale, t);
                yield return null;
            }

            transform.localScale = endScale;
        }

        //-----------------------------------------------------

        private IEnumerator UnlockScaleAnimation()
        {
            Vector3 originalScale = Vector3.one;
            Vector3 bigScale = originalScale * 1.3f;
            float duration = 0.2f;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(originalScale, bigScale, t);
                yield return null;
            }
            transform.localScale = originalScale;
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(bigScale, originalScale, t);
                yield return null;
            }

            transform.localScale = originalScale;
        }

        //-----------------------------------------------------

        public void OnBeginDrag(PointerEventData eventData)
        {
            // ✅ Only allow dragging unlocked abilities
            if (!isUnlocked || classAbility == null || classAbility.ability == null)
            {
                if (enableDebugLogs)
                {
                    Debug.Log($"[SkillTreeNodeUI] Cannot drag - Unlocked: {isUnlocked}, ClassAbility: {classAbility != null}, Ability: {classAbility?.ability != null}");
                }
                return;
            }

            isDragging = true;

            // ✅ Start tracking this ability as being dragged
            AbilityDragHandler.StartDrag(classAbility.ability);

            // ✅ Create visual drag preview
            CreateDragPreview();

            // Hide tooltip during drag
            HideTooltipInternal();
            HideHoverTooltip();

            Debug.Log($"[SkillTreeNodeUI] Started dragging: {classAbility.ability.abilityName}");
        }

        public void OnDrag(PointerEventData eventData)
        {
            // ✅ Update drag preview position to follow cursor
            if (dragPreview != null)
            {
                RectTransform previewRect = dragPreview.GetComponent<RectTransform>();
                if (previewRect != null && dragCanvas != null)
                {
                    Vector2 localPoint;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        dragCanvas.GetComponent<RectTransform>(),
                        eventData.position,
                        dragCanvas.worldCamera,
                        out localPoint
                    );
                    previewRect.localPosition = localPoint;
                }
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;

            // ✅ Destroy drag preview
            if (dragPreview != null)
            {
                Destroy(dragPreview);
                dragPreview = null;
            }

            Debug.Log($"[SkillTreeNodeUI] OnEndDrag - ability: {(classAbility?.ability != null ? classAbility.ability.abilityName : "NULL")}");
            
            // ✅ Check what we're hovering over
            var results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
            UnityEngine.EventSystems.EventSystem.current.RaycastAll(eventData, results);
            
            Debug.Log($"[SkillTreeNodeUI] OnEndDrag - Found {results.Count} raycast hits");
            
            bool droppedOnSlot = false;
            foreach (var result in results)
            {
                var slot = result.gameObject.GetComponent<AbilitySlotUI>();
                if (slot != null)
                {
                    Debug.Log($"[SkillTreeNodeUI] OnEndDrag - Found AbilitySlotUI on {result.gameObject.name}, slot index: {slot.GetSlotIndex()}");
                    droppedOnSlot = true;
                    break;
                }
            }
            
            // ✅ Only end drag if NOT dropped on a slot (let OnDrop handle it)
            if (!droppedOnSlot)
            {
                Debug.Log($"[SkillTreeNodeUI] OnEndDrag - Not dropped on slot, clearing drag state");
                AbilityDragHandler.ClearDraggedAbility();
            }
            else
            {
                Debug.Log($"[SkillTreeNodeUI] OnEndDrag - Dropped on slot, waiting for OnDrop to handle");
            }
        }

        /// <summary>
        /// Creates a visual preview that follows the cursor during drag
        /// </summary>
        private void CreateDragPreview()
        {
            if (dragCanvas == null || classAbility == null || classAbility.ability == null)
            {
                Debug.LogWarning("[SkillTreeNodeUI] Cannot create preview - missing canvas or ability");
                return;
            }

            try
            {
                // Simple approach: Create directly on root canvas
                Canvas rootCanvas = dragCanvas.rootCanvas ?? dragCanvas;
                
                dragPreview = new GameObject("SkillDragPreview");
                dragPreview.transform.SetParent(rootCanvas.transform, false);
                
                // Add Canvas with high sort order
                Canvas previewCanvas = dragPreview.AddComponent<Canvas>();
                previewCanvas.overrideSorting = true;
                previewCanvas.sortingOrder = 10000;
                
                // Add CanvasGroup
                CanvasGroup cg = dragPreview.AddComponent<CanvasGroup>();
                cg.alpha = 0.8f;
                cg.blocksRaycasts = false;
                cg.interactable = false;
                
                // Add RectTransform
                RectTransform rt = dragPreview.AddComponent<RectTransform>();
                rt.sizeDelta = new Vector2(60, 60);
                rt.position = Input.mousePosition;
                
                // Add icon image
                Image img = dragPreview.AddComponent<Image>();
                img.sprite = classAbility.ability.icon;
                img.raycastTarget = false;
                
                dragPreview.transform.SetAsLastSibling();
                
                Debug.Log($"[SkillTreeNodeUI] Created drag preview successfully");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SkillTreeNodeUI] Failed to create drag preview: {e.Message}");
            }
        }
    }
}