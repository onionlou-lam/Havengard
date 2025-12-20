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

        [Header("References")]
        [SerializeField] private WaveSpawner spawner;

        [Tooltip("Optional receiver for Gold/EXP/Celestium wave rewards.")]
        [SerializeField] private MonoBehaviour rewardReceiverBehaviour;

        private IWaveRewardReceiver rewardReceiver;

        private int currentWaveIndex = -1;
        private WaveRuntimeTracker currentTracker;
        private Coroutine runRoutine;

        public bool IsRunning => runRoutine != null;
        public int CurrentWaveIndex => currentWaveIndex;

        private void Awake()
        {
            if (spawner == null) spawner = GetComponent<WaveSpawner>();
            if (spawner == null) spawner = gameObject.AddComponent<WaveSpawner>();

            spawner.SetZones(spawnZones);

            rewardReceiver = rewardReceiverBehaviour as IWaveRewardReceiver;
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
        }

        private IEnumerator RunNight()
        {
            currentWaveIndex = -1;

            // Run each wave
            for (int i = 0; i < waveSet.waves.Length; i++)
            {
                var wave = waveSet.waves[i];
                if (wave == null) continue;

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

                currentWaveIndex = i;
                currentTracker = new WaveRuntimeTracker(i, wave);

                Debug.Log($"[WaveManager] Starting wave {i + 1}/{waveSet.waves.Length}: {wave.waveName}");

                // Spawn wave (async)
                yield return StartCoroutine(spawner.SpawnWave(wave, currentTracker, HandleEnemySpawned));

                // Wait for completion if required
                while (!currentTracker.IsComplete())
                    yield return null;

                Debug.Log($"[WaveManager] Completed wave {i + 1}: {wave.waveName}");

                // Rewards
                if (rewardReceiver != null)
                    rewardReceiver.GrantWaveRewards(wave.rewardGold, wave.rewardExp, wave.rewardCelestium);
            }

            Debug.Log("[WaveManager] Night complete (all waves finished).");
            runRoutine = null;
        }

        private void HandleEnemySpawned(GameObject enemy)
        {
            // We need to detect death to decrement alive count
            // You already use Health + OnDeath for enemies.
            var health = enemy.GetComponent<Havengard.HealthSystem.Health>();
            if (health != null)
            {
                health.OnDeath += () =>
                {
                    if (currentTracker != null)
                        currentTracker.MarkDead(enemy);
                };
            }
            else
            {
                // If no Health component, still allow wave to finish as "all spawned only"
                Debug.LogWarning($"[WaveManager] Spawned enemy {enemy.name} has no Health component.");
            }
        }
    }
}
