using System.Collections;
using UnityEngine;

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

        private IWaveRewardReceiver rewardReceiver;

        private int currentWaveIndex = -1;
        private WaveRuntimeTracker currentTracker;
        private Coroutine runRoutine;
        private bool waitingForPreWavePhaseToEnd = false;

        public bool IsRunning => runRoutine != null;
        public int CurrentWaveIndex => currentWaveIndex;

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

            // Subscribe to PreWavePhase events
            if (preWavePhase != null)
            {
                preWavePhase.OnPhaseEnded.AddListener(OnPreWavePhaseEnded);
                Debug.Log("[WaveManager] Subscribed to PreWavePhase events");
            }
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
        }

        private IEnumerator RunNight()
        {
            currentWaveIndex = -1;

            // Run each wave
            for (int i = 0; i < waveSet.waves.Length; i++)
            {
                var wave = waveSet.waves[i];
                if (wave == null) continue;

                // === PRE-WAVE PHASE ===
                if (usePreWavePhase && preWavePhase != null)
                {
                    Debug.Log($"[WaveManager] Starting pre-wave phase for wave {i + 1}");
                    
                    // Start the pre-wave phase
                    preWavePhase.StartPhase(i + 1);
                    waitingForPreWavePhaseToEnd = true;

                    // Wait until player clicks "Start Wave" button
                    yield return new WaitUntil(() => !waitingForPreWavePhaseToEnd);
                    
                    Debug.Log($"[WaveManager] Pre-wave phase ended, starting wave {i + 1}");
                }
                else
                {
                    // Fallback to old behavior if pre-wave phase is disabled
                    // Notify HUD: wave starting
                    if (hudButtons != null)
                        hudButtons.StartWavePhase(i);

                    // Start condition
                    float delay = Mathf.Max(0f, wave.startDelay);

                    if (wave.startCondition == WaveStartCondition.TimerAfterPreviousStart)
                    {
                        // Timer begins immediately (wave starts regardless of previous completion)
                        if (delay > 0f) yield return new WaitForSeconds(delay);
                    }
                    else
                    {
                        // Timer after previous completion
                        if (i > 0)
                        {
                            // ensure previous wave tracker complete
                            while (currentTracker != null && !currentTracker.IsComplete())
                                yield return null;
                        }

                        if (delay > 0f) yield return new WaitForSeconds(delay);
                    }
                }

                // === WAVE EXECUTION ===
                currentWaveIndex = i;
                currentTracker = new WaveRuntimeTracker(i, wave);

                Debug.Log($"[WaveManager] Starting wave {i + 1}/{waveSet.waves.Length}: {wave.waveName}");

                // Notify HUD: wave starting (if not using pre-wave phase)
                if (!usePreWavePhase && hudButtons != null)
                    hudButtons.StartWavePhase(i);

                // Spawn wave (async)
                yield return StartCoroutine(spawner.SpawnWave(wave, currentTracker, HandleEnemySpawned));

                // Wait for completion if required
                while (!currentTracker.IsComplete())
                    yield return null;

                Debug.Log($"[WaveManager] Completed wave {i + 1}: {wave.waveName}");

                // Rewards
                if (rewardReceiver != null)
                    rewardReceiver.GrantWaveRewards(wave.rewardGold, wave.rewardExp, wave.rewardCelestium);

                // Notify HUD: wave ended
                if (hudButtons != null)
                    hudButtons.EndWavePhase();
            }

            Debug.Log("[WaveManager] Night complete (all waves finished).");
            runRoutine = null;

            // Notify HUD: all waves complete
            if (hudButtons != null)
                hudButtons.EndWavePhase();
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