using UnityEngine;
using Havengard.Units;
using Havengard.HealthSystem;
using Havengard.Character;
using Havengard.Statuses;

namespace Havengard.Abilities
{
    [CreateAssetMenu(menuName = "Havengard/Abilities/Fireball")]
    public class Fireball : AbilityBase
    {
        [Header("Projectile Settings")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float projectileSpeed = 10f;

        [Tooltip("Fallback damage if no StatsComponent/Attack is available.")]
        [SerializeField] private int baseDamage = 25;

        [Tooltip("If true, uses caster Attack for projectile damage; otherwise uses baseDamage.")]
        [SerializeField] private bool useCasterAttackForProjectile = true;

        [Header("Explosion Settings")]
        [SerializeField] private float explosionRadius = 2.5f;

        [Tooltip("AoE damage is based on (projectileDamage * aoeDamageMultiplier) + aoeDamageBonus.")]
        [SerializeField] private float aoeDamageMultiplier = 1.0f;

        [Tooltip("Flat bonus added after multiplier.")]
        [SerializeField] private int aoeDamageBonus = 0;

        [SerializeField] private GameObject explosionVFX;
        [SerializeField] private AudioClip explosionSFX;

        [Header("Burn (DoT)")]
        [Tooltip("Optional. Applies this StatusEffectData to all valid targets in the explosion radius.")]
        [SerializeField] private StatusEffectData burnEffect;

        [Header("Targeting")]
        [SerializeField] private bool friendlyFire = false;

        public override bool CanCast(GameObject caster, GameObject target)
        {
            return projectilePrefab != null;
        }

        public override void Cast(GameObject caster, GameObject target)
        {
            if (projectilePrefab == null) return;

            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;

            Vector2 direction = (mouseWorld - caster.transform.position).normalized;
            if (direction.sqrMagnitude < 0.0001f) direction = Vector2.right;

            var casterFaction = caster.GetComponent<IHealth>()?.GetFaction() ?? Faction.Neutral;

            // --- Determine projectile damage ---
            int projectileDamage = baseDamage;

            if (useCasterAttackForProjectile)
            {
                var statsComp = caster.GetComponent<StatsComponent>();
                if (statsComp != null && statsComp.CurrentStats != null)
                {
                    int atk = statsComp.CurrentStats.Attack;
                    if (atk > 0) projectileDamage = atk;
                }
            }

            // --- Determine AoE damage (separate adjustable) ---
            int aoeDamage = Mathf.RoundToInt(projectileDamage * aoeDamageMultiplier) + aoeDamageBonus;
            if (aoeDamage < 0) aoeDamage = 0;

            // Spawn projectile
            GameObject proj = Instantiate(projectilePrefab, caster.transform.position, Quaternion.identity);

            var projectile = proj.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Init(direction, casterFaction, friendlyFire, projectileDamage, projectileSpeed);
                projectile.ConfigureImpactEffects(explosionVFX, null, explosionSFX);

                // Explosion logic lives on projectile object so it triggers when projectile is destroyed
                var explosion = proj.AddComponent<FireballExplosion>();
                explosion.Setup(explosionRadius, casterFaction, friendlyFire, aoeDamage, burnEffect);
            }
        }
    }
}
