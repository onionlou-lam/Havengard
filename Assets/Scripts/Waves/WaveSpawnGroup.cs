using UnityEngine;

namespace Havengard.Waves
{
    public enum SpawnZoneSelection
    {
        RandomZone,
        RoundRobin
    }

    public enum SpawnOrder
    {
        InOrder,
        Random
    }

    [CreateAssetMenu(menuName = "Havengard/Waves/Wave Spawn Group")]
    public class WaveSpawnGroup : ScriptableObject
    {
        public string groupName = "Group";

        [Tooltip("Delay before this group begins spawning (relative to wave start).")]
        public float delayFromWaveStart = 0f;

        [Tooltip("Time between individual spawns.")]
        public float spawnInterval = 0.4f;

        [Tooltip("How spawn zones are selected.")]
        public SpawnZoneSelection zoneSelection = SpawnZoneSelection.RandomZone;

        [Tooltip("How enemies are selected within the group.")]
        public SpawnOrder spawnOrder = SpawnOrder.InOrder;

        [Header("Entries")]
        public SpawnEntry[] entries;

        [System.Serializable]
        public class SpawnEntry
        {
            public GameObject enemyPrefab;
            public int count = 5;
        }
    }
}
