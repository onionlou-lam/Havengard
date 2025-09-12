using UnityEngine;
using Havengard.HealthSystem;
using Havengard.Combat;

public class FireballProjectile : MonoBehaviour
{
    private Transform target;
    private Vector3 travelPos;
    private float damage;
    private float splashRadius;
    private float dotDuration;
    private float dotPercent;
    private float speed;
    private Faction sourceFaction;

    private bool travelToPoint;

    public void Init(Transform target, float damage, float splashRadius, float dotDuration, float dotPercent, float speed, Faction sourceFaction, Vector3? point = null)
    {
        this.target = target;
        this.damage = damage;
        this.splashRadius = splashRadius;
        this.dotDuration = dotDuration;
        this.dotPercent = dotPercent;
        this.speed = speed;
        this.sourceFaction = sourceFaction;

        if (point.HasValue)
        {
            travelToPoint = true;
            travelPos = point.Value;
        }
    }

    private void Update()
    {
        Vector3 destination = travelToPoint ? travelPos : target?.position ?? transform.position;

        transform.position = Vector2.MoveTowards(transform.position, destination, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, destination) < 0.1f)
        {
            Explode();
        }
    }

    private void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, splashRadius);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IHealth>(out var health))
            {
                if (health.GetFaction() == sourceFaction) continue; // no friendly fire

                // Full damage for main target, half for others
                float appliedDamage = (hit.transform == target) ? damage : damage * 0.5f;
                health.TakeDamage(appliedDamage, sourceFaction);

                // Apply DoT
                health.ApplyDoT(appliedDamage * dotPercent, dotDuration, sourceFaction);
            }
        }
        Destroy(gameObject);
    }
}
