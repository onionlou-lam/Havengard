using System.Collections;
using UnityEngine;

namespace Havengard.Dialogue
{
    /// <summary>Small damped up/down bounce of the speaker portrait each time a line starts.</summary>
    public class DialoguePortraitBob : MonoBehaviour
    {
        [SerializeField] private RectTransform portrait;

        [Header("Bob")]
        [Tooltip("Peak offset in canvas units.")]
        [SerializeField] private float height = 6f;
        [SerializeField] private float duration = 0.35f;
        [SerializeField] private int bounces = 2;

        private Vector2 basePosition;
        private Coroutine routine;

        private void Awake()
        {
            basePosition = portrait.anchoredPosition;
        }

        private void OnDisable()
        {
            StopBob();
        }

        /// <summary>Called by the dialogue presenter whenever a new line starts.</summary>
        public void NotifyLineStarted()
        {
            if (!isActiveAndEnabled) return;

            StopBob();
            routine = StartCoroutine(Bob());
        }

        private void StopBob()
        {
            if (routine != null) { StopCoroutine(routine); routine = null; }
            portrait.anchoredPosition = basePosition;
        }

        private IEnumerator Bob()
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);

                // Damped sine: moves up and down, then settles back on the base position.
                float offset = Mathf.Sin(progress * Mathf.PI * 2f * bounces) * (1f - progress) * height;
                portrait.anchoredPosition = basePosition + new Vector2(0f, offset);
                yield return null;
            }

            portrait.anchoredPosition = basePosition;
            routine = null;
        }
    }
}