/*using UnityEngine;

/// <summary>
/// Friendly melee unit: same logic as MeleeEnemy but intended to target enemies.
/// Make sure inspector targetLayer is set to the enemy layer.
/// </summary>
public class FriendlyMelee : UnitBase
{
    [SerializeField] private float damage = 12f;
    [SerializeField] private float approachMultiplier = 1.25f;

    protected override void TryAttack(GameObject target)
    {
        float dist = Vector2.Distance(transform.position, target.transform.position);

        if (dist <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            PerformAttack(target);
            lastAttackTime = Time.time;
        }
        else
        {
            Vector3 dir = (target.transform.position - transform.position).normalized;
            transform.position += dir * (moveSpeed * approachMultiplier * Time.deltaTime);
        }
    }

    public override void PerformAttack(GameObject target)
    {
        float dist = Vector2.Distance(transform.position, target.transform.position);

        if (dist <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            PerformAttack(target);
            lastAttackTime = Time.time;
        }
        else
        {
            Vector3 dir = (target.transform.position - transform.position).normalized;
            transform.position += dir * (moveSpeed * approachMultiplier * Time.deltaTime);
        }
        if (target.TryGetComponent<UnitBase>(out var unit))
        {
            unit.TakeDamage(damage);
        }
    }
}
*/