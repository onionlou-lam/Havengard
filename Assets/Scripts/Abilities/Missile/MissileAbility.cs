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

        [Header("Wall Collision")]
        [SerializeField] private AudioClip wallHitSFX;
        [SerializeField] private LayerMask wallLayers;

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

        /// <summary>
        /// Check if any sub-skill enables piercing
        /// </summary>
        private bool HasPiercingSubSkill(out int pierceCount, out float damageReduction)
        {
            pierceCount = 0;
            damageReduction = 0f;

            foreach (var subSkill in activeSubSkills)
            {
                if (subSkill != null && subSkill.enablesPiercing)
                {
                    pierceCount = subSkill.pierceCount;
                    damageReduction = subSkill.pierceDamageReduction;
                    return true;
                }
            }

            return false;
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
                // Check for piercing
                bool isPiercing = HasPiercingSubSkill(out int pierceCount, out float damageReduction);

                projectile.Initialize(
                    direction,
                    projectileSpeed,
                    projectileLifetime,
                    user.gameObject,
                    (hit, shouldDestroy) => OnProjectileHit(projectileObj, hit, user, shouldDestroy),
                    wallLayers,
                    isPiercing,
                    pierceCount
                );

                Color damageColor = GetDamageTypeColor();
                // projectile.ConfigureVisuals(damageColor, trailTime);

                if (homing && target != null)
                {
                    projectile.SetHomingTarget(target.transform, homingStrength);
                }
            }
        }

        protected virtual void OnProjectileHit(GameObject projectile, GameObject target, AbilityUser user, bool shouldDestroy)
        {
            if (target == null || user == null) return;

            // Check if hit a wall
            bool hitWall = IsWall(target);

            if (hitWall)
            {
                // Play wall hit sound
                if (wallHitSFX != null)
                {
                    AudioSource.PlayClipAtPoint(wallHitSFX, projectile.transform.position);
                }

                // Optional: Spawn impact VFX for walls too
                if (impactVFX != null)
                {
                    GameObject vfx = Instantiate(impactVFX, projectile.transform.position, Quaternion.identity);
                    Destroy(vfx, 2f);
                }

                // Walls always destroy projectiles (even piercing ones)
                Destroy(projectile);
                return;
            }

            // Get projectile component to check pierce count
            var projectileComponent = projectile.GetComponent<Projectile>();
            int enemiesHit = projectileComponent != null ? projectileComponent.GetEnemiesHit() : 0;

            // Calculate damage with pierce reduction
            float damage = CalculateDamage(user.gameObject);
            
            // Apply pierce damage reduction if applicable
            if (HasPiercingSubSkill(out int pierceCount, out float damageReduction) && enemiesHit > 0)
            {
                // Reduce damage based on number of enemies already hit
                float reductionMultiplier = Mathf.Pow(1f - damageReduction, enemiesHit);
                damage *= reductionMultiplier;
                Debug.Log($"[MissileAbility] Pierce damage reduced to {damage} ({enemiesHit} enemies hit, {damageReduction * 100}% reduction per pierce)");
            }

            // Hit a valid target (not a wall)
            var health = target.GetComponent<Havengard.Core.HealthSystem.Health>();
            if (health != null)
            {
                int damageDealt = (int)damage;
                health.TakeDamage(damageDealt, user.gameObject);

                // Apply lifesteal if configured
                if (lifestealPercent > 0f)
                {
                    LifestealHandler.ApplyLifesteal(user.gameObject, damageDealt, lifestealPercent);
                }

                // Apply resource generation if enabled
                if (enableResourceGeneration)
                {
                    ResourceGenerationHandler.ApplyResourceGeneration(
                        user.gameObject,
                        damageDealt,
                        resourceGenerationPercent,
                        flatResourceGeneration);
                }
            }

            // Check if any sub-skill adds explosion
            bool shouldExplode = hasAOE || HasExplosionSubSkill();
            float explosionRadius = GetEffectiveExplosionRadius();
            float explosionDamageMultiplier = GetEffectiveExplosionDamageMultiplier();

            if (shouldExplode)
            {
                ApplyAOEDamage(target.transform.position, user.gameObject, explosionRadius, explosionDamageMultiplier);
            }

            // Only spawn VFX/SFX on each hit if not piercing, or on final hit
            bool shouldPlayEffects = !HasPiercingSubSkill(out _, out _) || shouldDestroy;

            if (shouldPlayEffects)
            {
                if (impactVFX != null)
                {
                    GameObject vfx = Instantiate(impactVFX, projectile.transform.position, Quaternion.identity);
                    Destroy(vfx, 2f);
                }

                if (impactSFX != null)
                {
                    AudioSource.PlayClipAtPoint(impactSFX, projectile.transform.position);
                }
            }

            // Destroy projectile if told to (reached pierce limit or not piercing)
            if (shouldDestroy)
            {
                Destroy(projectile);
            }
        }

        /// <summary>
        /// Check if the hit object is a wall based on layer
        /// </summary>
        private bool IsWall(GameObject obj)
        {
            if (wallLayers == 0) return false; // No wall layers defined
            return ((1 << obj.layer) & wallLayers) != 0;
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
                    int damageDealt = (int)aoeDamage;
                    health.TakeDamage(damageDealt, caster);

                    // Apply lifesteal for AOE damage
                    if (lifestealPercent > 0f)
                    {
                        LifestealHandler.ApplyLifesteal(caster, damageDealt, lifestealPercent);
                    }

                    // Apply resource generation for AOE damage
                    if (enableResourceGeneration)
                    {
                        ResourceGenerationHandler.ApplyResourceGeneration(
                            caster,
                            damageDealt,
                            resourceGenerationPercent,
                            flatResourceGeneration);
                    }
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