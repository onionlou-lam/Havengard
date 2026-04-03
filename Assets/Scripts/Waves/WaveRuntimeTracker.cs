using System.Collections.Generic;
using UnityEngine;

namespace Havengard.Waves
{
    public class WaveRuntimeTracker
    {
        public int WaveIndex { get; private set; }
        public WaveDefinition Definition { get; private set; }

        private int totalToSpawn;
        private int spawnedCount;
        private HashSet<GameObject> aliveEnemies = new HashSet<GameObject>();

        // ADD THIS CONSTRUCTOR
        public WaveRuntimeTracker(int waveIndex, WaveDefinition definition)
        {
            WaveIndex = waveIndex;
            Definition = definition;

            totalToSpawn = 0;
            if (definition != null && definition.groups != null)
            {
                foreach (var group in definition.groups)
                {
                    if (group != null && group.entries != null)
                    {
                        foreach (var entry in group.entries)
                        {
                            totalToSpawn += entry.count;
                        }
                    }
                }
            }
        }

        public void MarkSpawned(GameObject enemy)
        {
            spawnedCount++;
            aliveEnemies.Add(enemy);
        }

        public void MarkDead(GameObject enemy)
        {
            aliveEnemies.Remove(enemy);
        }

        public void MarkAllSpawned()
        {
            // All spawning complete
        }

        // ADD THIS METHOD
        public bool IsComplete()
        {
            return spawnedCount >= totalToSpawn && aliveEnemies.Count == 0;
        }

        public int AliveCount => aliveEnemies.Count;
        public int SpawnedCount => spawnedCount;
        public int TotalToSpawn => totalToSpawn;
    }
}