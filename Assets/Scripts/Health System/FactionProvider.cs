using UnityEngine;

namespace Havengard.Health
{
    [DisallowMultipleComponent]
    public class FactionProvider : MonoBehaviour
    {
        [SerializeField] private Faction faction = Faction.Neutral;
        public Faction Faction => faction;
    }
}
