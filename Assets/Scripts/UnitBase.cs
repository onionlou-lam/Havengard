using UnityEngine;

/// <summary>
/// Shared base for any unit (friendly or enemy).
/// Handles detection (OverlapCircle) and basic moving/attack loop.
/// Child classes implement PerformAttack and can override TryAttack to control approach/attack behavior.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public abstract class UnitBase : MonoBehaviour
{
    [Header("Core")]
    public float health = 100f;
    public float moveSpeed = 2.5f;

    [Header("Detection / Combat")]
    public float detectionRadius = 6f;
    public LayerMask targetLayer;        // set in inspector to what this unit should target
    public float attackRange = 1.5f;     // distance at which attack can occur
    public float attackCooldown = 1f;

    protected GameObject currentTarget;
    protected float lastAttackTime;

    protected virtual void Update()
    {
        DetectTarget();

        if (currentTarget != null)
        {
            TryAttack(currentTarget);
        }
        else
        {
            // idle behavior - can be overridden (patrol, roam, move to gate, etc.)
            IdleBehavior();
        }
    }

    protected virtual void IdleBehavior() { /* no-op by default */ }

    protected virtual void DetectTarget()
    {
        // If a target already exists, don't change it here (child may clear it when dead)
        if (currentTarget != null)
        {
            if (!IsTargetValid(currentTarget)) currentTarget = null;
            else return;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, targetLayer);
        if (hits.Length > 0)
        {
            // simple: pick the first detected target; you can add prioritization later
            currentTarget = hits[0].gameObject;
            OnTargetAcquired(currentTarget);
        }
    }

    protected virtual bool IsTargetValid(GameObject target)
    {
        return target != null && target.activeInHierarchy;
    }

    protected virtual void OnTargetAcquired(GameObject target)
    {
        // override in subclasses for e.g. move-to or prepare
    }

    /// <summary>
    /// Attempt to attack the target. Child classes control approach and calling PerformAttack.
    /// </summary>
    protected virtual void TryAttack(GameObject target)
    {
        // default behaviour: if in range and cooldown passed, call PerformAttack
        float dist = Vector2.Distance(transform.position, target.transform.position);
        if (dist <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            PerformAttack(target);
            lastAttackTime = Time.time;
        }
        else
        {
            // move toward the target
            MoveTowards(target.transform.position);
        }
    }

    protected void MoveTowards(Vector3 pos)
    {
        Vector3 dir = (pos - transform.position).normalized;
        transform.position += dir * (moveSpeed * Time.deltaTime);
    }

    public virtual void PerformAttack(GameObject target)
    {
        // default fallback (children should override)
        Debug.Log($"{name} performs a default attack on {target.name}");
    }

    public virtual void TakeDamage(float amount)
    {
        health -= amount;
        // you can add hit reactions/particles here

        if (health <= 0f)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        // you can add death animation here
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
