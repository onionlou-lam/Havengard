using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

namespace Havengard.Waves
{
    /// <summary>
    /// Controls global lighting intensity during wave phases (2D Lighting)
    /// </summary>
    public class WaveLightingController : MonoBehaviour
    {
        [Header("Light Settings")]
        [SerializeField] private Light2D globalLight;
        [SerializeField] private float defaultIntensity = 1.5f;
        [SerializeField] private float combatIntensity = 0.8f;
        [SerializeField] private float transitionDuration = 2f;

        [Header("Auto-Find Settings")]
        [SerializeField] private bool autoFindGlobalLight = true;

        private Coroutine currentTransition;
        private float targetIntensity;

        private void Awake()
        {
            // Auto-find the global light if not assigned
            if (globalLight == null && autoFindGlobalLight)
            {
                Light2D[] lights = FindObjectsByType<Light2D>(FindObjectsSortMode.None);
                foreach (Light2D light in lights)
                {
                    if (light.lightType == Light2D.LightType.Global)
                    {
                        globalLight = light;
                        Debug.Log($"[WaveLightingController] Auto-found global Light2D: {light.name}");
                        break;
                    }
                }
            }

            if (globalLight == null)
            {
                Debug.LogWarning("[WaveLightingController] No global Light2D assigned or found!");
            }
        }

        private void Start()
        {
            // Set default intensity on start
            SetIntensityImmediate(defaultIntensity);
        }

        /// <summary>
        /// Called when waves begin - dims light gradually
        /// </summary>
        public void OnWavesStarted()
        {
            TransitionToIntensity(combatIntensity);
        }

        /// <summary>
        /// Called when all waves complete - restores light gradually
        /// </summary>
        public void OnWavesCompleted()
        {
            TransitionToIntensity(defaultIntensity);
        }

        /// <summary>
        /// Smoothly transition light intensity
        /// </summary>
        private void TransitionToIntensity(float intensity)
        {
            if (globalLight == null) return;

            targetIntensity = intensity;

            if (currentTransition != null)
                StopCoroutine(currentTransition);

            currentTransition = StartCoroutine(TransitionRoutine(intensity));
        }

        private IEnumerator TransitionRoutine(float targetIntensity)
        {
            float startIntensity = globalLight.intensity;
            float elapsed = 0f;

            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / transitionDuration;
                globalLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, t);
                yield return null;
            }

            globalLight.intensity = targetIntensity;
            currentTransition = null;
        }

        /// <summary>
        /// Set intensity immediately without transition
        /// </summary>
        public void SetIntensityImmediate(float intensity)
        {
            if (globalLight != null)
                globalLight.intensity = intensity;
        }

        /// <summary>
        /// Manual control for custom transitions
        /// </summary>
        public void SetCustomIntensity(float intensity, float duration)
        {
            transitionDuration = duration;
            TransitionToIntensity(intensity);
        }
    }
}