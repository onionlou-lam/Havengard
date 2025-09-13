using UnityEngine;

/// <summary>
/// Simple enemy base. Implements IEnemy-like behavior (PerformAttack & OnDeath).
/// Inherit for specific enemy behaviours.
/// </summary>
public abstract class EnemyBase : UnitBase
{
    // Optionally expose special enemy-only fields here

    public override void PerformAttack(GameObject target)
    {
        // If a subclass forgot to override, this is called.
        Debug.LogWarning($"{name} attempted to PerformAttack but no implementation provided.");
    }

    // You can override OnDestroy or Die to provide common enemy death logic
    protected override void Die()
    {
        OnDeath();
        base.Die();
    }

    protected virtual void OnDeath()
    {
        // default enemy death hook
        Debug.Log($"{name} died (EnemyBase.OnDeath).");
    }
}
