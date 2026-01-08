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

        [Header("Status Effect (e.g. Burn)")]
        [SerializeField] private StatusEffectData statusEffect;

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

            Vector2 dir = (mouseWorld - caster.transform.position);
            if (dir.sqrMagnitude < 0.001f) dir = Vector2.right;
            dir.Normalize();

            var casterHealth = caster.GetComponent<IHealth>();
            var casterFaction = casterHealth != null ? casterHealth.GetFaction() : Faction.Neutral;

            // Use stats if present, else fall back
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
                // Ensure SplashDamage exists
                var splash = projGO.GetComponent<SplashDamage>();
                if (splash == null) splash = projGO.AddComponent<SplashDamage>();

                splash.Setup(splashRadius, casterFaction, friendlyFire, splashDamage, statusEffect);

                // Wire impact event
                projectile.OnImpact += splash.HandleProjectileImpact;
            }
            else if (statusEffect != null)
            {
                // If no splash, but we still want a status: apply it to direct target on impact
                var splash = projGO.GetComponent<SplashDamage>();
                if (splash == null) splash = projGO.AddComponent<SplashDamage>();
                splash.Setup(0f, casterFaction, friendlyFire, 0, statusEffect);
                projectile.OnImpact += splash.HandleProjectileImpact;
            }
        }
    }
}
