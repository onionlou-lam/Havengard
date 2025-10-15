using UnityEngine;
using Havengard.Units;
using Havengard.Character;
using Havengard.HealthSystem;
using Havengard.Combat;
using Havengard.Utility;

namespace Havengard.Abilities
{
    [CreateAssetMenu(menuName = "Havengard/Abilities/Fireball")]
    public class Fireball : AbilityBase
    {
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private GameObject impactEffectPrefab;
        [SerializeField] private AudioClip impactSound;
        [SerializeField, Range(0f, 1f)] private float impactSoundVolume = 0.8f;
        [SerializeField] private float projectileSpeed = 12f;
        [SerializeField] private bool friendlyFire = false;

        private ObjectPool projectilePool;
        private ObjectPool impactPool;

        public override bool CanCast(GameObject caster, GameObject target)
        {
            return projectilePrefab != null && caster != null;
        }

        public override void Cast(GameObject caster, GameObject target)
        {
            if (!CanCast(caster, target)) return;

            // Ensure pools exist
            if (projectilePool == null)
            {
                var poolObj = new GameObject($"{projectilePrefab.name}_Pool");
                projectilePool = poolObj.AddComponent<ObjectPool>();
                projectilePool.GetType().GetField("prefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(projectilePool, projectilePrefab);
            }

            if (impactPool == null)
            {
                var vfxPoolObj = new GameObject($"{impactEffectPrefab.name}_Pool");
                impactPool = vfxPoolObj.AddComponent<ObjectPool>();
                impactPool.GetType().GetField("prefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(impactPool, impactEffectPrefab);
            }

            int attackValue = caster.GetComponent<StatsComponent>()?.CurrentStats.Attack ?? 15;
            Vector3 dir = (target.transform.position - caster.transform.position).normalized;

            GameObject projGO = projectilePool.Get(caster.transform.position, Quaternion.identity);
            var proj = projGO.GetComponent<Projectile>();

            if (proj != null)
            {
                var casterFaction = caster.GetComponent<IHealth>()?.GetFaction() ?? Faction.Neutral;
                proj.Init(dir, casterFaction, friendlyFire, attackValue, projectileSpeed, impactEffectPrefab, impactSound, impactSoundVolume, projectilePool, impactPool);

                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                projGO.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
        }
    }
}
