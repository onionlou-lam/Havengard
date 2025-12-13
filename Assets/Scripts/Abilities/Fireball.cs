using UnityEngine;
using Havengard.Units;
using Havengard.HealthSystem;
using Havengard.Combat;
using Havengard.Character;

namespace Havengard.Abilities
{
    [CreateAssetMenu(menuName = "Havengard/Abilities/Fireball")]
    public class Fireball : AbilityBase
    {
        [Header("Projectile Settings")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float projectileSpeed = 10f;
        [SerializeField] private int baseDamage = 25;
        [SerializeField] private float explosionRadius = 2.5f;
        [SerializeField] private GameObject explosionVFX;
        [SerializeField] private AudioClip explosionSFX;
        [SerializeField] private bool friendlyFire = false;

        public override bool CanCast(GameObject caster, GameObject target)
        {
            return projectilePrefab != null;
        }

        public override void Cast(GameObject caster, GameObject target)
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;

            Vector2 direction = (mouseWorld - caster.transform.position).normalized;

            var casterFaction = caster.GetComponent<IHealth>()?.GetFaction() ?? Faction.Neutral;

            // Get attack power from stats if present, otherwise fall back to baseDamage
            var statsComp = caster.GetComponent<StatsComponent>();
            int attackPower = statsComp != null && statsComp.CurrentStats != null
                ? statsComp.CurrentStats.Attack
                : baseDamage;

            GameObject proj = Instantiate(projectilePrefab, caster.transform.position, Quaternion.identity);

            var projectile = proj.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Init(direction, casterFaction, friendlyFire, attackPower, projectileSpeed);
                projectile.ConfigureImpactEffects(explosionVFX, null, explosionSFX);

                var explosion = proj.AddComponent<FireballExplosion>();
                explosion.Setup(explosionRadius, casterFaction, friendlyFire, attackPower);
            }
        }
    }
}
