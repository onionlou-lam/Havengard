using System.Collections;
using UnityEngine;

namespace Havengard.Spawning
{
    /// <summary>
    /// Manages sequential spawning of waves from ScriptableObject definitions.
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        [SerializeField] private WaveData[] waves;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private Transform gateTarget;
        [SerializeField] private float delayBetweenWaves = 5f;

        private int currentWaveIndex;

        private void Start()
        {
            StartCoroutine(RunWaves());
        }

        private IEnumerator RunWaves()
        {
            for (currentWaveIndex = 0; currentWaveIndex < waves.Length; currentWaveIndex++)
            {
                yield return StartCoroutine(SpawnWave(waves[currentWaveIndex]));
                yield return new WaitForSeconds(delayBetweenWaves);
            }
            Debug.Log("All waves completed!");
        }

        private IEnumerator SpawnWave(WaveData wave)
        {
            foreach (var entry in wave.entries)
            {
                for (int i = 0; i < entry.count; i++)
                {
                    var point = spawnPoints[Random.Range(0, spawnPoints.Length)];
                    var unit = Instantiate(entry.unitPrefab, point.position, Quaternion.identity);

                    // If enemy, assign Gate
                    if (unit.TryGetComponent<Havengard.Units.EnemyUnit>(out var enemy) && gateTarget != null)
                    {
                        enemy.GetType().GetField("gateTarget",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            ?.SetValue(enemy, gateTarget);
                    }

                    yield return new WaitForSeconds(entry.interval);
                }
            }
        }
    }
}
