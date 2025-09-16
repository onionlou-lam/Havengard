using UnityEngine;

public class MeleeEnemy : UnitBase
{
    [Header("Melee Settings")]
    public float meleeDamage = 15f;

    protected override void PerformAttack(Transform target)
    {
        IHealth health = target.GetComponent<IHealth>();
        if (health != null && health.GetFaction() != GetFaction())
        {
            health.TakeDamage(meleeDamage);
            Debug.Log($"{name} slashes {target.name} for {meleeDamage} damage!");
        }
    }
}
