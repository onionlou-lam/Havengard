using Havengard.Core.Character;
using Havengard.Core.HealthSystem;
using Havengard.Statuses;
using Havengard.Units;
using UnityEngine;

namespace Havengard.Abilities
{
    [CreateAssetMenu(menuName = "Havengard/Abilities/Charged Shot")]
    public class ChargedShotAbility : ChanneledAbilityBase
    {
        [Header("Projectile")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float baseProjectileSpeed = 15f;
        [SerializeField] private float chargedSpeedMultiplier = 2f;
        [SerializeField] private bool friendlyFire = false;

        [Header("Damage")]
        [SerializeField] private int baseDamage = 20;
        [SerializeField] private float chargedDamageMultiplier = 3f; // 3x damage at full charge

        [Header("Size Scaling")]
        [SerializeField] private bool scaleProjectileWithCharge = true;
        [SerializeField] private float minScale = 0.5f;
        [SerializeField] private float maxScale = 2f;

        [Header("Splash (at full charge)")]
        [SerializeField] private bool enableSplashAtFullCharge = true;
        [SerializeField] private float splashRadius = 3f;
        [SerializeField] private int splashDamage = 15;

        [Header("Status Effects")]
        [SerializeField] private StatusEffectData statusEffect;
        [SerializeField] private int maxStatusStacks = 1;

        [Header("Homing")]
        [SerializeField] private bool enableHoming = false;
        [SerializeField] private float homingStrength = 5f;
        [SerializeField] private float homingDelay = 0f;

        // REMOVED: CanCast override - not in base class

        public override void OnRelease(GameObject caster, GameObject target, float chargePercent)
        {
            if (projectilePrefab == null) return;

            // Get shoot direction
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;
            Vector2 direction = (mouseWorld - caster.transform.position).normalized;

            var casterHealth = caster.GetComponent<IHealth>();
            var casterFaction = casterHealth != null ? casterHealth.GetFaction() : Faction.Neutral;

            // Calculate damage based on charge
            float damageMultiplier = Mathf.Lerp(1f, chargedDamageMultiplier, chargePercent);
            int finalDamage = baseDamage;

            var stats = caster.GetComponent<StatsComponent>();
            if (stats != null && stats.CurrentStats != null)
            {
                finalDamage = stats.CurrentStats.Attack;
            }

            finalDamage = Mathf.RoundToInt(finalDamage * damageMultiplier);

            // Calculate projectile speed based on charge
            float speed = Mathf.Lerp(baseProjectileSpeed, baseProjectileSpeed * chargedSpeedMultiplier, chargePercent);

            // Spawn projectile
            var projGO = Instantiate(projectilePrefab, caster.transform.position, Quaternion.identity);

            // Scale projectile if enabled
            if (scaleProjectileWithCharge)
            {
                float scale = Mathf.Lerp(minScale, maxScale, chargePercent);
                projGO.transform.localScale = Vector3.one * scale;
            }

            var projectile = projGO.GetComponent<Projectile>();
            if (projectile == null)
            {
                Debug.LogError($"[ChargedShotAbility] Projectile prefab is missing Projectile component!");
                Destroy(projGO);
                return;
            }

            // Initialize projectile
            projectile.Initialize(direction, casterFaction, friendlyFire, finalDamage, speed, caster);

            // Enable homing if configured
            if (enableHoming)
            {
                projectile.EnableHoming(homingStrength, homingDelay, target);
            }

            // Add splash damage at full charge
            bool isFullCharge = chargePercent >= 0.99f;
            if ((enableSplashAtFullCharge && isFullCharge) || statusEffect != null)
            {
                var splash = projGO.GetComponent<SplashDamage>();
                if (splash == null) splash = projGO.AddComponent<SplashDamage>();

                float radius = (enableSplashAtFullCharge && isFullCharge) ? splashRadius : 0f;
                int damage = (enableSplashAtFullCharge && isFullCharge) ? splashDamage : 0;

                splash.Initialize(
                    caster,
                    radius,
                    casterFaction,
                    friendlyFire,
                    damage,
                    statusEffect,
                    maxStatusStacks
                );

                projectile.OnImpact += (impactPoint, collider) =>
                {
                    splash.HandleProjectileImpact(impactPoint, collider?.gameObject);
                };
            }

            Debug.Log($"Released charged shot at {chargePercent * 100}% charge with {finalDamage} damage");
        }

        public override void OnChannelCancel(GameObject caster)
        {
            base.OnChannelCancel(caster);
            Debug.Log("Charged shot cancelled");
        }

        // ADD: Implement abstract methods from AbilityBase
        public override void Activate(AbilityUser user, Vector3 targetPosition, GameObject targetEnemy)
        {
            Cast(user.gameObject, targetEnemy);
        }

        public override void Deactivate(AbilityUser user)
        {
            // Channeled abilities handle their own cleanup
        }
    }
}