using Havengard.Abilities;
using Havengard.Combat;
using Havengard.HealthSystem;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

namespace Havengard.Units
{
    public class RangedEnemy : UnitBase // or : UnitBase
    {
        [Header("Ranged")]
        [SerializeField] protected GameObject projectilePrefab;
        [SerializeField] protected float projectileSpeed = 10f;
        [SerializeField] protected int projectileDamage = 8;
        [SerializeField] protected float attackCooldown = 1.25f;
        [SerializeField] protected bool friendlyFire = false;

        protected float lastAttackTime;

        protected override void PerformAttack(GameObject target)
        {
            if (Time.time < lastAttackTime + attackCooldown || target == null) return;

            var th = target.GetComponent<IHealth>();
            if (th == null || !FactionUtility.CanDamage(GetMyFaction(), th, friendlyFire)) return;

            Vector3 dir = (target.transform.position - transform.position).normalized;
            Quaternion rot = Quaternion.LookRotation(Vector3.forward, dir);

            var projGO = Instantiate(projectilePrefab, transform.position, rot);
            if (projGO.TryGetComponent<Projectile>(out var proj))
            {
                proj.Init(dir, GetMyFaction(), friendlyFire, projectileDamage, projectileSpeed);
            }

            lastAttackTime = Time.time;
        }

        protected override GameObject FindTarget()
        {
            GameObject closest = null;
            float closestDist = Mathf.Infinity;
            var myFaction = GetMyFaction();

            foreach (var enemy in UnitTargetManager.GetEnemiesOf(myFaction))
            {
                var obj = (enemy as MonoBehaviour).gameObject;
                float d = Vector2.Distance(transform.position, obj.transform.position);
                if (d < closestDist && d <= aggroRange)
                {
                    closestDist = d;
                    closest = obj;
                }
            }

            return closest;
        }
    }
}
