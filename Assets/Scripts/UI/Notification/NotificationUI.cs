using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Havengard.UI.Notifications
{
    /// <summary>
    /// Individual notification popup with animation
    /// </summary>
    public class NotificationUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI messageText;

        [Header("Animation Settings")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        [SerializeField] private float moveUpSpeed = 50f;
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private RectTransform rectTransform;
        private float lifetime;
        private bool isAnimating;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            // Start invisible
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;
        }

        /// <summary>
        /// Initialize and show the notification
        /// </summary>
        public void Show(NotificationData data)
        {
            if (messageText != null)
                messageText.text = data.message;

            if (backgroundImage != null)
                backgroundImage.color = data.backgroundColor;

            if (messageText != null)
                messageText.color = data.textColor;

            if (iconImage != null)
            {
                if (data.icon != null)
                {
                    iconImage.sprite = data.icon;
                    iconImage.gameObject.SetActive(true);
                }
                else
                {
                    iconImage.gameObject.SetActive(false);
                }
            }

            lifetime = data.duration;

            StartCoroutine(AnimateNotification());
        }

        /// <summary>
        /// Main animation coroutine: fade in, wait, fade out
        /// </summary>
        private IEnumerator AnimateNotification()
        {
            isAnimating = true;

            // Phase 1: Fade In
            yield return StartCoroutine(FadeIn());

            // Phase 2: Display (move up slowly)
            float displayTimer = 0f;
            while (displayTimer < lifetime)
            {
                displayTimer += Time.unscaledDeltaTime;

                // Move upward
                rectTransform.anchoredPosition += Vector2.up * moveUpSpeed * Time.unscaledDeltaTime;

                yield return null;
            }

            // Phase 3: Fade Out
            yield return StartCoroutine(FadeOut());

            // Cleanup
            isAnimating = false;
            Destroy(gameObject);
        }

        /// <summary>
        /// Fade in animation
        /// </summary>
        private IEnumerator FadeIn()
        {
            float elapsed = 0f;

            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / fadeInDuration;
                float curveValue = fadeCurve.Evaluate(t);

                if (canvasGroup != null)
                    canvasGroup.alpha = curveValue;

                // Move upward during fade in too
                rectTransform.anchoredPosition += Vector2.up * moveUpSpeed * Time.unscaledDeltaTime;

                yield return null;
            }

            if (canvasGroup != null)
                canvasGroup.alpha = 1f;
        }

        /// <summary>
        /// Fade out animation
        /// </summary>
        private IEnumerator FadeOut()
        {
            float elapsed = 0f;
            float startAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;

            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / fadeOutDuration;
                float curveValue = fadeCurve.Evaluate(1f - t); // Reverse curve

                if (canvasGroup != null)
                    canvasGroup.alpha = startAlpha * curveValue;

                // Continue moving upward during fade out
                rectTransform.anchoredPosition += Vector2.up * moveUpSpeed * Time.unscaledDeltaTime;

                yield return null;
            }

            if (canvasGroup != null)
                canvasGroup.alpha = 0f;
        }
    }
}