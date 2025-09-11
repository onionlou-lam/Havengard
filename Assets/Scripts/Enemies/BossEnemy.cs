using UnityEngine;

namespace Havengard.Enemies
{
    public class BossEnemy : EnemyBase
    {
        [SerializeField] private float specialAbilityCooldown = 5f;
        private float lastAbilityTime;

        public override void PerformAttack(GameObject target)
        {
            if (Time.time >= lastAbilityTime + specialAbilityCooldown)
            {
                Debug.Log("Boss uses special ability!");
                lastAbilityTime = Time.time;
            }
            else
            {
                Debug.Log("Boss uses basic attack!");
            }
        }
    }
}
