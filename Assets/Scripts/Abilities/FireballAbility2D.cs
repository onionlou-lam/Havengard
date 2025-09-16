using Havengard.Abilities;
using Havengard.Health;
using UnityEngine;

public class FireballAbility2D : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject projectilePrefab;
    public ParticleSystem castEffect;
    public ParticleSystem impactEffect;
    public ParticleSystem dotEffect;

    [Header("Settings")]
    public float projectileSpeed = 10f;
    public float damage = 20f;
    public float dotDamage = 3f;
    public float dotDuration = 5f;

    public void Cast(Vector3 targetPosition, Faction casterFaction)
    {
        // Play cast effect
        if (castEffect != null)
        {
            ParticleSystem cast = Instantiate(castEffect, transform.position, Quaternion.identity);
            Destroy(cast.gameObject, cast.main.duration);
        }

        // Spawn projectile
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        FireballProjectile2D proj = projectile.AddComponent<FireballProjectile2D>();
        proj.Init(targetPosition, projectileSpeed, damage, dotDamage, dotDuration, impactEffect, dotEffect, casterFaction);
    }
}
