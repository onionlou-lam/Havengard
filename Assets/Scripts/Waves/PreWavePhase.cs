using UnityEngine;
using UnityEngine.Events;
using Havengard.Waves.UI; // Added this line

namespace Havengard.Waves
{
    /// <summary>
    /// Manages the pre-wave phase where players can prepare before the next wave
    /// </summary>
    public class PreWavePhase : MonoBehaviour
    {
        [Header("Phase Settings")]
        [SerializeField] private bool useTimeLimit = true;
        [Tooltip("Time limit in seconds for the pre-wave phase (0 = infinite)")]
        [SerializeField] private float timeLimitSeconds = 10f;
        
        [Header("UI References")]
        [SerializeField] private PreWavePhaseUI phaseUI;
        [SerializeField] private WavePreviewPanel wavePreviewPanel;

        [Header("Wave Preview")]
        [SerializeField] private bool showWavePreview = true;

        [Header("Events")]
        public UnityEvent OnPhaseStarted;
        public UnityEvent OnPhaseEnded;
        public UnityEvent<float> OnTimerUpdated; // Remaining time

        private bool isPhaseActive = false;
        private float remainingTime;
        private WaveDefinition currentWaveDefinition;

        public bool IsPhaseActive => isPhaseActive;
        public float RemainingTime => remainingTime;
        public bool UseTimeLimit => useTimeLimit;

        private void Awake()
        {
            // Auto-find UI if not assigned
            if (phaseUI == null)
            {
                phaseUI = GetComponentInChildren<PreWavePhaseUI>();
                if (phaseUI == null)
                {
                    phaseUI = FindFirstObjectByType<PreWavePhaseUI>();
                }
            }

            // Auto-find wave preview panel if not assigned
            if (wavePreviewPanel == null)
            {
                wavePreviewPanel = GetComponentInChildren<WavePreviewPanel>();
                if (wavePreviewPanel == null)
                {
                    wavePreviewPanel = FindFirstObjectByType<WavePreviewPanel>();
                }
            }

            Debug.Log($"[PreWavePhase] Initialized. UI found: {phaseUI != null}, Preview Panel found: {wavePreviewPanel != null}, UseTimeLimit: {useTimeLimit}, TimeLimitSeconds: {timeLimitSeconds}");
        }

        private void Update()
        {
            if (isPhaseActive && useTimeLimit && timeLimitSeconds > 0)
            {
                remainingTime -= Time.deltaTime;
                OnTimerUpdated?.Invoke(remainingTime);

                // Log every second for debugging
                if (Mathf.FloorToInt(remainingTime) != Mathf.FloorToInt(remainingTime + Time.deltaTime))
                {
                    Debug.Log($"[PreWavePhase] Timer update: {Mathf.CeilToInt(remainingTime)}s remaining");
                }

                if (remainingTime <= 0f)
                {
                    Debug.Log("[PreWavePhase] Time limit reached, auto-starting wave");
                    EndPhase();
                }
            }
        }

        /// <summary>
        /// Start the pre-wave phase
        /// </summary>
        public void StartPhase(int upcomingWaveNumber, WaveDefinition waveDefinition = null)
        {
            if (isPhaseActive)
            {
                Debug.LogWarning("[PreWavePhase] Phase already active!");
                return;
            }

            isPhaseActive = true;
            remainingTime = timeLimitSeconds;
            currentWaveDefinition = waveDefinition;
            
            if (phaseUI != null)
            {
                phaseUI.ShowPhase(upcomingWaveNumber, useTimeLimit, timeLimitSeconds);
            }

            // Show wave preview if enabled
            if (showWavePreview && wavePreviewPanel != null && waveDefinition != null)
            {
                // CHANGE THIS LINE:
                // var previewData = WavePreviewData.CreateFromDefinition(waveDefinition, upcomingWaveNumber);
                // TO:
                var previewData = WavePreviewData.FromWaveDefinition(waveDefinition, upcomingWaveNumber);
                wavePreviewPanel.ShowPreview(previewData, useTimeLimit, timeLimitSeconds);
            }

            // Debug logging
            Debug.Log($"[PreWavePhase] StartPhase called - About to invoke OnPhaseStarted event");
            Debug.Log($"[PreWavePhase] OnPhaseStarted listener count: {OnPhaseStarted?.GetPersistentEventCount() ?? 0}");
            
            // Invoke event
            OnPhaseStarted?.Invoke();
            
            Debug.Log($"[PreWavePhase] ✓ OnPhaseStarted event invoked for wave {upcomingWaveNumber}");
        }

        /// <summary>
        /// Overload for backward compatibility
        /// </summary>
        public void StartPhase(int upcomingWaveNumber)
        {
            StartPhase(upcomingWaveNumber, null);
        }

        /// <summary>
        /// End the pre-wave phase and start the wave
        /// </summary>
        public void EndPhase()
        {
            if (!isPhaseActive)
            {
                Debug.LogWarning("[PreWavePhase] Phase is not active!");
                return;
            }

            isPhaseActive = false;
            
            //Debug.Log("[PreWavePhase] Ending pre-wave phase, starting wave");
            
            // Call OnWaveStarted instead of HidePhase - keeps wave number visible
            if (phaseUI != null)
            {
                phaseUI.OnWaveStarted(); // ← CHANGED from HidePhase()
            }

            // Hide wave preview
            if (wavePreviewPanel != null)
            {
                wavePreviewPanel.HidePreview();
            }

            OnPhaseEnded?.Invoke();
        }

        /// <summary>
        /// Toggle time limit on/off during the phase
        /// </summary>
        public void ToggleTimeLimit(bool enabled)
        {
            useTimeLimit = enabled;
            
            //Debug.Log($"[PreWavePhase] Time limit toggled: {(enabled ? "ON" : "OFF")}");
            
            // Reset timer when enabling
            if (enabled && isPhaseActive)
            {
                remainingTime = timeLimitSeconds;
                Debug.Log($"[PreWavePhase] Timer reset to: {remainingTime}s");
            }
            
            // Update UI immediately
            if (isPhaseActive && phaseUI != null)
            {
                phaseUI.UpdateTimeLimitToggle(enabled);
            }
        }

        /// <summary>
        /// Set custom time limit
        /// </summary>
        public void SetTimeLimit(float seconds)
        {
            timeLimitSeconds = Mathf.Max(0f, seconds);
            
            if (isPhaseActive && useTimeLimit)
            {
                remainingTime = timeLimitSeconds;
            }
            
            //Debug.Log($"[PreWavePhase] Time limit set to: {timeLimitSeconds}s");
        }

        /// <summary>
        /// Player manually starts the wave
        /// </summary>
        public void ManuallyStartWave()
        {
            //Debug.Log("[PreWavePhase] Player manually started wave");
            EndPhase();
        }

        /// <summary>
        /// Toggle wave preview on/off
        /// </summary>
        public void SetShowWavePreview(bool show)
        {
            showWavePreview = show;
        }
    }
}