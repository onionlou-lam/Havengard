using UnityEngine;

/// <summary>
/// Boss uses a special attack periodically and otherwise performs heavy single attacks.
/// </summary>
public class BossEnemy : UnitBase
{
    [SerializeField] private float specialDamage = 30f;
    [SerializeField] private float basicDamage = 12f;
    [SerializeField] private float specialCooldown = 6f;
    private float lastSpecial = -999f;

    protected override void PerformAttack(Transform target)
    {
        // special area attack around boss
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);
        foreach (var h in hits)
        {
            if (h.gameObject == gameObject) continue;
            if (h.TryGetComponent<UnitBase>(out var unit))
            {
                unit.TakeDamage(specialDamage);
            }
        }
        Debug.Log($"{name} used SPECIAL attack for {specialDamage} damage.");
    }
}
