using UnityEngine;

namespace Havengard.Audio
{
    /// <summary>
    /// Manages UI sound effects (button clicks, hovers, etc.)
    /// </summary>
    public class UIAudioManager : MonoBehaviour
    {
        public static UIAudioManager Instance { get; private set; }

        [Header("UI Sound Effects")]
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip buttonHoverSound;
        [SerializeField] private AudioClip panelOpenSound;
        [SerializeField] private AudioClip panelCloseSound;
        [SerializeField] private AudioClip errorSound;
        [SerializeField] private AudioClip successSound;

        [Header("Settings")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField][Range(0f, 1f)] private float uiVolume = 0.7f;

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Create audio source if not assigned
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.volume = uiVolume;
            }
        }

        /// <summary>
        /// Play button click sound
        /// </summary>
        public void PlayButtonClick()
        {
            PlaySound(buttonClickSound);
        }

        /// <summary>
        /// Play button hover sound
        /// </summary>
        public void PlayButtonHover()
        {
            PlaySound(buttonHoverSound);
        }

        /// <summary>
        /// Play panel open sound
        /// </summary>
        public void PlayPanelOpen()
        {
            PlaySound(panelOpenSound);
        }

        /// <summary>
        /// Play panel close sound
        /// </summary>
        public void PlayPanelClose()
        {
            PlaySound(panelCloseSound);
        }

        /// <summary>
        /// Play error sound
        /// </summary>
        public void PlayError()
        {
            PlaySound(errorSound);
        }

        /// <summary>
        /// Play success sound
        /// </summary>
        public void PlaySuccess()
        {
            PlaySound(successSound);
        }

        /// <summary>
        /// Play a specific sound
        /// </summary>
        private void PlaySound(AudioClip clip)
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip, uiVolume);
            }
        }

        /// <summary>
        /// Set UI volume
        /// </summary>
        public void SetVolume(float volume)
        {
            uiVolume = Mathf.Clamp01(volume);
            if (audioSource != null)
                audioSource.volume = uiVolume;
        }
    }
}