using UnityEngine;

namespace Havengard.Enemies
{
    public interface IEnemy
    {
        void PerformAttack(GameObject target);
        void OnDeath();
    }
}
