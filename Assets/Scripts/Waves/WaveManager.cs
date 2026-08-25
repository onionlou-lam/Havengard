using UnityEngine;
using System.Collections;

namespace Havengard.Waves
{
    [DisallowMultipleComponent]
    public class WaveManager : MonoBehaviour
    {
        [Header("Wave Data")]
        [SerializeField] private WaveSet waveSet;

        [Header("Spawn Zones")]
        [SerializeField] private Transform[] spawnZones;

        [Header("Behaviour")]
        [SerializeField] private bool autoStartOnEnable = true;
        [SerializeField] private bool usePreWavePhase = true;

        [Header("References")]
        [SerializeField] private WaveSpawner spawner;
        [SerializeField] private PreWavePhase preWavePhase;

        [Tooltip("Optional receiver for Gold/EXP/Celestium wave rewards.")]
        [SerializeField] private MonoBehaviour rewardReceiverBehaviour;

        [Header("UI")]
        [SerializeField] private Havengard.UI.WaveHUDButtons hudButtons;

        [Header("Audio")]
        [SerializeField] private WaveAudioConfig audioConfig;
        [SerializeField] private bool playWaveSounds = true;

        [Header("Lighting")]
        [SerializeField] private WaveLightingController lightingController;
        [SerializeField] private bool useDynamicLighting = true;

        [Header("Difficulty Scaling")]
        [SerializeField] private bool applyDifficultyScaling = true;

        [Header("Events")]
        [SerializeField] private WaveEvents waveEvents;

        private IWaveRewardReceiver rewardReceiver;

        private int currentWaveIndex = -1;
        private WaveRuntimeTracker currentTracker;
        private Coroutine runRoutine;
        private bool waitingForPreWavePhaseToEnd = false;
        private bool firstWaveStarted = false;

        public bool IsRunning => runRoutine != null;
        public int CurrentWaveIndex => currentWaveIndex;
        public int TotalWaves => waveSet != null && waveSet.waves != null ? waveSet.waves.Length : 0;

        private void Awake()
        {
            if (spawner == null) spawner = GetComponent<WaveSpawner>();
            if (spawner == null) spawner = gameObject.AddComponent<WaveSpawner>();

            spawner.SetZones(spawnZones);

            rewardReceiver = rewardReceiverBehaviour as IWaveRewardReceiver;

            if (hudButtons == null)
                hudButtons = FindFirstObjectByType<Havengard.UI.WaveHUDButtons>();

            // Auto-find PreWavePhase if not assigned
            if (preWavePhase == null)
                preWavePhase = FindFirstObjectByType<PreWavePhase>();

            // Auto-find LightingController if not assigned
            if (lightingController == null)
                lightingController = FindFirstObjectByType<WaveLightingController>();

            // Subscribe to PreWavePhase events
            if (preWavePhase != null)
            {
                preWavePhase.OnPhaseEnded.AddListener(OnPreWavePhaseEnded);
                Debug.Log("[WaveManager] Subscribed to PreWavePhase events");
            }

            // Initialize events if null
            if (waveEvents == null)
                waveEvents = new WaveEvents();
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (preWavePhase != null)
            {
                preWavePhase.OnPhaseEnded.RemoveListener(OnPreWavePhaseEnded);
            }
        }

        private void OnEnable()
        {
            if (autoStartOnEnable)
                StartNight();
        }

        public void StartNight()
        {
            if (waveSet == null || waveSet.waves == null || waveSet.waves.Length == 0)
            {
                Debug.LogWarning("[WaveManager] No waves assigned.");
                return;
            }

            if (runRoutine != null) StopCoroutine(runRoutine);
            runRoutine = StartCoroutine(RunNight());
        }

        public void StopNight()
        {
            if (runRoutine != null) StopCoroutine(runRoutine);
            runRoutine = null;
            currentWaveIndex = -1;
            currentTracker = null;
            waitingForPreWavePhaseToEnd = false;
            firstWaveStarted = false;
        }

        private IEnumerator RunNight()
        {
            currentWaveIndex = -1;
            firstWaveStarted = false;

            // Run each wave
            for (int i = 0; i < waveSet.waves.Length; i++)
            {
                var wave = waveSet.waves[i];
                if (wave == null) continue;

                // === PRE-WAVE PHASE ===
                if (usePreWavePhase && preWavePhase != null)
                {
                    Debug.Log($"[WaveManager] Starting pre-wave phase for wave {i + 1}");
                    
                    // Configure pre-wave phase based on wave definition
                    bool useTimer = wave.UsesTimer();
                    float timerDuration = useTimer ? wave.startDelay : 0f;
                    
                    // Set timer settings before starting phase
                    preWavePhase.SetTimeLimit(timerDuration);
                    preWavePhase.ToggleTimeLimit(useTimer);
                    
                    // Start the pre-wave phase WITH wave definition for preview
                    preWavePhase.StartPhase(i + 1, wave); // ← CHANGED: Added wave parameter
                    waitingForPreWavePhaseToEnd = true;

                    // Wait until player clicks "Start Wave" button or timer expires
                    yield return new WaitUntil(() => !waitingForPreWavePhaseToEnd);
                    
                    Debug.Log($"[WaveManager] Pre-wave phase ended, starting wave {i + 1}");
                }
                else
                {
                    // Fallback to old behavior if pre-wave phase is disabled
                    if (hudButtons != null)
                        hudButtons.StartWavePhase(i);

                    float delay = Mathf.Max(0f, wave.startDelay);

                    if (wave.startCondition == WaveStartCondition.TimerAfterPreviousStart)
                    {
                        if (delay > 0f) yield return new WaitForSeconds(delay);
                    }
                    else if (wave.startCondition == WaveStartCondition.TimerAfterPreviousComplete ||
                             wave.startCondition == WaveStartCondition.ManualOrTimerAfterComplete)
                    {
                        if (i > 0)
                        {
                            while (currentTracker != null && !currentTracker.IsComplete())
                                yield return null;
                        }

                        if (delay > 0f) yield return new WaitForSeconds(delay);
                    }
                    else if (wave.startCondition == WaveStartCondition.ManualStartOnly)
                    {
                        if (i > 0)
                        {
                            while (currentTracker != null && !currentTracker.IsComplete())
                                yield return null;
                        }
                    }
                }

                // === FIRST WAVE INITIALIZATION ===
                if (!firstWaveStarted)
                {
                    firstWaveStarted = true;
                    OnFirstWaveStarted();
                }

                // === WAVE EXECUTION ===
                currentWaveIndex = i;
                currentTracker = new WaveRuntimeTracker(i, wave);

                Debug.Log($"[WaveManager] Starting wave {i + 1}/{waveSet.waves.Length}: {wave.waveName}");

                // Notify systems: wave starting
                OnWaveStarted(i);

                // Notify HUD: wave starting (if not using pre-wave phase)
                if (!usePreWavePhase && hudButtons != null)
                    hudButtons.StartWavePhase(i);

                // Spawn wave (async) with scaling callback
                System.Action<GameObject> spawnCallback = (enemy) =>
                {
                    HandleEnemySpawned(enemy);
                    
                    // Apply difficulty scaling
                    if (applyDifficultyScaling && waveSet != null)
                    {
                        WaveScaler.ApplyScaling(enemy, waveSet, i);
                    }
                };

                yield return StartCoroutine(spawner.SpawnWave(wave, currentTracker, spawnCallback));

                // Wait for completion if required
                while (!currentTracker.IsComplete())
                    yield return null;

                Debug.Log($"[WaveManager] Completed wave {i + 1}: {wave.waveName}");

                // === WAVE COMPLETED ===
                OnWaveCleared(i);

                // Notify PreWavePhaseUI that wave is complete
                if (usePreWavePhase && preWavePhase != null)
                {
                    var phaseUI = preWavePhase.GetComponentInChildren<PreWavePhaseUI>();
                    if (phaseUI == null)
                        phaseUI = FindFirstObjectByType<PreWavePhaseUI>();
                    
                    if (phaseUI != null)
                    {
                        phaseUI.OnWaveCompleted();
                    }
                }

                // Rewards with scaling
                if (rewardReceiver != null)
                {
                    float rewardMultiplier = waveSet != null ? waveSet.GetRewardMultiplier(i) : 1f;
                    int scaledGold = Mathf.RoundToInt(wave.rewardGold * rewardMultiplier);
                    int scaledExp = Mathf.RoundToInt(wave.rewardExp * rewardMultiplier);
                    int scaledCelestium = Mathf.RoundToInt(wave.rewardCelestium * rewardMultiplier);
                    
                    rewardReceiver.GrantWaveRewards(scaledGold, scaledExp, scaledCelestium);
                }

                // Notify HUD: wave ended
                if (hudButtons != null)
                    hudButtons.EndWavePhase();
            }

            Debug.Log("[WaveManager] Night complete (all waves finished).");
            
            // === ALL WAVES COMPLETE ===
            OnAllWavesComplete();

            runRoutine = null;

            // Notify HUD: all waves complete
            if (hudButtons != null)
                hudButtons.EndWavePhase();
        }

        /// <summary>
        /// Called when the first wave starts - triggers lighting and initial sounds
        /// </summary>
        private void OnFirstWaveStarted()
        {
            Debug.Log("[WaveManager] First wave starting - triggering systems");

            // Dim lighting gradually
            if (useDynamicLighting && lightingController != null)
            {
                lightingController.OnWavesStarted();
            }

            // Play waves started sound
            if (playWaveSounds && audioConfig != null)
            {
                audioConfig.PlaySound(audioConfig.wavesStartSound, audioConfig.waveEventVolume);
            }

            // Show notification
            if (Havengard.UI.Notifications.NotificationManager.Instance != null)
            {
                Havengard.UI.Notifications.NotificationManager.Instance.Show(
                    "Wave Battle Started!",
                    Havengard.UI.Notifications.NotificationType.Warning
                );
            }

            // Invoke event
            waveEvents?.OnWavesStarted?.Invoke();
        }

        /// <summary>
        /// Called when an individual wave starts
        /// </summary>
        private void OnWaveStarted(int waveIndex)
        {
            Debug.Log($"[WaveManager] Wave {waveIndex + 1} started");

            // Play wave start sound
            if (playWaveSounds && audioConfig != null)
            {
                audioConfig.PlaySound(audioConfig.waveStartSound, audioConfig.waveEventVolume);
            }

            // Show notification
            if (Havengard.UI.Notifications.NotificationManager.Instance != null)
            {
                string waveName = waveSet.waves[waveIndex].waveName;
                Havengard.UI.Notifications.NotificationManager.Instance.Show(
                    $"Wave {waveIndex + 1}: {waveName}",
                    Havengard.UI.Notifications.NotificationType.Info
                );
            }

            // Invoke event
            waveEvents?.OnWaveStarted?.Invoke(waveIndex);
        }

        /// <summary>
        /// Called when a wave is cleared
        /// </summary>
        private void OnWaveCleared(int waveIndex)
        {
            Debug.Log($"[WaveManager] Wave {waveIndex + 1} cleared!");

            // Play wave cleared sound
            if (playWaveSounds && audioConfig != null)
            {
                audioConfig.PlaySound(audioConfig.waveClearedSound, audioConfig.waveEventVolume);
            }

            // Show notification
            if (Havengard.UI.Notifications.NotificationManager.Instance != null)
            {
                Havengard.UI.Notifications.NotificationManager.Instance.Show(
                    $"Wave {waveIndex + 1} Cleared!",
                    Havengard.UI.Notifications.NotificationType.Success
                );
            }

            // Invoke event
            waveEvents?.OnWaveCleared?.Invoke(waveIndex);
        }

        /// <summary>
        /// Called when all waves are complete
        /// </summary>
        private void OnAllWavesComplete()
        {
            Debug.Log("[WaveManager] All waves complete - triggering victory sequence");

            // Restore lighting gradually
            if (useDynamicLighting && lightingController != null)
            {
                lightingController.OnWavesCompleted();
            }

            // Play all waves complete sound
            if (playWaveSounds && audioConfig != null)
            {
                audioConfig.PlaySound(audioConfig.allWavesCompleteSound, audioConfig.waveEventVolume);
            }

            // Show notification
            if (Havengard.UI.Notifications.NotificationManager.Instance != null)
            {
                Havengard.UI.Notifications.NotificationManager.Instance.Show(
                    "All Waves Complete!",
                    Havengard.UI.Notifications.NotificationType.Success
                );
            }

            // Invoke event
            waveEvents?.OnAllWavesComplete?.Invoke();

            // Trigger level complete after a short delay
            StartCoroutine(TriggerLevelCompleteDelayed(2f));
        }

        /// <summary>
        /// Trigger level complete with rewards
        /// </summary>
        private IEnumerator TriggerLevelCompleteDelayed(float delay)
        {
            yield return new WaitForSeconds(delay);

            Debug.Log("[WaveManager] Level Complete!");

            // Play level complete sound
            if (playWaveSounds && audioConfig != null)
            {
                audioConfig.PlaySound(audioConfig.levelCompleteSound, audioConfig.victoryVolume);
            }

            // Show level complete notification
            if (Havengard.UI.Notifications.NotificationManager.Instance != null)
            {
                Havengard.UI.Notifications.NotificationManager.Instance.Show(
                    "🎉 Level Complete! 🎉",
                    Havengard.UI.Notifications.NotificationType.Success
                );
            }

            // Invoke event (connect to rewards panel, level transition, etc.)
            waveEvents?.OnLevelComplete?.Invoke();
        }

        /// <summary>
        /// Called when the pre-wave phase ends (player clicked "Start Wave")
        /// </summary>
        private void OnPreWavePhaseEnded()
        {
            Debug.Log("[WaveManager] Pre-wave phase ended callback received");
            waitingForPreWavePhaseToEnd = false;
        }

        /// <summary>
        /// Called when an enemy is spawned - registers it with the tracker and hooks up death event
        /// </summary>
        private void HandleEnemySpawned(GameObject enemy)
        {
            if (currentTracker == null || enemy == null) return;

            // Hook up the death event to track when enemies die
            var health = enemy.GetComponent<Havengard.Core.HealthSystem.Health>();
            if (health != null)
            {
                health.OnDeath += () => HandleEnemyDeath(enemy);
                Debug.Log($"[WaveManager] Registered death listener for enemy: {enemy.name}");
            }
            else
            {
                Debug.LogWarning($"[WaveManager] Enemy {enemy.name} has no Health component!");
            }
        }

        /// <summary>
        /// Called when an enemy dies - notifies the tracker
        /// </summary>
        private void HandleEnemyDeath(GameObject enemy)
        {
            if (currentTracker != null && enemy != null)
            {
                currentTracker.MarkDead(enemy);
                Debug.Log($"[WaveManager] Enemy died: {enemy.name}. Remaining: {currentTracker.AliveCount}/{currentTracker.SpawnedCount}");
            }
        }
    }
}