using UnityEngine;

/// <summary>
/// Enemy that uses a special ability (single-target or AoE) on cooldown.
/// NOTE: remove any "override Awake()"—we use normal Awake if needed.
/// </summary>
public class AbilityEnemy : EnemyBase
{
    [SerializeField] private float abilityDamage = 15f;
    [SerializeField] private float abilityCooldown = 4f;
    private float lastAbilityTime = -999f;

    // If you need to initialize something, use Awake() but do NOT use "override" unless base has Awake()
    void Awake()
    {
        // initialization if needed
    }

    protected override void TryAttack(GameObject target)
    {
        if (Time.time >= lastAbilityTime + abilityCooldown)
        {
            PerformAttack(target);
            lastAbilityTime = Time.time;
        }
        else
        {
            // approach or basic attack fallback
            float dist = Vector2.Distance(transform.position, target.transform.position);
            if (dist > attackRange)
                MoveTowards(target.transform.position);
        }
    }

    public override void PerformAttack(GameObject target)
    {
        if (target.TryGetComponent<UnitBase>(out var unit))
        {
            unit.TakeDamage(abilityDamage);
            Debug.Log($"{name} used ability on {target.name} for {abilityDamage} damage.");
        }
    }
}
