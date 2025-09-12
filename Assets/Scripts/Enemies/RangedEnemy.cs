using UnityEngine;
using Havengard.HealthSystem;

namespace Havengard.Enemies
{
    public class RangedEnemy : EnemyBase
    {
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float projectileSpeed = 10f;
        [SerializeField] private float damage = 8f;

        public override void PerformAttack(GameObject target)
        {
            if (projectilePrefab == null || target == null) return;

            var projectileObj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            if (projectileObj.TryGetComponent<Projectile>(out var projectile))
            {
                projectile.Init(target.transform, damage, health.GetFaction());
            }
            else
            {
                Debug.LogError("Projectile prefab is missing a Projectile component!");
            }
        }
    }
}
