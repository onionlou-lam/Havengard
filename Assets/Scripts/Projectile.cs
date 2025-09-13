using UnityEngine;
using Havengard;
using Havengard.HealthSystem;

public class Projectile : MonoBehaviour
{
    private Transform target;
    private float damage;
    private Faction sourceFaction;

    public void Init(Transform target, float damage, Faction sourceFaction)
    {
        this.target = target;
        this.damage = damage;
        this.sourceFaction = sourceFaction;
    }

    void Update()
    {
        if (target == null) { Destroy(gameObject); return; }

        // Move towards target
        transform.position = Vector2.MoveTowards(transform.position, target.position, 10f * Time.deltaTime);

        // Hit check
        if (Vector2.Distance(transform.position, target.position) < 0.1f)
        {
            if (target.TryGetComponent<IHealth>(out var health))
            {
                health.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
    }
}
