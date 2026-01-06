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

        [Header("Direct Hit Damage")]
        [SerializeField] private int baseDamage = 10;
        [SerializeField] private bool friendlyFire = false;

        [Header("Explosion")]
        [SerializeField] private bool useExplosion = true;
        [SerializeField] private float explosionRadius = 2.5f;

        [Header("Optional Status Effect (AoE)")]
        [SerializeField] private StatusEffectData statusEffectOnExplosion; // e.g., Burn

        public override bool CanCast(GameObject caster, GameObject target)
        {
            return projectilePrefab != null;
        }

        public override void Cast(GameObject caster, GameObject target)
        {
            if (projectilePrefab == null || caster == null) return;

            // Aim at mouse position (2D)
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;

            Vector2 dir = (mouseWorld - caster.transform.position);
            if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;
            dir.Normalize();

            // Determine faction
            var casterFaction = caster.GetComponent<IHealth>()?.GetFaction() ?? Faction.Neutral;

            // Damage uses stats if present
            int dmg = baseDamage;
            var stats = caster.GetComponent<StatsComponent>();
            if (stats != null)
            {
                // If your Attack is intended to drive missile damage
                dmg = Mathf.Max(1, stats.CurrentStats.Attack);
            }

            // Spawn projectile
            GameObject projGO = Instantiate(projectilePrefab, caster.transform.position, Quaternion.identity);

            var proj = projGO.GetComponent<Projectile>();
            if (proj == null)
            {
                Debug.LogError($"[MissileAbility] Projectile prefab '{projectilePrefab.name}' is missing Projectile component.");
                Destroy(projGO);
                return;
            }

            // Init projectile movement + hit filtering + hit damage
            proj.Init(dir, casterFaction, friendlyFire, dmg, projectileSpeed);

            // Optional explosion handler (called by Projectile on impact)
            if (useExplosion)
            {
                var explosion = projGO.GetComponent<FireballExplosion>();
                if (explosion == null) explosion = projGO.AddComponent<FireballExplosion>();

                // IMPORTANT: include dmg here (this fixes your CS7036 error)
                explosion.Setup(caster, explosionRadius, casterFaction, friendlyFire, dmg, statusEffectOnExplosion);

                // If your Projectile calls Impact(...) we can hook here.
                // If your Projectile already calls FireballExplosion.HandleProjectileImpact, you're good.
                proj.SetImpactListener(explosion.HandleProjectileImpact);
            }
        }
    }
}
