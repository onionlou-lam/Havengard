/* using UnityEngine;

namespace Havengard.Spawning
{
    [CreateAssetMenu(menuName = "Havengard/Spawning/Wave Data")]
    public class WaveData : ScriptableObject
    {
        [System.Serializable]
        public struct WaveEntry
        {
            public GameObject unitPrefab;
            public int count;
            public float interval; // time between each spawn of this type
        }

        [Tooltip("List of units to spawn in this wave.")]
        public WaveEntry[] entries;
    }
}
*/