using UnityEngine;
using Havengard.Health;

public class CleaveAbility2D : MonoBehaviour
{
    [SerializeField] private float radius = 2f;
    [SerializeField] private float damage = 20f;
    [SerializeField] private bool allowFriendly = false;

    public void PerformCleave()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (var c in hits)
        {
            if (c.TryGetComponent<IHealth>(out var hp))
            {
                if (!allowFriendly && hp.GetFaction() == GetComponent<FactionProvider>().Faction) continue;
                hp.TakeDamage(damage);
            }
        }
    }
}
