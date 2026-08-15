using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Havengard.Abilities;

namespace Havengard.UI
{
    /// <summary>
    /// Represents a single ability slot in the ability bar.
    /// Supports drag-and-drop from skill tree and slot rearrangement.
    /// </summary>
    public class AbilitySlotUI : MonoBehaviour, 
        IPointerEnterHandler, 
        IPointerExitHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IDropHandler
    {
        [Header("References")]
        [SerializeField] private Image abilityIcon;
        [SerializeField] private Image cooldownOverlay;
        [SerializeField] private TextMeshProUGUI keybindText;
        [SerializeField] private TextMeshProUGUI cooldownText;
        [SerializeField] private Image flashOverlay;
        [SerializeField] private Image slotBackground;

        [Header("Visual Settings")]
        [SerializeField] private Color availableColor = Color.white;
        [SerializeField] private Color onCooldownColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        [SerializeField] private Color flashColor = new Color(1f, 1f, 1f, 0.5f);
        [SerializeField] private Color dropHighlightColor = new Color(1f, 1f, 0f, 0.3f);
        [SerializeField] private float flashDuration = 0.2f;

        private AbilityBase ability;
        private int slotIndex;
        private float cooldownEndTime;
        private bool isOnCooldown;
        private float flashTimer;

        private AbilityBarUI parentBar;
        private Canvas dragCanvas;
        private GameObject dragPreview;
        private Color originalBackgroundColor;

        private void Awake()
        {
            if (cooldownOverlay != null)
            {
                cooldownOverlay.fillAmount = 0f;
                cooldownOverlay.enabled = true;
            }

            if (cooldownText != null)
                cooldownText.enabled = false;

            if (flashOverlay != null)
                flashOverlay.enabled = false;

            if (slotBackground != null)
                originalBackgroundColor = slotBackground.color;

            dragCanvas = GetComponentInParent<Canvas>();
        }

        private void Update()
        {
            UpdateCooldown();
            UpdateFlash();
        }

        /// <summary>
        /// Initialize this slot with its index and parent bar.
        /// </summary>
        public void Initialize(int index, AbilityBarUI bar)
        {
            slotIndex = index;
            parentBar = bar;
        }

        /// <summary>
        /// Sets the keybind text (e.g., "Q", "LMB", "RMB").
        /// </summary>
        public void SetKeybind(string keybind)
        {
            if (keybindText != null)
                keybindText.text = keybind;
        }

        /// <summary>
        /// Sets the ability for this slot.
        /// </summary>
        public void SetAbility(AbilityBase newAbility)
        {
            ability = newAbility;

            if (abilityIcon != null)
            {
                if (ability != null && ability.icon != null)
                {
                    abilityIcon.sprite = ability.icon;
                    abilityIcon.enabled = true;
                }
                else
                {
                    abilityIcon.sprite = null;
                    abilityIcon.enabled = false;
                }
            }

            ResetCooldown();
        }

        /// <summary>
        /// Starts the cooldown visual effect.
        /// </summary>
        public void StartCooldown(float duration)
        {
            if (duration <= 0f) return;

            cooldownEndTime = Time.time + duration;
            isOnCooldown = true;

            if (cooldownOverlay != null)
            {
                cooldownOverlay.fillAmount = 1f;
            }

            if (cooldownText != null)
                cooldownText.enabled = true;

            if (abilityIcon != null)
                abilityIcon.color = onCooldownColor;
        }

        /// <summary>
        /// Triggers a flash effect on the ability icon.
        /// </summary>
        public void Flash()
        {
            flashTimer = flashDuration;

            if (flashOverlay != null)
            {
                flashOverlay.color = flashColor;
                flashOverlay.enabled = true;
            }
        }

        /// <summary>
        /// Resets the cooldown state.
        /// </summary>
        public void ResetCooldown()
        {
            isOnCooldown = false;
            cooldownEndTime = 0f;

            if (cooldownOverlay != null)
                cooldownOverlay.fillAmount = 0f;

            if (cooldownText != null)
                cooldownText.enabled = false;

            if (abilityIcon != null)
                abilityIcon.color = availableColor;
        }

        private void UpdateCooldown()
        {
            if (!isOnCooldown) return;

            float now = Time.time;
            float remaining = cooldownEndTime - now;

            if (remaining <= 0f)
            {
                ResetCooldown();
                return;
            }

            // Update cooldown overlay (radial fill)
            if (cooldownOverlay != null && ability != null)
            {
                float cooldownDuration = ability.baseCooldown;
                if (cooldownDuration > 0f)
                {
                    float progress = remaining / cooldownDuration;
                    cooldownOverlay.fillAmount = progress;
                }
            }

            // Update cooldown text
            if (cooldownText != null)
            {
                if (remaining >= 1f)
                    cooldownText.text = Mathf.Ceil(remaining).ToString("F0");
                else
                    cooldownText.text = remaining.ToString("F1");
            }
        }

        private void UpdateFlash()
        {
            if (flashTimer <= 0f) return;

            flashTimer -= Time.deltaTime;

            if (flashOverlay != null)
            {
                if (flashTimer <= 0f)
                {
                    flashOverlay.enabled = false;
                }
                else
                {
                    // Fade out the flash
                    float alpha = Mathf.Lerp(0f, flashColor.a, flashTimer / flashDuration);
                    Color c = flashColor;
                    c.a = alpha;
                    flashOverlay.color = c;
                }
            }
        }

        /// <summary>
        /// Returns the ability assigned to this slot.
        /// </summary>
        public AbilityBase GetAbility()
        {
            return ability;
        }

        public int GetSlotIndex()
        {
            return slotIndex;
        }

        private void OnEnable()
        {
            Debug.Log($"[AbilitySlotUI] Slot {slotIndex} enabled - has IDropHandler: {this is IDropHandler}");
        }

        //-----------------------------------------------------
        // DRAG AND DROP IMPLEMENTATION
        //-----------------------------------------------------

        public void OnBeginDrag(PointerEventData eventData)
        {
            // Only allow dragging if there's an ability in this slot
            if (ability == null) return;

            // Create drag preview
            CreateDragPreview();

            // Hide tooltip while dragging
            if (TooltipManager.Instance != null)
            {
                TooltipManager.Instance.HideAbilityTooltip();
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (dragPreview != null)
            {
                // Move the drag preview to follow the cursor
                dragPreview.transform.position = eventData.position;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // Destroy the drag preview
            if (dragPreview != null)
            {
                Destroy(dragPreview);
                dragPreview = null;
            }

            // Check if we dropped on another ability slot
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (var result in results)
            {
                var targetSlot = result.gameObject.GetComponent<AbilitySlotUI>();
                if (targetSlot != null && targetSlot != this)
                {
                    // Swap abilities between slots
                    if (parentBar != null)
                    {
                        parentBar.SwapAbilities(slotIndex, targetSlot.GetSlotIndex());
                    }
                    return;
                }
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            Debug.Log($"[AbilitySlotUI] OnDrop called on slot {slotIndex}");
            
            // Restore background color
            if (slotBackground != null)
            {
                slotBackground.color = originalBackgroundColor;
            }

            // ✅ Safety check: Try to find parent bar if not initialized
            if (parentBar == null)
            {
                parentBar = GetComponentInParent<AbilityBarUI>();
                if (parentBar != null)
                {
                    Debug.LogWarning($"[AbilitySlotUI] parentBar was null, found via GetComponentInParent: {parentBar.name}");
                }
                else
                {
                    Debug.LogError($"[AbilitySlotUI] Could not find AbilityBarUI parent!");
                    return;
                }
            }

            // ✅ Check drag state first
            if (!AbilityDragHandler.IsDragging())
            {
                Debug.LogWarning($"[AbilitySlotUI] OnDrop - Not currently dragging!");
                return;
            }

            // Check if we're receiving an ability from the skill tree
            var draggedAbility = AbilityDragHandler.GetDraggedAbility();
            Debug.Log($"[AbilitySlotUI] Dragged ability from handler: {(draggedAbility != null ? draggedAbility.abilityName : "NULL")}");
            
            if (draggedAbility != null && parentBar != null)
            {
                Debug.Log($"[AbilitySlotUI] Assigning {draggedAbility.abilityName} to slot {slotIndex} via {parentBar.name}");
                parentBar.AssignAbilityToSlot(slotIndex, draggedAbility);
                
                // ✅ Clear drag state AFTER successful drop
                AbilityDragHandler.ClearDraggedAbility();
            }
            else
            {
                if (draggedAbility == null)
                    Debug.LogWarning($"[AbilitySlotUI] OnDrop - draggedAbility is NULL");
                if (parentBar == null)
                    Debug.LogWarning($"[AbilitySlotUI] OnDrop - parentBar is NULL");
                    
                // ✅ Clear drag state even if drop failed
                AbilityDragHandler.ClearDraggedAbility();
            }
        }

        private void CreateDragPreview()
        {
            if (dragCanvas == null || ability == null) return;

            // Create a new GameObject for the preview
            dragPreview = new GameObject("AbilityDragPreview");
            dragPreview.transform.SetParent(dragCanvas.transform, false);

            // Add RectTransform
            var rectTransform = dragPreview.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(50, 50); // Smaller preview

            // Add Image component
            var previewImage = dragPreview.AddComponent<Image>();
            previewImage.sprite = ability.icon;
            previewImage.raycastTarget = false;

            // Make it semi-transparent
            var color = previewImage.color;
            color.a = 0.7f;
            previewImage.color = color;

            // Position it at the cursor
            dragPreview.transform.position = Input.mousePosition;

            // Bring to front
            dragPreview.transform.SetAsLastSibling();
        }

        //-----------------------------------------------------
        // TOOLTIP / HOVER
        //-----------------------------------------------------

        public void OnPointerEnter(PointerEventData eventData)
        {
            // Highlight slot background when hovering during a drag
            if (AbilityDragHandler.IsDragging())
            {
                Debug.Log($"[AbilitySlotUI] Hovering over slot {slotIndex} during drag");
                if (slotBackground != null)
                {
                    slotBackground.color = dropHighlightColor;
                }
            }

            // Show tooltip
            if (ability != null && TooltipManager.Instance != null)
            {
                TooltipManager.Instance.ShowAbilityTooltip(ability);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // Restore background color
            if (slotBackground != null)
            {
                slotBackground.color = originalBackgroundColor;
            }

            // Hide tooltip
            if (TooltipManager.Instance != null)
            {
                TooltipManager.Instance.HideAbilityTooltip();
            }
        }
    }
}