using UnityEngine;
using Havengard.Abilities;
using Havengard.Statuses;
using Havengard.Combat;

namespace Havengard.Units
{
    public class BossEnemy : RangedEnemy
    {
        [Header("Boss Settings")]
        [SerializeField] private float bossAttackCooldown = 1.25f;
        [SerializeField] private int bossProjectileDamage = 25;
        [SerializeField] private GameObject bossImpactVFX;
        [SerializeField] private AudioClip bossImpactSFX;

        protected override void Awake()
        {
            base.Awake();
            attackCooldown = bossAttackCooldown;
        }

        public override void PerformAttack(GameObject target)
        {
            if (!IsAttackReady() || target == null) return;
            if (GetComponent<StatusEffectInstance>()?.IsSilenced() == true) return;

            attackEffects?.PlayAttackEffect();

            Vector3 dir = (target.transform.position - transform.position).normalized;
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, dir);

            GameObject proj = Instantiate(projectilePrefab, transform.position, rotation);
            var projectile = proj.GetComponent<Projectile>();

            if (projectile != null)
            {
                projectile.Init(dir, faction, false, bossProjectileDamage, projectileSpeed);
                projectile.ConfigureImpactEffects(bossImpactVFX, null, bossImpactSFX);
            }

            Debug.Log($"🔥 Boss {name} launched projectile at {target.name}");
            ResetAttackCooldown();
        }

        public override void OnDeath()
        {
            Debug.Log($"💀 Boss {name} defeated!");
        }
    }
}
