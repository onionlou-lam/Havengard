using UnityEngine;
using Havengard.Units;
using Havengard.Character;
using Havengard.HealthSystem;
using Havengard.Combat;

namespace Havengard.Abilities
{
    [CreateAssetMenu(menuName = "Havengard/Abilities/Fireball")]
    public class Fireball : AbilityBase
    {
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float projectileSpeed = 12f;
        [SerializeField] private bool friendlyFire = false;

        public override bool CanCast(GameObject caster, GameObject target)
        {
            return projectilePrefab != null && caster != null;
        }

        public override void Cast(GameObject caster, GameObject target)
        {
            if (!CanCast(caster, target)) return;

            // Calculate caster’s attack-based damage now, defense is applied later on hit
            int attackValue = caster.GetComponent<StatsComponent>()?.CurrentStats.Attack ?? 15;

            Vector3 dir = (target.transform.position - caster.transform.position).normalized;
            GameObject projGO = Instantiate(projectilePrefab, caster.transform.position, Quaternion.identity);

            var proj = projGO.GetComponent<Projectile>();
            if (proj != null)
            {
                var casterFaction = caster.GetComponent<IHealth>()?.GetFaction() ?? Faction.Neutral;
                proj.Init(dir, casterFaction, friendlyFire, attackValue, projectileSpeed);
            }

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            projGO.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
}
