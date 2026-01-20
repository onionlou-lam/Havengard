/*using UnityEngine;
using Havengard.Units;   // ⬅️ important

namespace Havengard.Units
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Enemy Prefabs")]
        [SerializeField] private UnitBase[] enemyPrefabs; // was EnemyUnit[]
        [SerializeField] private Transform[] spawnPoints;

        public UnitBase SpawnRandom()
        {
            if (enemyPrefabs == null || enemyPrefabs.Length == 0) return null;
            var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            var spawn = spawnPoints.Length > 0 ? spawnPoints[Random.Range(0, spawnPoints.Length)] : transform;
            return Instantiate(prefab, spawn.position, Quaternion.identity);
        }

        public T Spawn<T>(T prefab, Vector3 position) where T : UnitBase
        {
            return Instantiate(prefab, position, Quaternion.identity);
        }
    }
}
*/