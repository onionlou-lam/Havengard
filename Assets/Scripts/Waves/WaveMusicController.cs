using System.Collections;
using UnityEngine;

namespace Havengard.Waves
{
    /// <summary>
    /// Controls background music loops for the wave gameplay loop:
    /// a "prep" loop during the PreWavePhase and a "combat" loop while a wave is active.
    /// Crossfades between the two tracks using two AudioSources.
    /// </summary>
    public class WaveMusicController : MonoBehaviour
    {
        public static WaveMusicController Instance { get; private set; }

        [Header("References")]
        [SerializeField] private WaveManager waveManager;
        [SerializeField] private PreWavePhase preWavePhase;

        [Header("Music Clips")]
        [Tooltip("Looping music played while preparing for the next wave (Pre-Wave Phase).")]
        [SerializeField] private AudioClip preWaveMusicLoop;

        [Tooltip("Looping music played while a wave is actively in combat.")]
        [SerializeField] private AudioClip activeWaveMusicLoop;

        [Header("Settings")]
        [SerializeField][Range(0f, 1f)] private float musicVolume = 0.6f;
        [SerializeField] private float crossfadeDuration = 1.5f;
        [SerializeField] private bool playOnAwake = true;

        private AudioSource sourceA;
        private AudioSource sourceB;
        private AudioSource activeSource;
        private AudioSource inactiveSource;
        private Coroutine crossfadeRoutine;

        private void Awake()
        {
            // Singleton setup
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Auto-find references if not assigned
            if (waveManager == null)
                waveManager = FindFirstObjectByType<WaveManager>();

            if (preWavePhase == null)
                preWavePhase = FindFirstObjectByType<PreWavePhase>();

            // Create two audio sources for crossfading
            sourceA = gameObject.AddComponent<AudioSource>();
            sourceB = gameObject.AddComponent<AudioSource>();

            foreach (var source in new[] { sourceA, sourceB })
            {
                source.loop = true;
                source.playOnAwake = false;
                source.volume = 0f;
            }

            activeSource = sourceA;
            inactiveSource = sourceB;
        }

        private void Start()
        {
            SubscribeToEvents();

            if (playOnAwake && preWaveMusicLoop != null)
            {
                PlayImmediate(preWaveMusicLoop);
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            if (preWavePhase != null)
            {
                preWavePhase.OnPhaseStarted.RemoveListener(OnPreWavePhaseStarted);
                preWavePhase.OnPhaseStarted.AddListener(OnPreWavePhaseStarted);
            }

            if (waveManager != null)
            {
                waveManager.waveEvents.OnWaveStarted.RemoveListener(OnWaveStarted);
                waveManager.waveEvents.OnWaveStarted.AddListener(OnWaveStarted);

                waveManager.waveEvents.OnAllWavesComplete.RemoveListener(OnAllWavesComplete);
                waveManager.waveEvents.OnAllWavesComplete.AddListener(OnAllWavesComplete);
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (preWavePhase != null)
            {
                preWavePhase.OnPhaseStarted.RemoveListener(OnPreWavePhaseStarted);
            }

            if (waveManager != null)
            {
                waveManager.waveEvents.OnWaveStarted.RemoveListener(OnWaveStarted);
                waveManager.waveEvents.OnAllWavesComplete.RemoveListener(OnAllWavesComplete);
            }
        }

        /// <summary>
        /// Called when the pre-wave (prep) phase starts.
        /// </summary>
        private void OnPreWavePhaseStarted()
        {
            Debug.Log("[WaveMusicController] Pre-wave phase started - switching to prep music");
            CrossfadeTo(preWaveMusicLoop);
        }

        /// <summary>
        /// Called when an individual wave starts (combat begins).
        /// </summary>
        private void OnWaveStarted(int waveIndex)
        {
            Debug.Log("[WaveMusicController] Wave started - switching to combat music");
            CrossfadeTo(activeWaveMusicLoop);
        }

        /// <summary>
        /// Called when all waves are complete - stop combat music, return to prep loop (or silence).
        /// </summary>
        private void OnAllWavesComplete()
        {
            Debug.Log("[WaveMusicController] All waves complete - fading music out");
            CrossfadeTo(preWaveMusicLoop);
        }

        /// <summary>
        /// Immediately plays a clip on the active source without fading (used for initial start).
        /// </summary>
        private void PlayImmediate(AudioClip clip)
        {
            if (clip == null) return;

            activeSource.clip = clip;
            activeSource.volume = musicVolume;
            activeSource.Play();
        }

        /// <summary>
        /// Crossfades from the current track to the given clip. If already playing this clip, does nothing.
        /// </summary>
        private void CrossfadeTo(AudioClip clip)
        {
            if (clip == null)
                return;

            if (activeSource.clip == clip && activeSource.isPlaying)
                return;

            if (crossfadeRoutine != null)
                StopCoroutine(crossfadeRoutine);

            crossfadeRoutine = StartCoroutine(CrossfadeRoutine(clip));
        }

        private IEnumerator CrossfadeRoutine(AudioClip newClip)
        {
            inactiveSource.clip = newClip;
            inactiveSource.volume = 0f;
            inactiveSource.Play();

            float elapsed = 0f;
            float startActiveVolume = activeSource.volume;

            while (elapsed < crossfadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = crossfadeDuration > 0f ? elapsed / crossfadeDuration : 1f;

                inactiveSource.volume = Mathf.Lerp(0f, musicVolume, t);
                activeSource.volume = Mathf.Lerp(startActiveVolume, 0f, t);

                yield return null;
            }

            inactiveSource.volume = musicVolume;
            activeSource.volume = 0f;
            activeSource.Stop();

            // Swap active/inactive
            var temp = activeSource;
            activeSource = inactiveSource;
            inactiveSource = temp;

            crossfadeRoutine = null;
        }

        /// <summary>
        /// Set the master music volume (applied to the currently active source).
        /// </summary>
        public void SetVolume(float newVolume)
        {
            musicVolume = Mathf.Clamp01(newVolume);
            if (activeSource != null && activeSource.isPlaying)
                activeSource.volume = musicVolume;
        }

        /// <summary>
        /// Stop all music immediately.
        /// </summary>
        public void StopMusic()
        {
            if (crossfadeRoutine != null)
            {
                StopCoroutine(crossfadeRoutine);
                crossfadeRoutine = null;
            }

            sourceA.Stop();
            sourceB.Stop();
        }
    }
}