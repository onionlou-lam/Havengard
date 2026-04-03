using UnityEngine;
using Havengard.Combat;

namespace Havengard.Abilities
{
    [CreateAssetMenu(menuName = "Havengard/Abilities/Channeled/Charged Shot")]
    public class ChargedShotAbility : ChanneledAbilityBase
    {
        [Header("Charged Shot Settings")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float minChargeSpeed = 10f;
        [SerializeField] private float maxChargeSpeed = 30f;
        [SerializeField] private float minChargeDamageMultiplier = 1f;
        [SerializeField] private float maxChargeDamageMultiplier = 3f;

        protected override void OnChannelTick(AbilityUser user, Vector3 targetPosition, GameObject targetEnemy)
        {
            // Visual feedback during charge (optional)
        }

        protected override void OnRelease(GameObject caster, GameObject target, float channelTime)
        {
            float chargePercent = Mathf.Clamp01(channelTime / channelDuration);

            float speed = Mathf.Lerp(minChargeSpeed, maxChargeSpeed, chargePercent);
            float damageMultiplier = Mathf.Lerp(minChargeDamageMultiplier, maxChargeDamageMultiplier, chargePercent);

            Vector3 direction = target != null
                ? (target.transform.position - caster.transform.position).normalized
                : caster.transform.right;

            SpawnProjectile(caster, direction, speed, damageMultiplier);
        }

        private void SpawnProjectile(GameObject caster, Vector3 direction, float speed, float damageMultiplier)
        {
            if (projectilePrefab == null) return;

            GameObject proj = Instantiate(projectilePrefab, caster.transform.position, Quaternion.identity);

            var projectile = proj.GetComponent<Projectile>();
            if (projectile != null)
            {
                float finalDamage = CalculateDamage(caster) * damageMultiplier;

                projectile.Initialize(
                    direction,
                    speed,
                    5f,
                    caster,
                    (hit) => OnProjectileHit(hit, caster, finalDamage)
                );
            }
        }

        private void OnProjectileHit(GameObject target, GameObject caster, float damage)
        {
            var health = target.GetComponent<Havengard.Core.HealthSystem.Health>();
            if (health != null)
            {
                health.TakeDamage((int)damage, caster);
            }
        }

        // Make this public override to match base signature
        public override void OnChannelCancel(GameObject caster)
        {
            base.OnChannelCancel(caster);
            // Released too early - no projectile
        }
    }
}