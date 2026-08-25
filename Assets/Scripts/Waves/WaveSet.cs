using UnityEngine;

namespace Havengard.Waves
{
    [CreateAssetMenu(menuName = "Havengard/Waves/Wave Set")]
    public class WaveSet : ScriptableObject
    {
        [Header("Wave Set")]
        public string setName = "New Wave Set";
        public WaveDefinition[] waves;

        [Header("Difficulty Scaling")]
        [Tooltip("Multiplier applied to enemy health (1.0 = normal)")]
        [Range(0.1f, 10f)]
        public float healthMultiplier = 1f;

        [Tooltip("Multiplier applied to enemy damage (1.0 = normal)")]
        [Range(0.1f, 10f)]
        public float damageMultiplier = 1f;

        [Tooltip("Multiplier applied to enemy speed (1.0 = normal)")]
        [Range(0.1f, 3f)]
        public float speedMultiplier = 1f;

        [Tooltip("Multiplier applied to reward drops (1.0 = normal)")]
        [Range(0.1f, 5f)]
        public float rewardMultiplier = 1f;

        [Header("Progressive Scaling")]
        [Tooltip("If true, difficulty increases with each wave")]
        public bool useProgressiveScaling = false;

        [Tooltip("Multiplier increase per wave (e.g., 0.1 = +10% per wave)")]
        [Range(0f, 0.5f)]
        public float scalingPerWave = 0.1f;

        /// <summary>
        /// Get the effective health multiplier for a specific wave index
        /// </summary>
        public float GetHealthMultiplier(int waveIndex)
        {
            if (!useProgressiveScaling) return healthMultiplier;
            return healthMultiplier * (1f + (scalingPerWave * waveIndex));
        }

        /// <summary>
        /// Get the effective damage multiplier for a specific wave index
        /// </summary>
        public float GetDamageMultiplier(int waveIndex)
        {
            if (!useProgressiveScaling) return damageMultiplier;
            return damageMultiplier * (1f + (scalingPerWave * waveIndex));
        }

        /// <summary>
        /// Get the effective speed multiplier for a specific wave index
        /// </summary>
        public float GetSpeedMultiplier(int waveIndex)
        {
            if (!useProgressiveScaling) return speedMultiplier;
            return speedMultiplier * (1f + (scalingPerWave * 0.5f * waveIndex)); // Speed scales slower
        }

        /// <summary>
        /// Get the effective reward multiplier for a specific wave index
        /// </summary>
        public float GetRewardMultiplier(int waveIndex)
        {
            if (!useProgressiveScaling) return rewardMultiplier;
            return rewardMultiplier * (1f + (scalingPerWave * waveIndex));
        }
    }
}
