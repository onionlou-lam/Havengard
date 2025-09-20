using UnityEngine;
using Havengard.Units;

namespace Havengard.Spawning
{
    /// <summary>
    /// Generic spawner for both enemies and allies.
    /// - Enemies: assign Enemy prefabs + Gate reference.
    /// - Allies: assign Ally prefabs + no Gate required.
    /// </summary>
    public class UnitSpawner : MonoBehaviour
    {
        [Header("Spawn Setup")]
        [SerializeField] private GameObject[] unitPrefabs;
        [SerializeField] private Transform[] spawnPoints;

        [Tooltip("Assign the Gate for enemies. Leave empty for allies.")]
        [SerializeField] private Transform gateTarget;

        [Header("Timing")]
        [SerializeField] private float spawnInterval = 3f;
        [SerializeField] private int maxAlive = 20;

        private float timer;

        private void Update()
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                timer = spawnInterval;
                TrySpawnUnit();
            }
        }

        private void TrySpawnUnit()
        {
            if (unitPrefabs.Length == 0 || spawnPoints.Length == 0) return;
            if (CountAliveUnits() >= maxAlive) return;

            var prefab = unitPrefabs[Random.Range(0, unitPrefabs.Length)];
            var point = spawnPoints[Random.Range(0, spawnPoints.Length)];

            var unitObj = Instantiate(prefab, point.position, Quaternion.identity);

            // If spawned unit is Enemy, assign Gate reference
            if (unitObj.TryGetComponent<EnemyUnit>(out var enemy) && gateTarget != null)
            {
                enemy.GetType().GetField("gateTarget",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(enemy, gateTarget);
            }
        }

        private int CountAliveUnits()
        {
            return FindObjectsOfType<UnitBase>().Length;
        }
    }
}
