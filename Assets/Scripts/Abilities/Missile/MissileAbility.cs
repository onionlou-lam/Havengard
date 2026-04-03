using Havengard.Combat;
using UnityEngine;

namespace Havengard.Abilities
{
    [CreateAssetMenu(menuName = "Havengard/Abilities/Missile/Projectile Ability")]
    public class MissileAbility : AbilityBase
    {
        [Header("Projectile Settings")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float projectileSpeed = 20f;
        [SerializeField] private float projectileLifetime = 5f;
        [SerializeField] private bool homing = false;
        [SerializeField] private float homingStrength = 5f;

        [Header("Visual Effects")]
        [SerializeField] private float trailTime = 0.5f;
        [SerializeField] private float projectileScale = 1f;

        [Header("Area of Effect")]
        [SerializeField] private bool hasAOE = false;
        [SerializeField] private float aoeRadius = 3f;
        [SerializeField] private float aoeDamageMultiplier = 0.5f;

        [Header("Multishot")]
        [SerializeField] private int projectileCount = 1;
        [SerializeField] private float spreadAngle = 15f;

        public override void Activate(AbilityUser user, Vector3 targetPosition, GameObject targetEnemy)
        {
            if (user == null) return;

            Vector3 direction = targetEnemy != null
                ? (targetEnemy.transform.position - user.transform.position).normalized
                : (targetPosition - user.transform.position).normalized;

            if (castVFX != null)
            {
                GameObject vfx = Instantiate(castVFX, user.transform.position, Quaternion.identity);
                Destroy(vfx, 2f);
            }

            if (castSFX != null)
            {
                AudioSource.PlayClipAtPoint(castSFX, user.transform.position);
            }

            for (int i = 0; i < projectileCount; i++)
            {
                float angle = 0f;
                if (projectileCount > 1)
                {
                    float angleStep = spreadAngle / (projectileCount - 1);
                    angle = -spreadAngle / 2f + (angleStep * i);
                }

                Vector3 spreadDirection = Quaternion.Euler(0, 0, angle) * direction;
                SpawnProjectile(user, user.transform.position, spreadDirection, targetEnemy);
            }
        }

        private void SpawnProjectile(AbilityUser user, Vector3 position, Vector3 direction, GameObject target)
        {
            if (projectilePrefab == null) return;

            GameObject projectileObj = Instantiate(projectilePrefab, position, Quaternion.identity);

            if (projectileScale != 1f)
            {
                projectileObj.transform.localScale = Vector3.one * projectileScale;
            }

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            projectileObj.transform.rotation = Quaternion.Euler(0, 0, angle);

            var projectile = projectileObj.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Initialize(
                    direction,
                    projectileSpeed,
                    projectileLifetime,
                    user.gameObject,
                    (hit) => OnProjectileHit(projectileObj, hit, user)
                );

                Color damageColor = GetDamageTypeColor();
                projectile.ConfigureVisuals(damageColor, trailTime);

                if (homing && target != null)
                {
                    projectile.SetHomingTarget(target.transform, homingStrength);
                }
            }
        }

        protected virtual void OnProjectileHit(GameObject projectile, GameObject target, AbilityUser user)
        {
            if (target == null || user == null) return;

            var health = target.GetComponent<Havengard.Core.HealthSystem.Health>();
            if (health != null)
            {
                float damage = CalculateDamage(user.gameObject);
                health.TakeDamage((int)damage, user.gameObject);
            }

            if (hasAOE)
            {
                ApplyAOEDamage(target.transform.position, user.gameObject);
            }

            if (impactVFX != null)
            {
                GameObject vfx = Instantiate(impactVFX, projectile.transform.position, Quaternion.identity);
                Destroy(vfx, 2f);
            }

            if (impactSFX != null)
            {
                AudioSource.PlayClipAtPoint(impactSFX, projectile.transform.position);
            }

            Destroy(projectile);
        }

        private void ApplyAOEDamage(Vector3 center, GameObject caster)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, aoeRadius, targetLayers);

            foreach (var hit in hits)
            {
                var health = hit.GetComponent<Havengard.Core.HealthSystem.Health>();
                if (health != null)
                {
                    float aoeDamage = CalculateDamage(caster) * aoeDamageMultiplier;
                    health.TakeDamage((int)aoeDamage, caster);
                }
            }
        }

        private Color GetDamageTypeColor()
        {
            return damageType switch
            {
                DamageType.Fire => new Color(1f, 0.3f, 0f),
                DamageType.Frost => new Color(0.3f, 0.8f, 1f),
                DamageType.Lightning => new Color(1f, 1f, 0.3f),
                DamageType.Holy => new Color(1f, 0.9f, 0.3f),
                DamageType.Physical => new Color(0.8f, 0.8f, 0.8f),
                _ => Color.white
            };
        }

        public override void Deactivate(AbilityUser user)
        {
            // Missiles don't need explicit deactivation
        }
    }
}