using UnityEngine;

namespace Havengard.Units
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Spawn Setup")]
        [SerializeField] private GameObject[] enemyPrefabs;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private Transform gateTarget;

        [Header("Timing")]
        [SerializeField] private float spawnInterval = 2.5f;
        [SerializeField] private int maxAlive = 20;

        private float timer;

        private void Update()
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                timer = spawnInterval;
                TrySpawnEnemy();
            }
        }

        private void TrySpawnEnemy()
        {
            if (enemyPrefabs.Length == 0 || spawnPoints.Length == 0) return;
            if (CountAliveEnemies() >= maxAlive) return;

            var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            var point = spawnPoints[Random.Range(0, spawnPoints.Length)];

            var enemyObj = Instantiate(prefab, point.position, Quaternion.identity);

            if (enemyObj.TryGetComponent<EnemyUnit>(out var enemy))
            {
                enemy.GetType().GetField("gateTarget",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(enemy, gateTarget);
            }
        }

        private int CountAliveEnemies()
        {
            return FindObjectsOfType<EnemyUnit>().Length;
        }
    }
}
