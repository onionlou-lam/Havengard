using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Havengard.Abilities;

namespace Havengard.UI.SkillTree
{
    public class SkillTreeTooltip : MonoBehaviour
    {
        [Header("UI Components (Auto-Assigned)")]
        [SerializeField] private GameObject tooltipPanel;
        [SerializeField] private TextMeshProUGUI abilityNameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI requirementsText;
        [SerializeField] private TextMeshProUGUI statusText;

        [Header("Layout")]
        [SerializeField] private Vector2 offset = new Vector2(50f, -50f); // Increased for visibility

        private RectTransform rectTransform;
        private Canvas parentCanvas;
        private bool isVisible = false;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            parentCanvas = GetComponentInParent<Canvas>();

            // Auto-find references if not assigned
            if (tooltipPanel == null)
                tooltipPanel = gameObject;

            if (abilityNameText == null)
            {
                Transform child = transform.Find("AbilityNameText");
                if (child != null)
                    abilityNameText = child.GetComponent<TextMeshProUGUI>();
            }

            if (descriptionText == null)
            {
                Transform child = transform.Find("DescriptionText");
                if (child != null)
                    descriptionText = child.GetComponent<TextMeshProUGUI>();
            }

            if (requirementsText == null)
            {
                Transform child = transform.Find("RequirementsText");
                if (child != null)
                    requirementsText = child.GetComponent<TextMeshProUGUI>();
            }

            if (statusText == null)
            {
                Transform child = transform.Find("StatusText");
                if (child != null)
                    statusText = child.GetComponent<TextMeshProUGUI>();
            }

            HideTooltip();
        }

        private void Update()
        {
            if (isVisible)
            {
                UpdatePosition();
            }
        }

        public void ShowTooltip(ClassAbility classAbility, Vector2 nodeWorldPosition, bool isUnlocked, bool canUnlock)
        {
            if (classAbility == null || classAbility.ability == null)
                return;

            if (tooltipPanel != null)
                tooltipPanel.SetActive(true);

            isVisible = true;

            // Set ability name
            if (abilityNameText != null)
            {
                abilityNameText.text = classAbility.ability.abilityName;
            }

            // Set description
            if (descriptionText != null)
            {
                descriptionText.text = classAbility.GetDescription();
            }

            // Set requirements
            if (requirementsText != null)
            {
                string reqText = "";
                reqText += $"Required Level: {classAbility.requiredLevel}\n";
                reqText += $"Skill Points: {classAbility.skillPointCost}";

                if (classAbility.HasPrerequisites())
                {
                    reqText += "\n[Prerequisites Required]";
                }

                requirementsText.text = reqText;
            }

            // Set status
            if (statusText != null)
            {
                if (isUnlocked)
                {
                    statusText.text = "<color=green>[UNLOCKED]</color>";
                }
                else if (canUnlock)
                {
                    statusText.text = "<color=yellow>[Click to Unlock]</color>";
                }
                else
                {
                    statusText.text = "<color=red>[Locked]</color>";
                }
            }

            UpdatePosition();
        }

        private void UpdatePosition()
        {
            if (rectTransform == null || parentCanvas == null)
                return;

            // Get mouse position and convert to canvas space
            Vector2 mouseScreenPos = Input.mousePosition;
            RectTransform canvasRect = parentCanvas.transform as RectTransform;
            Vector2 localPoint;
            Camera canvasCamera = parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main;

            bool success = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                mouseScreenPos,
                canvasCamera,
                out localPoint
            );

            if (!success)
                return;

            // Apply offset
            localPoint += offset;

            // Clamp to canvas bounds
            Rect canvasBounds = canvasRect.rect;
            float tooltipWidth = rectTransform.rect.width;
            float tooltipHeight = rectTransform.rect.height;

            // Keep tooltip on screen
            if (localPoint.x + tooltipWidth > canvasBounds.xMax)
            {
                localPoint.x = canvasBounds.xMax - tooltipWidth - 5f;
            }
            if (localPoint.x < canvasBounds.xMin)
            {
                localPoint.x = canvasBounds.xMin + 5f;
            }
            if (localPoint.y > canvasBounds.yMax)
            {
                localPoint.y = canvasBounds.yMax - 5f;
            }
            if (localPoint.y - tooltipHeight < canvasBounds.yMin)
            {
                localPoint.y = canvasBounds.yMin + tooltipHeight + 5f;
            }

            // Set position
            rectTransform.anchoredPosition = localPoint;
        }

        public void HideTooltip()
        {
            isVisible = false;

            if (tooltipPanel != null)
                tooltipPanel.SetActive(false);
        }
    }
}