using UnityEngine;
using TMPro;

namespace Havengard.Interactions
{
    /// <summary>
    /// Tooltip UI that appears above interactable objects.
    /// Shows prompt text and key/button hint.
    /// </summary>
    public class InteractionTooltip : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField]
        [Tooltip("Text component for the prompt (e.g., 'Enter Inn')")]
        private TextMeshProUGUI promptText;

        [SerializeField]
        [Tooltip("Text component for the key hint (e.g., 'E')")]
        private TextMeshProUGUI keyText;

        [Header("Visual Settings")]
        [SerializeField]
        [Tooltip("Offset above the target position")]
        private Vector3 heightOffset = new Vector3(0, 1.5f, 0);

        [SerializeField]
        [Tooltip("Should the tooltip float/bob?")]
        private bool enableBobbing = true;

        [SerializeField]
        private float bobSpeed = 2f;

        [SerializeField]
        private float bobHeight = 0.1f;

        [Header("Animation")]
        [SerializeField]
        private float fadeInSpeed = 5f;

        private CanvasGroup canvasGroup;
        private RectTransform rectTransform;
        private Vector3 basePosition;
        private float bobTimer = 0f;
        private Camera mainCamera;
        private Transform targetTransform;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            rectTransform = GetComponent<RectTransform>();
            mainCamera = Camera.main;

            // Start invisible
            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (targetTransform == null) return;

            // Update position to follow target
            UpdatePosition();

            // Bob animation
            if (enableBobbing)
            {
                bobTimer += Time.deltaTime * bobSpeed;
                float bobOffset = Mathf.Sin(bobTimer) * bobHeight;
                rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, 
                    rectTransform.anchoredPosition.y + bobOffset);
            }
        }

        public void Show(IInteractable interactable)
        {
            if (interactable == null) return;

            targetTransform = interactable.GetTooltipTransform();
            if (targetTransform == null) return;

            // Set text
            if (promptText != null)
            {
                promptText.text = interactable.GetInteractionPrompt();
            }

            if (keyText != null)
            {
                keyText.text = interactable.GetInteractionKey();
            }

            // Show and fade in
            gameObject.SetActive(true);
            UpdatePosition();
            StartCoroutine(FadeIn());
        }

        public void Hide()
        {
            StartCoroutine(FadeOut());
        }

        private void UpdatePosition()
        {
            if (targetTransform == null || mainCamera == null) return;

            // Convert world position to screen position
            Vector3 worldPosition = targetTransform.position + heightOffset;
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);

            // Check if on screen
            if (screenPosition.z > 0)
            {
                rectTransform.position = screenPosition;
            }
        }

        private System.Collections.IEnumerator FadeIn()
        {
            while (canvasGroup.alpha < 1f)
            {
                canvasGroup.alpha += Time.deltaTime * fadeInSpeed;
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        private System.Collections.IEnumerator FadeOut()
        {
            while (canvasGroup.alpha > 0f)
            {
                canvasGroup.alpha -= Time.deltaTime * fadeInSpeed;
                yield return null;
            }
            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
            targetTransform = null;
        }
    }
}