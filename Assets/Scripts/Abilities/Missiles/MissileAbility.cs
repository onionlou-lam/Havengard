using UnityEngine;
using Havengard.Units;
using Havengard.HealthSystem;
using Havengard.Character;
using Havengard.Statuses;

namespace Havengard.Abilities
{
    [CreateAssetMenu(menuName = "Havengard/Abilities/Missile Ability")]
    public class MissileAbility : AbilityBase
    {
        [Header("Projectile")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float projectileSpeed = 10f;
        [SerializeField] private int directHitDamage = 25;
        [SerializeField] private bool friendlyFire = false;

        [Header("Splash / AoE")]
        [SerializeField] private bool enableSplash = true;
        [SerializeField] private float splashRadius = 2.5f;
        [SerializeField] private int splashDamage = 10;

        [Header("Impact VFX/SFX")]
        [SerializeField] private GameObject hitVFX;
        [SerializeField] private GameObject missVFX;
        [SerializeField] private AudioClip hitSFX;

        public override bool CanCast(GameObject caster, GameObject target)
        {
            return projectilePrefab != null;
        }

        public override void Cast(GameObject caster, GameObject target)
        {
            if (projectilePrefab == null) return;

            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;

            Vector2 dir = (mouseWorld - caster.transform.position).normalized;

            var casterHealth = caster.GetComponent<IHealth>();
            var casterFaction = casterHealth != null ? casterHealth.GetFaction() : Faction.Neutral;

            int attackPower = directHitDamage;
            var stats = caster.GetComponent<StatsComponent>();
            if (stats != null && stats.CurrentStats != null)
                attackPower = Mathf.Max(0, stats.CurrentStats.Attack);

            var projGO = Instantiate(projectilePrefab, caster.transform.position, Quaternion.identity);

            var projectile = projGO.GetComponent<Projectile>();
            if (projectile == null)
            {
                Debug.LogError($"[MissileAbility] Projectile prefab '{projectilePrefab.name}' is missing Projectile component.");
                Destroy(projGO);
                return;
            }

            projectile.ConfigureImpactEffects(hitVFX, missVFX, hitSFX);
            projectile.Init(dir, casterFaction, friendlyFire, attackPower, projectileSpeed);

            if (enableSplash)
            {
                var splash = projGO.GetComponent<SplashDamage>();
                if (splash == null) splash = projGO.AddComponent<SplashDamage>();

                splash.Setup(caster, splashRadius, casterFaction, friendlyFire, splashDamage, statusEffect, maxStatusStacks, hitVFX, hitSFX);

                projectile.OnImpact += (impactPoint, collider) =>
                {
                    splash.HandleProjectileImpact(impactPoint, collider?.gameObject);
                };
            }
            else if (statusEffect != null)
            {
                var splash = projGO.GetComponent<SplashDamage>();
                if (splash == null) splash = projGO.AddComponent<SplashDamage>();
                splash.Setup(caster, 0f, casterFaction, friendlyFire, 0, statusEffect, maxStatusStacks, hitVFX, hitSFX);

                projectile.OnImpact += (impactPoint, collider) =>
                {
                    splash.HandleProjectileImpact(impactPoint, collider?.gameObject);
                };
            }
        }
    }
}