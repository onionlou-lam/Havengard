using UnityEngine;

/// <summary>
/// Boss uses a special attack periodically and otherwise performs heavy single attacks.
/// </summary>
public class BossEnemy : EnemyBase
{
    [SerializeField] private float specialDamage = 30f;
    [SerializeField] private float basicDamage = 12f;
    [SerializeField] private float specialCooldown = 6f;
    private float lastSpecial = -999f;

    protected override void TryAttack(GameObject target)
    {
        if (Time.time >= lastSpecial + specialCooldown)
        {
            // use special attack
            PerformAttack(target);
            lastSpecial = Time.time;
        }
        else
        {
            // fallback to basic attack when in range
            float dist = Vector2.Distance(transform.position, target.transform.position);
            if (dist <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                if (target.TryGetComponent<UnitBase>(out var unit)) unit.TakeDamage(basicDamage);
                lastAttackTime = Time.time;
            }
            else
            {
                MoveTowards(target.transform.position);
            }
        }
    }

    public override void PerformAttack(GameObject target)
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
