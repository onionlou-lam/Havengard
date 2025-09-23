using UnityEngine;

namespace Havengard.Units
{
    public class EnemyRewards : MonoBehaviour
    {
        [SerializeField] private int expValue = 20;
        [SerializeField] private int goldValue = 5;
        [SerializeField] private int celestiumValue = 1;

        public int ExpValue => expValue;
        public int GoldValue => goldValue;
        public int CelestiumValue => celestiumValue;
    }
}
