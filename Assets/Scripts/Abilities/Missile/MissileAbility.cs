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

        [Header("Multishot (Base)")]
        [SerializeField] private int baseProjectileCount = 1;
        [SerializeField] private float baseSpreadAngle = 15f;

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

            // Calculate effective projectile count with sub-skill modifiers
            int effectiveProjectileCount = GetEffectiveProjectileCount();
            float effectiveSpreadAngle = GetEffectiveSpreadAngle();

            Debug.Log($"[MissileAbility] Firing {effectiveProjectileCount} projectiles with {effectiveSpreadAngle}° spread");

            for (int i = 0; i < effectiveProjectileCount; i++)
            {
                float angle = 0f;
                if (effectiveProjectileCount > 1)
                {
                    float angleStep = effectiveSpreadAngle / (effectiveProjectileCount - 1);
                    angle = -effectiveSpreadAngle / 2f + (angleStep * i);
                }

                Vector3 spreadDirection = Quaternion.Euler(0, 0, angle) * direction;
                SpawnProjectile(user, user.transform.position, spreadDirection, targetEnemy);
            }
        }

        /// <summary>
        /// Calculate total projectile count including sub-skill modifiers
        /// </summary>
        private int GetEffectiveProjectileCount()
        {
            int count = baseProjectileCount;

            // Apply sub-skill projectile modifiers
            foreach (var subSkill in activeSubSkills)
            {
                if (subSkill != null && subSkill.addsProjectiles)
                {
                    count += subSkill.additionalProjectiles;
                    Debug.Log($"[MissileAbility] Sub-skill '{subSkill.subSkillName}' adds {subSkill.additionalProjectiles} projectiles");
                }
            }

            return count;
        }

        /// <summary>
        /// Get effective spread angle (use sub-skill value if present, otherwise base)
        /// </summary>
        private float GetEffectiveSpreadAngle()
        {
            float angle = baseSpreadAngle;

            // Use sub-skill spread angle if available
            foreach (var subSkill in activeSubSkills)
            {
                if (subSkill != null && subSkill.addsProjectiles && subSkill.spreadAngle > 0)
                {
                    angle = subSkill.spreadAngle;
                    break; // Use first sub-skill's spread angle
                }
            }

            return angle;
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
                // projectile.ConfigureVisuals(damageColor, trailTime);

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

            // Check if any sub-skill adds explosion
            bool shouldExplode = hasAOE || HasExplosionSubSkill();
            float explosionRadius = GetEffectiveExplosionRadius();
            float explosionDamageMultiplier = GetEffectiveExplosionDamageMultiplier();

            if (shouldExplode)
            {
                ApplyAOEDamage(target.transform.position, user.gameObject, explosionRadius, explosionDamageMultiplier);
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

        /// <summary>
        /// Check if any sub-skill adds explosion effect
        /// </summary>
        private bool HasExplosionSubSkill()
        {
            foreach (var subSkill in activeSubSkills)
            {
                if (subSkill != null && subSkill.addsExplosion)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Get effective explosion radius (use sub-skill if present, otherwise base AOE)
        /// </summary>
        private float GetEffectiveExplosionRadius()
        {
            foreach (var subSkill in activeSubSkills)
            {
                if (subSkill != null && subSkill.addsExplosion)
                    return subSkill.explosionRadius;
            }
            return aoeRadius;
        }

        /// <summary>
        /// Get effective explosion damage multiplier
        /// </summary>
        private float GetEffectiveExplosionDamageMultiplier()
        {
            foreach (var subSkill in activeSubSkills)
            {
                if (subSkill != null && subSkill.addsExplosion)
                    return subSkill.explosionDamageMultiplier;
            }
            return aoeDamageMultiplier;
        }

        private void ApplyAOEDamage(Vector3 center, GameObject caster, float radius, float damageMultiplier)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius, targetLayers);

            Debug.Log($"[MissileAbility] AoE explosion at {center} with radius {radius}, hit {hits.Length} targets");

            foreach (var hit in hits)
            {
                var health = hit.GetComponent<Havengard.Core.HealthSystem.Health>();
                if (health != null)
                {
                    float aoeDamage = CalculateDamage(caster) * damageMultiplier;
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