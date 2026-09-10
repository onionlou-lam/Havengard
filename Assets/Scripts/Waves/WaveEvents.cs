using UnityEngine;
using UnityEngine.Events;

namespace Havengard.Waves
{
    /// <summary>
    /// Container for wave-related events
    /// </summary>
    [System.Serializable]
    public class WaveEvents
    {
        public UnityEvent OnWavesStarted = new UnityEvent();
        public UnityEvent<int> OnWaveStarted = new UnityEvent<int>();
        public UnityEvent<int> OnWaveCleared = new UnityEvent<int>();
        public UnityEvent OnAllWavesComplete = new UnityEvent();

        /// <summary>
        /// Raised whenever the count of enemies remaining alive in the current wave changes
        /// (enemy spawned or enemy died). Parameters: (remainingCount, totalToSpawn).
        /// </summary>
        public UnityEvent<int, int> OnEnemyCountChanged = new UnityEvent<int, int>();
    }
}