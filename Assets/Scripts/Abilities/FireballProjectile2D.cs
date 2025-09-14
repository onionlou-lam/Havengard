using Havengard.Health;
using UnityEngine;

public class FireballProjectile : MonoBehaviour
{
    private Vector3 target;
    private float speed;
    private float damage;
    private float dotDamage;
    private float dotDuration;
    private ParticleSystem impactEffect;
    private ParticleSystem dotEffect;
    private Faction casterFaction;

    public void Init(Vector3 targetPosition, float speed, float damage, float dotDamage, float dotDuration,
                     ParticleSystem impactEffect, ParticleSystem dotEffect, Faction casterFaction)
    {
        this.target = targetPosition;
        this.speed = speed;
        this.damage = damage;
        this.dotDamage = dotDamage;
        this.dotDuration = dotDuration;
        this.impactEffect = impactEffect;
        this.dotEffect = dotEffect;
        this.casterFaction = casterFaction;
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.1f)
        {
            OnHit();
        }
    }

    private void OnHit()
    {
        // Play impact effect
        if (impactEffect != null)
        {
            ParticleSystem impact = Instantiate(impactEffect, transform.position, Quaternion.identity);
            Destroy(impact.gameObject, impact.main.duration);
        }

        // Damage + DOT
        Collider[] hits = Physics.OverlapSphere(transform.position, 2f);
        foreach (Collider hit in hits)
        {
            IHealth health = hit.GetComponent<IHealth>();
            if (health != null && health.GetFaction() != casterFaction)
            {
                health.TakeDamage(damage);

                // Apply DoT flames
                if (dotEffect != null)
                {
                    ParticleSystem flames = Instantiate(dotEffect, hit.transform.position, Quaternion.identity, hit.transform);
                    Destroy(flames.gameObject, dotDuration);
                }

                // Start DoT coroutine
                hit.GetComponent<MonoBehaviour>().StartCoroutine(ApplyDoT(health));
            }
        }

        Destroy(gameObject);
    }

    private System.Collections.IEnumerator ApplyDoT(IHealth targetHealth)
    {
        float elapsed = 0f;
        while (elapsed < dotDuration)
        {
            targetHealth.TakeDamage(dotDamage);
            yield return new WaitForSeconds(1f);
            elapsed += 1f;
        }
    }
}
