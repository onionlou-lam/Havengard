using UnityEngine;
using UnityEngine.Events;

namespace Havengard.Waves
{
    /// <summary>
    /// Centralized events for wave system
    /// </summary>
    [System.Serializable]
    public class WaveEvents
    {
        [Header("Wave Lifecycle")]
        public UnityEvent OnWavesStarted;           // First wave begins
        public UnityEvent<int> OnWaveStarted;       // Individual wave starts (wave index)
        public UnityEvent<int> OnWaveCleared;       // Wave cleared (wave index)
        public UnityEvent OnAllWavesComplete;       // All waves finished

        [Header("Level Complete")]
        public UnityEvent OnLevelComplete;          // Level victory
    }
}