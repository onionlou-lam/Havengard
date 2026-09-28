using Havengard.Core.HealthManagement;
using Havengard.Combat;
using UnityEngine;

namespace Havengard.Abilities
{
    public enum BasicAttackMode
    {
        Melee,
        Ranged
    }

    /// <summary>
    /// Basic attack: always bound to primary mouse button, never consumes resource,
    /// and is the primary generator of Resource (mana) while other abilities are on cooldown.
    /// Inherits the full modifier pipeline from AbilityBase (crit, cooldown, investment,
    /// sub-skills) so passives/items/skill points affect it exactly like any other ability.
    /// Supports both melee (instant hit-detection) and ranged (projectile) hero/unit types.
    /// </summary>
    [CreateAssetMenu(fileName = "New Basic Attack", menuName = "Havengard/Abilities/Basic Attack")]
    public class BasicAttackAbility : AbilityBase
    {
        [Header("Basic Attack Overrides")]
        [Tooltip("Basic attacks never cost resource by default")]
        public bool consumesResource = false;

        [Header("Attack Mode")]
        [SerializeField] private BasicAttackMode attackMode = BasicAttackMode.Melee;

        [Header("Melee Hit Detection")]
        [SerializeField] private float hitRadius = 1.5f;
        [SerializeField] private bool friendlyFire = false;

        [Header("Ranged / Projectile")]
        [Tooltip("Prefab must have a Projectile component (see Havengard.Combat.Projectile)")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float projectileSpeed = 12f;
        [SerializeField] private float projectileLifetime = 5f;
        [SerializeField] private float spawnOffset = 0.5f;
        [SerializeField] private LayerMask wallLayers;
        [SerializeField] private bool isPiercing = false;
        [SerializeField] private int pierceCount = 0;

        [Header("VFX / SFX")]
        [SerializeField] private GameObject hitVFXPrefab;
        [SerializeField] private AudioClip swingSFX;

        public override int GetEffectiveResourceCost()
        {
            return consumesResource ? base.GetEffectiveResourceCost() : 0;
        }

        public override void Activate(AbilityUser user, Vector3 targetPosition, GameObject targetEnemy)
        {
            if (user == null) return;

            if (swingSFX != null)
            {
                PlayAbilitySFX(swingSFX, user.transform.position);
            }

            switch (attackMode)
            {
                case BasicAttackMode.Ranged:
                    ActivateRanged(user, targetPosition, targetEnemy);
                    break;

                case BasicAttackMode.Melee:
                default:
                    ActivateMelee(user, targetEnemy);
                    break;
            }
        }

        public override void Deactivate(AbilityUser user)
        {
            // Basic attacks are instant/non-channeled; nothing to clean up.
        }

        #region Melee

        private void ActivateMelee(AbilityUser user, GameObject targetEnemy)
        {
            GameObject target = targetEnemy;

            // If no explicit target passed in, find nearest valid target in range
            if (target == null)
            {
                Collider2D[] hits = Physics2D.OverlapCircleAll(user.transform.position, hitRadius, targetLayers);
                float closestDist = float.MaxValue;

                foreach (var hit in hits)
                {
                    if (hit.gameObject == user.gameObject) continue;

                    float dist = Vector3.Distance(user.transform.position, hit.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        target = hit.gameObject;
                    }
                }
            }

            if (target == null) return;

            ApplyHitEffects(user.gameObject, target);
        }

        #endregion

        #region Ranged

        private void ActivateRanged(AbilityUser user, Vector3 targetPosition, GameObject targetEnemy)
        {
            if (projectilePrefab == null) return;

            GameObject caster = user.gameObject;

            Vector3 aimPoint = targetEnemy != null
                ? targetEnemy.transform.position
                : targetPosition;

            Vector3 dir3D = (aimPoint - caster.transform.position).normalized;
            if (dir3D.sqrMagnitude < 0.0001f) dir3D = caster.transform.right;

            Vector3 spawnPos = caster.transform.position + dir3D * spawnOffset;

            GameObject projGO = Object.Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            var projectile = projGO.GetComponent<Projectile>();
            if (projectile == null) return;

            // Roll crit once at fire-time so the projectile carries a fixed outcome on impact
            bool isCrit = RollCrit(caster);

            projectile.Initialize(
                dir3D,
                projectileSpeed,
                projectileLifetime,
                caster,
                (hit, shouldDestroy) => OnProjectileHit(hit, caster, isCrit),
                wallLayers,
                isPiercing,
                pierceCount
            );
        }

        private void OnProjectileHit(GameObject target, GameObject caster, bool isCrit)
        {
            if (target == null || caster == null) return;
            ApplyHitEffects(caster, target, isCrit);
        }

        #endregion

        #region Shared Damage Application

        private void ApplyHitEffects(GameObject caster, GameObject target, bool? precomputedCrit = null)
        {
            var casterHealth = caster.GetComponent<IHealth>();
            var targetHealth = target.GetComponent<IHealth>();

            if (casterHealth != null && targetHealth != null)
            {
                bool canDamage = FactionUtility.CanDamage(casterHealth.GetFaction(), targetHealth, friendlyFire);
                if (!canDamage) return;
            }

            var health = target.GetComponent<Health>();
            if (health == null) return;

            bool isCrit = precomputedCrit ?? RollCrit(caster);
            float damage = CalculateDamage(caster, isCrit: isCrit);
            int damageDealt = Mathf.RoundToInt(damage);

            health.TakeDamage(damageDealt, caster);

            if (hitVFXPrefab != null)
            {
                Object.Instantiate(hitVFXPrefab, target.transform.position, Quaternion.identity);
            }

            if (lifestealPercent > 0f)
            {
                LifestealHandler.ApplyLifesteal(caster, damageDealt, lifestealPercent);
            }

            if (enableResourceGeneration)
            {
                int generated = CalculateResourceGeneration(damageDealt);
                if (generated > 0)
                {
                    var resourceSystem = caster.GetComponent<ResourceSystem>();
                    resourceSystem?.AddResource(generated);
                }
            }
        }

        #endregion
    }
}