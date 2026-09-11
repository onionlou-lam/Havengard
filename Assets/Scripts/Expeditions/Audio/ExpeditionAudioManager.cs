using UnityEngine;

namespace Havengard.Expeditions.Audio
{
    /// <summary>
    /// Plays audio feedback for expedition lifecycle events (started, success, failed).
    /// Subscribes to ExpeditionManager events so no other script needs to know about audio.
    /// </summary>
    public class ExpeditionAudioManager : MonoBehaviour
    {
        public static ExpeditionAudioManager Instance { get; private set; }

        [Header("Expedition Sound Effects")]
        [Tooltip("Played when an expedition is successfully started.")]
        [SerializeField] private AudioClip expeditionStartedSound;

        [Tooltip("Played when an expedition completes successfully.")]
        [SerializeField] private AudioClip expeditionSuccessSound;

        [Tooltip("Played when an expedition fails.")]
        [SerializeField] private AudioClip expeditionFailedSound;

        [Header("Settings")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] [Range(0f, 1f)] private float volume = 0.8f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }

        private void Start()
        {
            SubscribeToExpeditionManager();
        }

        private void OnDestroy()
        {
            UnsubscribeFromExpeditionManager();
        }

        private void SubscribeToExpeditionManager()
        {
            if (ExpeditionManager.Instance == null)
            {
                Debug.LogWarning("[ExpeditionAudioManager] ExpeditionManager.Instance not found. Audio feedback disabled.");
                return;
            }

            ExpeditionManager.Instance.OnExpeditionStarted += HandleExpeditionStarted;
            ExpeditionManager.Instance.OnExpeditionCompleted += HandleExpeditionCompleted;
        }

        private void UnsubscribeFromExpeditionManager()
        {
            if (ExpeditionManager.Instance == null) return;

            ExpeditionManager.Instance.OnExpeditionStarted -= HandleExpeditionStarted;
            ExpeditionManager.Instance.OnExpeditionCompleted -= HandleExpeditionCompleted;
        }

        private void HandleExpeditionStarted(ExpeditionInstance instance)
        {
            PlaySound(expeditionStartedSound);
        }

        private void HandleExpeditionCompleted(ExpeditionInstance instance, ExpeditionResult result)
        {
            PlaySound(result.success ? expeditionSuccessSound : expeditionFailedSound);
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip, volume);
            }
        }
    }
}
