using Havengard.HealthSystem;
using Havengard.Units;
using UnityEngine;

namespace Havengard.Abilities
{
    [CreateAssetMenu(menuName = "Havengard/Abilities/Fireball")]
    public class Fireball : AbilityBase
    {
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private int damage = 20;
        [SerializeField] private float projectileSpeed = 12f;
        [SerializeField] private bool friendlyFire = false;

        public override bool CanCast(GameObject caster, GameObject target)
        {
            return projectilePrefab != null && caster != null;
        }

        public override void Cast(GameObject caster, GameObject target)
        {
            if (!CanCast(caster, target)) return;

            Vector3 dir = (target.transform.position - caster.transform.position).normalized;
            GameObject projGO = Instantiate(projectilePrefab, caster.transform.position, Quaternion.identity);

            var proj = projGO.GetComponent<Projectile>();
            if (proj != null)
            {
                var casterFaction = caster.GetComponent<IHealth>()?.GetFaction() ?? Faction.Neutral;
                proj.Init(dir, casterFaction, friendlyFire, damage, projectileSpeed);
            }

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            projGO.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
}
