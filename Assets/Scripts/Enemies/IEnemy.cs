using UnityEngine;

public interface IEnemy
{
    void PerformAttack(GameObject target);
    void OnDeath();
}
