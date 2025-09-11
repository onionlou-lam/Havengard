using UnityEngine;

namespace Havengard.Enemies
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private GameObject[] enemyPrefabs;
        [SerializeField] private float spawnInterval = 2.5f;
        [SerializeField] private Transform[] spawnPoints;

        private float timer;

        private void Update()
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                timer = spawnInterval;
                SpawnEnemy();
            }
        }

        private void SpawnEnemy()
        {
            if (enemyPrefabs.Length == 0 || spawnPoints.Length == 0) return;

            var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            var point = spawnPoints[Random.Range(0, spawnPoints.Length)];

            Instantiate(prefab, point.position, Quaternion.identity);
        }
    }
}
