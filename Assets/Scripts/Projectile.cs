using Havengard.HealthSystem;
using UnityEngine;
using Havengard.Combat;

public class Projectile : MonoBehaviour
{
    private Transform target;
    private Vector3 targetPoint;
    private float damage;
    private float impactRadius;
    private float dotPercentage;
    private float dotDuration;
    private Faction sourceFaction;
    private float speed = 10f;
    private bool isSkillshot = false;

    // Homing version
    public void Init(Transform target, float damage, float impactRadius, float dotPercentage, float dotDuration, Faction sourceFaction, float speed = 10f)
    {
        this.target = target;
        this.damage = damage;
        this.impactRadius = impactRadius;
        this.dotPercentage = dotPercentage;
        this.dotDuration = dotDuration;
        this.sourceFaction = sourceFaction;
        this.speed = speed;
        isSkillshot = false;
    }

    // Skillshot version (point in space)
    public void Init(Vector3 targetPoint, float damage, float impactRadius, float dotPercentage, float dotDuration, Faction sourceFaction, float speed = 10f)
    {
        this.targetPoint = targetPoint;
        this.damage = damage;
        this.impactRadius = impactRadius;
        this.dotPercentage = dotPercentage;
        this.dotDuration = dotDuration;
        this.sourceFaction = sourceFaction;
        this.speed = speed;
        isSkillshot = true;
    }

    private void Update()
    {
        if (!isSkillshot)
        {
            if (target == null) { Destroy(gameObject); return; }
            transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

            if (Vector2.Distance(transform.position, target.position) < 0.1f)
            {
                Explode();
            }
        }
        else
        {
            // Move toward point
            transform.position = Vector2.MoveTowards(transform.position, targetPoint, speed * Time.deltaTime);

            if (Vector2.Distance(transform.position, targetPoint) < 0.1f)
            {
                Explode();
            }
        }
    }

    private void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, impactRadius);

        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent<IHealth>(out var health))
            {
                if (health.GetFaction() == sourceFaction) continue; // prevent friendly fire

                float finalDamage = (target != null && hit.transform == target) ? damage : damage * 0.5f;
                health.TakeDamage(finalDamage, sourceFaction);
                health.ApplyDoT(finalDamage * dotPercentage, dotDuration, sourceFaction);
            }
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, impactRadius);
    }
}
