using UnityEngine;
using UnityEngine.Events;

namespace Havengard.Waves
{
    /// <summary>
    /// Manages the pre-wave phase where players can prepare before the next wave
    /// </summary>
    public class PreWavePhase : MonoBehaviour
    {
        [Header("Phase Settings")]
        [SerializeField] private bool useTimeLimit = false;
        [Tooltip("Time limit in seconds for the pre-wave phase (0 = infinite)")]
        [SerializeField] private float timeLimitSeconds = 30f;
        
        [Header("UI References")]
        [SerializeField] private PreWavePhaseUI phaseUI;

        [Header("Events")]
        public UnityEvent OnPhaseStarted;
        public UnityEvent OnPhaseEnded;
        public UnityEvent<float> OnTimerUpdated; // Remaining time

        private bool isPhaseActive = false;
        private float remainingTime;
        private bool autoStartEnabled = false;

        public bool IsPhaseActive => isPhaseActive;
        public float RemainingTime => remainingTime;
        public bool UseTimeLimit => useTimeLimit;

        private void Update()
        {
            if (isPhaseActive && useTimeLimit && timeLimitSeconds > 0)
            {
                remainingTime -= Time.deltaTime;
                OnTimerUpdated?.Invoke(remainingTime);

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
        public void StartPhase(int upcomingWaveNumber)
        {
            if (isPhaseActive)
            {
                Debug.LogWarning("[PreWavePhase] Phase already active!");
                return;
            }

            isPhaseActive = true;
            remainingTime = timeLimitSeconds;

            Debug.Log($"[PreWavePhase] Starting pre-wave phase for Wave {upcomingWaveNumber}");
            
            if (phaseUI != null)
            {
                phaseUI.ShowPhase(upcomingWaveNumber, useTimeLimit, timeLimitSeconds);
            }

            OnPhaseStarted?.Invoke();
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
            
            Debug.Log("[PreWavePhase] Ending pre-wave phase, starting wave");
            
            if (phaseUI != null)
            {
                phaseUI.HidePhase();
            }

            OnPhaseEnded?.Invoke();
        }

        /// <summary>
        /// Toggle time limit on/off
        /// </summary>
        public void ToggleTimeLimit(bool enabled)
        {
            useTimeLimit = enabled;
            Debug.Log($"[PreWavePhase] Time limit {(enabled ? "enabled" : "disabled")}");
            
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
            timeLimitSeconds = seconds;
            if (isPhaseActive)
            {
                remainingTime = seconds;
            }
        }

        /// <summary>
        /// Player manually starts the wave
        /// </summary>
        public void ManuallyStartWave()
        {
            Debug.Log("[PreWavePhase] Player manually started wave");
            EndPhase();
        }
    }
}