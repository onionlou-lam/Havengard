using UnityEngine;

namespace Havengard.Enemies
{
    public class RangedEnemy : EnemyBase
    {
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float projectileSpeed = 10f;

        public override void PerformAttack(GameObject target)
        {
            if (projectilePrefab == null) return;

            var projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            Vector3 direction = (target.transform.position - transform.position).normalized;
            projectile.GetComponent<Rigidbody2D>().linearVelocity = direction * projectileSpeed;
        }
    }
}
