using UnityEngine;
using Havengard.HealthSystem;

namespace Havengard.Abilities
{
    [CreateAssetMenu(menuName = "Havengard/Abilities/Fireball")]
    public class FireballAbility : AbilityBase
    {
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float damage = 40f;
        [SerializeField] private float splashRadius = 2f;
        [SerializeField] private float dotDuration = 3f;
        [SerializeField] private float dotPercent = 0.25f;
        [SerializeField] private float projectileSpeed = 10f;

        protected override void Execute(GameObject caster, GameObject target)
        {
            if (projectilePrefab == null) return;

            // Left click → enemy target lock
            if (target != null && target.TryGetComponent<IHealth>(out var _))
            {
                LaunchProjectile(caster, target.transform);
            }
            // Shift+click → ground cast
            else if (Input.GetKey(KeyCode.LeftShift))
            {
                Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mouseWorld.z = 0;
                LaunchProjectile(caster, null, mouseWorld);
            }
        }

        private void LaunchProjectile(GameObject caster, Transform target, Vector3? overridePos = null)
        {
            var projObj = Object.Instantiate(projectilePrefab, caster.transform.position, Quaternion.identity);
            var proj = projObj.GetComponent<FireballProjectile>();
            proj.Init(target, damage, splashRadius, dotDuration, dotPercent, projectileSpeed, caster.GetComponent<FactionProvider>().Faction, overridePos);
        }
    }
}
