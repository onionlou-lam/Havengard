using System.Collections.Generic;
using UnityEngine;
using Havengard.Units;   // ⬅️ important

namespace Havengard.Units
{
    public class WaveManager : MonoBehaviour
    {
        [Header("Wave Prefabs")]
        [SerializeField] private UnitBase[] enemyPrefabs; // was EnemyUnit[]
        [SerializeField] private Transform[] spawnPoints;

        private readonly List<UnitBase> activeEnemies = new(); // was List<EnemyUnit>

        public void SpawnWave(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
                var spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
                var enemy = Instantiate(prefab, spawn.position, Quaternion.identity);
                activeEnemies.Add(enemy);
            }
        }

        public int ActiveEnemyCount() => activeEnemies.RemoveAll(e => e == null) >= 0 ? activeEnemies.Count : 0;

        // Optional: call this occasionally to prune nulls
        public void Prune()
        {
            activeEnemies.RemoveAll(e => e == null);
        }
    }
}
