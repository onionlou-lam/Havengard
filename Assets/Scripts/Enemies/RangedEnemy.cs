using UnityEngine;

public class RangedEnemy : UnitBase
{
    [Header("Ranged Settings")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 10f;

    protected override void PerformAttack(Transform target)
    {
        if (projectilePrefab == null || firePoint == null) return;

        var proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        if (proj.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 dir = (target.position - firePoint.position).normalized;
            rb.velocity = dir * projectileSpeed;
        }
        Debug.Log($"{name} fires at {target.name}");
    }
}
