using UnityEngine;
using Havengard.Abilities; // Projectile (if using ranged)
using Havengard.HealthSystem;
using Havengard.Combat;

namespace Havengard.Units
{
    /// <summary>
    /// Boss enemy with hybrid attack logic.
    /// - If close: melee strike (integer damage into HealthSystem)
    /// - If far: ranged projectile OR ability-based cast
    /// Respects faction rules with friendly-fire toggle.
    /// </summary>
    public class BossEnemy : EnemyUnit
    {
        [Header("Melee")]
        [SerializeField] private int meleeDamage = 20;
        [SerializeField] private float meleeRange = 1.75f;
        [SerializeField] private float meleeCooldown = 1.0f;

        [Header("Ranged (Projectile Mode)")]
        [SerializeField] private bool useProjectileRanged = true;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float projectileSpeed = 12f;
        [SerializeField] private int projectileDamage = 10;
        [SerializeField] private float rangedRange = 6f;
        [SerializeField] private float rangedCooldown = 2.0f;

        [Header("Ranged (Ability Mode)")]
        [SerializeField] private bool useAbilityRanged = false;
        [SerializeField] private int rangedAbilityIndex = 0;

        [Header("Rules")]
        [SerializeField] private bool friendlyFire = false;

        private float lastMeleeTime;
        private float lastRangedTime;

        protected override void PerformAttack(GameObject target)
        {
            if (target == null) return;

            float dist = Vector2.Distance(transform.position, target.transform.position);

            // Try melee if in melee range
            if (dist <= meleeRange && Time.time >= lastMeleeTime + meleeCooldown)
            {
                TryMelee(target);
                return;
            }

            // Otherwise try ranged if in ranged range
            if (dist <= rangedRange && Time.time >= lastRangedTime + rangedCooldown)
            {
                if (useProjectileRanged) TryProjectile(target);
                else if (useAbilityRanged) TryAbilityRanged(target);
            }
        }

        private void TryMelee(GameObject target)
        {
            var th = target.GetComponent<IHealth>();
            if (FactionUtility.CanDamage(GetMyFaction(), th, friendlyFire))
            {
                th.GetHealthSystem().Damage(meleeDamage);
                lastMeleeTime = Time.time;
            }
        }

        private void TryProjectile(GameObject target)
        {
            if (projectilePrefab == null) return;

            Vector3 dir = (target.transform.position - transform.position).normalized;
            GameObject projGO = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

            var proj = projGO.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.sourceFaction = GetMyFaction();
                proj.friendlyFire = friendlyFire;
                proj.damage = projectileDamage;
                proj.speed = projectileSpeed;
            }

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            projGO.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            lastRangedTime = Time.time;
        }

        private void TryAbilityRanged(GameObject target)
        {
            if (abilityUser == null) return;
            abilityUser.UseAbility(rangedAbilityIndex, target);
            lastRangedTime = Time.time;
        }
    }
}
