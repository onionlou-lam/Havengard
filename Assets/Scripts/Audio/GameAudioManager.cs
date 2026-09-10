using System.Collections.Generic;
using UnityEngine;

namespace Havengard.Audio
{
    /// <summary>
    /// Centralized gameplay SFX manager (distinct from UIAudioManager, which handles menu/UI chrome,
    /// and BuildingAudioManager, which handles building-mode domain events).
    /// 
    /// Solves two problems with ad-hoc AudioSource.PlayClipAtPoint / per-object AudioSource calls:
    /// 1. Sound spam: too many simultaneous instances of the same clip (e.g. many enemies attacking
    ///    at once) stacking volume and becoming overwhelming.
    /// 2. Pause bugs: looping SFX (zone effects, auras, etc.) keep playing in real time even when
    ///    Time.timeScale is set to 0, since Unity audio does not respect timeScale automatically.
    /// 
    /// Usage:
    /// - One-shot SFX: GameAudioManager.Instance.PlaySFX(clip, position, volume);
    /// - Looping SFX tied to a gameplay object: 
    ///       Handle handle = GameAudioManager.Instance.PlayLoop(clip, transform, volume);
    ///       ... later: GameAudioManager.Instance.StopLoop(handle);
    /// </summary>
    public class GameAudioManager : MonoBehaviour
    {
        public static GameAudioManager Instance { get; private set; }

        [Header("One-Shot SFX Limiting")]
        [Tooltip("Maximum number of simultaneous instances allowed per unique AudioClip.")]
        [SerializeField] private int maxConcurrentPerClip = 4;

        [Tooltip("Minimum time (seconds, unscaled) between plays of the same clip. Helps smooth out bursts (e.g. many enemies attacking in the same frame).")]
        [SerializeField] private float minRetriggerInterval = 0.05f;

        [Header("Pooling")]
        [SerializeField] private int initialPoolSize = 16;
        [SerializeField] private Transform poolParent;

        [Header("Volume")]
        [SerializeField][Range(0f, 1f)] private float masterSFXVolume = 1f;

        // One-shot pooling
        private readonly List<PooledSource> pool = new List<PooledSource>();
        private readonly Dictionary<AudioClip, int> activeCountPerClip = new Dictionary<AudioClip, int>();
        private readonly Dictionary<AudioClip, float> lastPlayTimePerClip = new Dictionary<AudioClip, float>();

        // Looping sources tied to gameplay objects (e.g. ZoneEffect auras)
        private readonly Dictionary<int, LoopingSource> activeLoops = new Dictionary<int, LoopingSource>();
        private int nextLoopHandleId = 1;

        private bool isPaused = false;

        private class PooledSource
        {
            public AudioSource source;
            public AudioClip trackedClip;
            public bool inUse;
        }

        private class LoopingSource
        {
            public AudioSource source;
            public bool wasPlayingBeforePause;
        }

        /// <summary>
        /// Opaque handle returned by PlayLoop, used to stop/track a specific looping instance.
        /// </summary>
        public struct LoopHandle
        {
            public int Id;
            public static readonly LoopHandle Invalid = new LoopHandle { Id = 0 };
            public bool IsValid => Id != 0;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (poolParent == null)
            {
                var poolObj = new GameObject("SFXPool");
                poolObj.transform.SetParent(transform);
                poolParent = poolObj.transform;
            }

            for (int i = 0; i < initialPoolSize; i++)
            {
                CreatePooledSource();
            }
        }

        private PooledSource CreatePooledSource()
        {
            var obj = new GameObject("PooledAudioSource");
            obj.transform.SetParent(poolParent);

            var source = obj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 1f; // 3D by default; callers can override via PlaySFX2D if needed

            var pooled = new PooledSource { source = source, inUse = false };
            pool.Add(pooled);
            return pooled;
        }

        private void Update()
        {
            // Reclaim pooled sources once they've finished playing
            foreach (var pooled in pool)
            {
                if (pooled.inUse && !pooled.source.isPlaying)
                {
                    ReleasePooledSource(pooled);
                }
            }
        }

        private void ReleasePooledSource(PooledSource pooled)
        {
            if (pooled.trackedClip != null && activeCountPerClip.TryGetValue(pooled.trackedClip, out int count))
            {
                activeCountPerClip[pooled.trackedClip] = Mathf.Max(0, count - 1);
            }

            pooled.inUse = false;
            pooled.trackedClip = null;
        }

        #region One-Shot SFX

        /// <summary>
        /// Plays a one-shot SFX at a world position, subject to per-clip concurrency limiting.
        /// Returns false if the request was suppressed (clip already at max concurrent instances,
        /// or retriggered too soon).
        /// </summary>
        public bool PlaySFX(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
        {
            if (clip == null)
                return false;

            if (IsPaused)
                return false;

            if (!CanPlayClip(clip))
                return false;

            var pooled = GetAvailableSource();
            if (pooled == null)
                return false;

            pooled.inUse = true;
            pooled.trackedClip = clip;
            pooled.source.transform.position = position;
            pooled.source.clip = clip;
            pooled.source.volume = volume * masterSFXVolume;
            pooled.source.pitch = pitch;
            pooled.source.loop = false;
            pooled.source.Play();

            RegisterPlay(clip);
            return true;
        }

        /// <summary>
        /// Plays a one-shot SFX with randomized pitch (common for footsteps, attacks, impacts).
        /// </summary>
        public bool PlaySFXRandomPitch(AudioClip clip, Vector3 position, float volume = 1f, float minPitch = 0.9f, float maxPitch = 1.1f)
        {
            float pitch = Random.Range(minPitch, maxPitch);
            return PlaySFX(clip, position, volume, pitch);
        }

        private bool CanPlayClip(AudioClip clip)
        {
            // Concurrency cap
            if (activeCountPerClip.TryGetValue(clip, out int count) && count >= maxConcurrentPerClip)
                return false;

            // Retrigger throttle (smooths out same-frame bursts, e.g. 10 enemies attacking at once)
            if (lastPlayTimePerClip.TryGetValue(clip, out float lastTime))
            {
                if (Time.unscaledTime - lastTime < minRetriggerInterval)
                    return false;
            }

            return true;
        }

        private void RegisterPlay(AudioClip clip)
        {
            activeCountPerClip.TryGetValue(clip, out int count);
            activeCountPerClip[clip] = count + 1;
            lastPlayTimePerClip[clip] = Time.unscaledTime;
        }

        private PooledSource GetAvailableSource()
        {
            foreach (var pooled in pool)
            {
                if (!pooled.inUse)
                    return pooled;
            }

            // Pool exhausted - grow it rather than dropping sound entirely
            return CreatePooledSource();
        }

        #endregion

        #region Looping SFX (pause-aware)

        /// <summary>
        /// Starts a looping SFX attached to a transform (e.g. a zone effect aura).
        /// The returned handle must be passed to StopLoop when finished.
        /// Looping sources are automatically paused/resumed with the game's pause state.
        /// </summary>
        public LoopHandle PlayLoop(AudioClip clip, Transform attachTo, float volume = 1f, float spatialBlend = 0.5f)
        {
            if (clip == null)
                return LoopHandle.Invalid;

            var obj = new GameObject($"LoopSFX_{clip.name}");
            if (attachTo != null)
                obj.transform.SetParent(attachTo, false);

            var source = obj.AddComponent<AudioSource>();
            source.clip = clip;
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = spatialBlend;
            source.volume = volume * masterSFXVolume;

            if (!isPaused)
                source.Play();

            int id = nextLoopHandleId++;
            activeLoops[id] = new LoopingSource { source = source, wasPlayingBeforePause = !isPaused };

            return new LoopHandle { Id = id };
        }

        /// <summary>
        /// Stops and cleans up a looping SFX started with PlayLoop.
        /// </summary>
        public void StopLoop(LoopHandle handle)
        {
            if (!handle.IsValid)
                return;

            if (activeLoops.TryGetValue(handle.Id, out var loop))
            {
                if (loop.source != null)
                {
                    loop.source.Stop();
                    Destroy(loop.source.gameObject);
                }

                activeLoops.Remove(handle.Id);
            }
        }

        #endregion

        #region Pause Handling

        public bool IsPaused => isPaused;

        /// <summary>
        /// Call this from your pause system (e.g. PauseMenuUI.Pause()) to pause all managed audio,
        /// including looping SFX that would otherwise ignore Time.timeScale.
        /// </summary>
        public void PauseAudio()
        {
            if (isPaused)
                return;

            isPaused = true;

            // Pause all in-flight one-shot sources
            foreach (var pooled in pool)
            {
                if (pooled.inUse && pooled.source.isPlaying)
                    pooled.source.Pause();
            }

            // Pause all looping sources
            foreach (var kvp in activeLoops)
            {
                var loop = kvp.Value;
                if (loop.source != null)
                {
                    loop.wasPlayingBeforePause = loop.source.isPlaying;
                    if (loop.wasPlayingBeforePause)
                        loop.source.Pause();
                }
            }
        }

        /// <summary>
        /// Call this from your pause system (e.g. PauseMenuUI.Resume()) to resume all managed audio.
        /// </summary>
        public void ResumeAudio()
        {
            if (!isPaused)
                return;

            isPaused = false;

            foreach (var pooled in pool)
            {
                if (pooled.inUse)
                    pooled.source.UnPause();
            }

            foreach (var kvp in activeLoops)
            {
                var loop = kvp.Value;
                if (loop.source != null && loop.wasPlayingBeforePause)
                    loop.source.UnPause();
            }
        }

        #endregion

        public void SetMasterVolume(float volume)
        {
            masterSFXVolume = Mathf.Clamp01(volume);
        }
    }
}