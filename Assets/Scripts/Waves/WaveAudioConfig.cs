using UnityEngine;

namespace Havengard.Waves
{
    /// <summary>
    /// Configuration for wave-related audio events
    /// </summary>
    [CreateAssetMenu(menuName = "Havengard/Waves/Wave Audio Config")]
    public class WaveAudioConfig : ScriptableObject
    {
        [Header("Wave Events")]
        [Tooltip("Sound played when waves start (first wave begins)")]
        public AudioClip wavesStartSound;

        [Tooltip("Sound played when a single wave starts")]
        public AudioClip waveStartSound;

        [Tooltip("Sound played when a wave is cleared")]
        public AudioClip waveClearedSound;

        [Tooltip("Sound played when all waves are complete")]
        public AudioClip allWavesCompleteSound;

        [Header("Victory")]
        [Tooltip("Sound played for level complete")]
        public AudioClip levelCompleteSound;

        [Header("Volume Settings")]
        [Range(0f, 1f)] public float waveEventVolume = 0.8f;
        [Range(0f, 1f)] public float victoryVolume = 1f;

        /// <summary>
        /// Play a sound from this config
        /// </summary>
        public void PlaySound(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;

            // Simple fallback: use AudioSource.PlayClipAtPoint for positional-free UI sounds
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, volume);
        }
    }
}