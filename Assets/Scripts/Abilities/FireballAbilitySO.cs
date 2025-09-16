using UnityEngine;
using Havengard.Abilities;
using Havengard.Health;

[CreateAssetMenu(fileName = "FireballAbility", menuName = "Abilities/Fireball")]
public class FireballAbilitySO : AbilityBase
{
    [Header("Prefabs")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private ParticleSystem castEffect;
    [SerializeField] private ParticleSystem impactEffect;
    [SerializeField] private ParticleSystem dotEffect;

    [Header("Settings")]
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float damage = 20f;
    [SerializeField] private float dotDamage = 3f;
    [SerializeField] private float dotDuration = 5f;

    protected override void Execute(GameObject caster, GameObject target)
    {
        Debug.Log($"FireballAbilitySO: Execute called. Caster = {caster.name}, Target = {target?.name}");

        if (projectilePrefab == null)
        {
            Debug.LogError("FireballAbilitySO: Projectile prefab is not assigned.");
            return;
        }

        if (target == null)
        {
            Debug.LogWarning("FireballAbilitySO: No target selected for fireball.");
            return;
        }

        Debug.Log("FireballAbilitySO: Instantiating projectile...");
        
        if (projectilePrefab == null)
        {
            Debug.LogError("FireballAbilitySO: Projectile prefab is not assigned.");
            return;
        }

        if (target == null)
        {
            Debug.LogWarning("FireballAbilitySO: No target selected for fireball.");
            return;
        }

        Vector3 targetPosition = target.transform.position;

        if (castEffect != null)
        {
            ParticleSystem cast = Instantiate(castEffect, caster.transform.position, Quaternion.identity);
            Destroy(cast.gameObject, cast.main.duration);
        }

        GameObject projectile = Instantiate(projectilePrefab, caster.transform.position, Quaternion.identity);
        FireballProjectile2D proj = projectile.AddComponent<FireballProjectile2D>();

        Faction casterFaction = caster.TryGetComponent(out FactionProvider factionProvider)
            ? factionProvider.Faction
            : Faction.Neutral;

        proj.Init(targetPosition, projectileSpeed, damage, dotDamage, dotDuration, impactEffect, dotEffect, casterFaction);

        Debug.Log($"FireballAbilitySO: {caster.name} cast Fireball at {target.name}.");
    }
}