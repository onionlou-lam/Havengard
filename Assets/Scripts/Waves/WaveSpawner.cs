using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Havengard.Waves
{
    [DisallowMultipleComponent]
    public class WaveSpawner : MonoBehaviour
    {
        [Header("Spawn Zones")]
        [SerializeField] private Transform[] spawnZones;

        private int rrIndex = 0;

        public void SetZones(Transform[] zones)
        {
            spawnZones = zones;
            rrIndex = 0;
        }

        public IEnumerator SpawnWave(WaveDefinition wave, WaveRuntimeTracker tracker, System.Action<GameObject> onSpawned)
        {
            if (wave == null || wave.groups == null)
            {
                tracker.MarkAllSpawned();
                yield break;
            }

            // Spawn each group via coroutines, but we want them to overlap if delays overlap.
            // We'll run them all and wait until all groups finished.
            List<Coroutine> running = new List<Coroutine>();

            foreach (var group in wave.groups)
            {
                if (group == null) continue;
                running.Add(StartCoroutine(SpawnGroup(group, tracker, onSpawned)));
            }

            // Wait until all groups are done by polling a small state we set per group.
            // (Simpler than tracking coroutine handles in Unity.)
            while (groupsStillSpawning) yield return null;

            tracker.MarkAllSpawned();
        }

        private int activeGroups = 0;
        private bool groupsStillSpawning => activeGroups > 0;

        private IEnumerator SpawnGroup(WaveSpawnGroup group, WaveRuntimeTracker tracker, System.Action<GameObject> onSpawned)
        {
            activeGroups++;

            if (group.delayFromWaveStart > 0f)
                yield return new WaitForSeconds(group.delayFromWaveStart);

            // Build spawn list
            List<GameObject> list = new List<GameObject>();
            foreach (var e in group.entries)
            {
                if (e.enemyPrefab == null || e.count <= 0) continue;
                for (int i = 0; i < e.count; i++) list.Add(e.enemyPrefab);
            }

            if (group.spawnOrder == SpawnOrder.Random)
            {
                // Shuffle
                for (int i = 0; i < list.Count; i++)
                {
                    int j = Random.Range(i, list.Count);
                    (list[i], list[j]) = (list[j], list[i]);
                }
            }

            // Spawn loop
            for (int i = 0; i < list.Count; i++)
            {
                var prefab = list[i];
                if (prefab == null) continue;

                Transform zone = ChooseZone(group.zoneSelection);
                Vector3 pos = zone != null ? zone.position : transform.position;

                GameObject enemy = Instantiate(prefab, pos, Quaternion.identity);
                tracker.MarkSpawned(enemy);
                onSpawned?.Invoke(enemy);

                if (group.spawnInterval > 0f)
                    yield return new WaitForSeconds(group.spawnInterval);
                else
                    yield return null;
            }

            activeGroups--;
        }

        private Transform ChooseZone(SpawnZoneSelection selection)
        {
            if (spawnZones == null || spawnZones.Length == 0) return null;

            if (selection == SpawnZoneSelection.RoundRobin)
            {
                var t = spawnZones[rrIndex % spawnZones.Length];
                rrIndex++;
                return t;
            }

            // Random
            return spawnZones[Random.Range(0, spawnZones.Length)];
        }
    }
}
