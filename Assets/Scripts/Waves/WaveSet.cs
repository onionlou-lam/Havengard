using UnityEngine;

namespace Havengard.Waves
{
    [CreateAssetMenu(menuName = "Havengard/Waves/Wave Set")]
    public class WaveSet : ScriptableObject
    {
        public string setName = "New Wave Set";
        public WaveDefinition[] waves;
    }
}
