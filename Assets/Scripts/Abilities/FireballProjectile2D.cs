using Havengard.Health;
using UnityEngine;

public class FireballProjectile2D : MonoBehaviour
{
    private Vector3 target;
    private float speed;
    private float damage;
    private float dotDamage;
    private float dotDuration;
    private ParticleSystem impactEffect;
    private ParticleSystem dotEffect;
    private Faction casterFaction;

    private float lifetime = 3f;
    private float timer;

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

        timer = 0f;
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        transform.position = new Vector3(transform.position.x, transform.position.y, 0f); // Ensure visible in 2D
        timer += Time.deltaTime;

        Debug.DrawLine(transform.position, target, Color.red, 0.1f);

        if (Vector3.Distance(transform.position, target) < 0.1f)
        {
            Debug.Log("FireballProjectile2D: Hit target position.");
            OnHit();
        }
        else if (timer >= lifetime)
        {
            Debug.Log("FireballProjectile2D: Timed out after 3 seconds. Destroying.");
            Destroy(gameObject);
        }
    }

    private void OnHit()
    {
        if (impactEffect != null)
        {
            ParticleSystem impact = Instantiate(impactEffect, transform.position, Quaternion.identity);
            Destroy(impact.gameObject, impact.main.duration);
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 2f);
        foreach (Collider2D hit in hits)
        {
            IHealth health = hit.GetComponent<IHealth>();
            if (health != null && health.GetFaction() != casterFaction)
            {
                health.TakeDamage(damage);
                Debug.Log($"FireballProjectile2D: Damaged {hit.name} for {damage}.");

                if (dotEffect != null)
                {
                    ParticleSystem flames = Instantiate(dotEffect, hit.transform.position, Quaternion.identity, hit.transform);
                    Destroy(flames.gameObject, dotDuration);
                }

                hit.GetComponent<MonoBehaviour>()?.StartCoroutine(ApplyDoT(health));
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
            Debug.Log($"FireballProjectile2D: Applied DoT ({dotDamage}) to {targetHealth}.");
            yield return new WaitForSeconds(1f);
            elapsed += 1f;
        }
    }
}