using UnityEngine;
using Havengard.Health;

public class EnemyBase : UnitBase, IEnemy
{
    [Header("Enemy Settings")]
    [SerializeField] private float attackDamage = 10f;

    public void PerformAttack(GameObject target)
    {
        if (target == null) return;

        if (target.TryGetComponent<IHealth>(out var health))
        {
            health.TakeDamage(attackDamage);
        }
    }

    public void OnDeath()
    {
        Debug.Log($"{name} died.");
        Destroy(gameObject);
    }
}
