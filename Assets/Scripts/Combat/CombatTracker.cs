using UnityEngine;

namespace Havengard.Combat
{
    public class CombatTracker : MonoBehaviour
    {
        public GameObject LastAttacker { get; private set; }

        public void RecordHit(GameObject attacker)
        {
            LastAttacker = attacker;
        }
    }
}
