using System.Collections.Generic;
using UnityEngine;

namespace Havengard.Waves
{
    public class WaveRuntimeTracker
    {
        public int WaveIndex { get; private set; }
        public WaveDefinition Definition { get; private set; }

        private readonly HashSet<GameObject> alive = new HashSet<GameObject>();
        public int AliveCount => alive.Count;

        public bool AllSpawned { get; private set; }

        public WaveRuntimeTracker(int waveIndex, WaveDefinition def)
        {
            WaveIndex = waveIndex;
            Definition = def;
        }

        public void MarkSpawned(GameObject enemy)
        {
            if (enemy != null) alive.Add(enemy);
        }

        public void MarkDead(GameObject enemy)
        {
            if (enemy != null) alive.Remove(enemy);
        }

        public void MarkAllSpawned()
        {
            AllSpawned = true;
        }

        public bool IsComplete()
        {
            switch (Definition.completeCondition)
            {
                case WaveCompleteCondition.AllSpawnedOnly:
                    return AllSpawned;

                case WaveCompleteCondition.AllSpawnedAndAllDead:
                default:
                    return AllSpawned && alive.Count == 0;
            }
        }
    }
}
