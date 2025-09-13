using UnityEngine;

/// <summary>
/// Friendly ranged unit: fires projectiles at enemies. Set projectile prefab & enemy layer.
/// </summary>
public class FriendlyRanged : UnitBase
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 9f;

    protected override void TryAttack(GameObject target)
    {
        if (Time.time < lastAttackTime + attackCooldown) return;
        if (target == null) return;

        PerformAttack(target);
        lastAttackTime = Time.time;
    }

    public override void PerformAttack(GameObject target)
    {
        if (projectilePrefab == null || target == null) return;

        var proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        if (proj.TryGetComponent<Rigidbody2D>(out var rb))
        {
            Vector2 dir = (target.transform.position - transform.position).normalized;
            rb.linearVelocity = dir * projectileSpeed;
        }
    }
}
