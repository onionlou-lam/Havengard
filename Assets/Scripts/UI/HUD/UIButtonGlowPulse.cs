using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Havengard.UI
{
    /// <summary>
    /// Attach to a HUD icon button to make it pulse with a glow effect for a limited duration
    /// (e.g. to draw the player's attention to a newly available feature).
    /// Works by pulsing the color/alpha of the button's Image and, optionally, a separate
    /// "glow" Image placed behind/around the icon (e.g. a soft blurred sprite).
    /// </summary>
    public class UIButtonGlowPulse : MonoBehaviour
    {
        [Header("Targets")]
        [Tooltip("Main icon image whose color will pulse. Auto-found on this GameObject if not assigned.")]
        [SerializeField] private Image targetImage;

        [Tooltip("Optional separate glow image (e.g. a soft halo sprite behind the icon) whose alpha will pulse.")]
        [SerializeField] private Image glowImage;

        [Header("Pulse Settings")]
        [SerializeField] private Color glowColor = new Color(1f, 0.95f, 0.4f, 1f);
        [SerializeField] private float pulseSpeed = 3f;
        [SerializeField][Range(0f, 1f)] private float minGlowAlpha = 0.15f;
        [SerializeField][Range(0f, 1f)] private float maxGlowAlpha = 0.9f;

        [Header("Duration")]
        [Tooltip("How long the glow pulse lasts before automatically stopping.")]
        [SerializeField] private float defaultDuration = 8f;

        private Color originalImageColor;
        private Coroutine pulseRoutine;

        private void Awake()
        {
            if (targetImage == null)
                targetImage = GetComponent<Image>();

            if (targetImage != null)
                originalImageColor = targetImage.color;

            if (glowImage != null)
                glowImage.gameObject.SetActive(false);
        }

        /// <summary>
        /// Start pulsing using the default duration (see defaultDuration field).
        /// </summary>
        public void StartPulse()
        {
            StartPulse(defaultDuration);
        }

        /// <summary>
        /// Start pulsing for a custom duration in seconds. 0 or negative = pulse indefinitely until StopPulse() is called.
        /// </summary>
        public void StartPulse(float duration)
        {
            StopPulse();
            pulseRoutine = StartCoroutine(PulseRoutine(duration));
        }

        /// <summary>
        /// Immediately stop pulsing and restore the original visual state.
        /// </summary>
        public void StopPulse()
        {
            if (pulseRoutine != null)
            {
                StopCoroutine(pulseRoutine);
                pulseRoutine = null;
            }

            if (targetImage != null)
                targetImage.color = originalImageColor;

            if (glowImage != null)
                glowImage.gameObject.SetActive(false);
        }

        private IEnumerator PulseRoutine(float duration)
        {
            if (glowImage != null)
                glowImage.gameObject.SetActive(true);

            float elapsed = 0f;
            bool infinite = duration <= 0f;

            while (infinite || elapsed < duration)
            {
                elapsed += Time.deltaTime;

                float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f; // 0..1

                if (targetImage != null)
                    targetImage.color = Color.Lerp(originalImageColor, glowColor, t);

                if (glowImage != null)
                {
                    var c = glowImage.color;
                    c.a = Mathf.Lerp(minGlowAlpha, maxGlowAlpha, t);
                    glowImage.color = c;
                }

                yield return null;
            }

            if (targetImage != null)
                targetImage.color = originalImageColor;

            if (glowImage != null)
                glowImage.gameObject.SetActive(false);

            pulseRoutine = null;
        }
    }
}
