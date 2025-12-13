using UnityEngine;
using Havengard.Units;
using Havengard.HealthSystem;
using Havengard.Combat;
using Havengard.Character;

namespace Havengard.Abilities
{
    [CreateAssetMenu(menuName = "Havengard/Abilities/Multishot")]
    public class Multishot : AbilityBase
    {
        [Header("Projectile Settings")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float projectileSpeed = 10f;
        [SerializeField] private int baseDamage = 20;
        [SerializeField] private int projectileCount = 3;
        [SerializeField] private float spreadAngle = 30f;
        [SerializeField] private bool friendlyFire = false;

        public override bool CanCast(GameObject caster, GameObject target)
        {
            return projectilePrefab != null;
        }

        public override void Cast(GameObject caster, GameObject target)
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;
            Vector2 baseDir = (mouseWorld - caster.transform.position).normalized;

            var casterFaction = caster.GetComponent<IHealth>()?.GetFaction() ?? Faction.Neutral;

            var statsComp = caster.GetComponent<StatsComponent>();
            int attackPower = statsComp != null && statsComp.CurrentStats != null
                ? statsComp.CurrentStats.Attack
                : baseDamage;

            int half = projectileCount - 1;
            for (int i = 0; i < projectileCount; i++)
            {
                float t = (projectileCount == 1) ? 0f : (i - half) / (float)half;
                float angleOffset = spreadAngle * t;

                Vector2 dir = Quaternion.Euler(0, 0, angleOffset) * baseDir;

                GameObject proj = Object.Instantiate(projectilePrefab, caster.transform.position, Quaternion.identity);
                var projectile = proj.GetComponent<Projectile>();
                if (projectile != null)
                {
                    projectile.Init(dir, casterFaction, friendlyFire, attackPower, projectileSpeed);
                }
            }
        }
    }
}
