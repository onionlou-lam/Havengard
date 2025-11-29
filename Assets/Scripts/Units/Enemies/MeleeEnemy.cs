using Havengard.Combat;
using Havengard.HealthSystem;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

namespace Havengard.Units
{
    public class MeleeEnemy : UnitBase // if you created EnemyBaseNavMesh; else : UnitBase
    {
        [Header("Melee")]
        [SerializeField] private int meleeDamage = 10;
        [SerializeField] private float attackCooldown = 1.1f;
        private float lastAttackTime;

        protected override void PerformAttack(GameObject target)
        {
            if (Time.time < lastAttackTime + attackCooldown || target == null) return;

            var h = target.GetComponent<IHealth>();
            if (h != null && FactionUtility.CanDamage(GetMyFaction(), h, false))
            {
                h.GetHealthSystem().Damage(meleeDamage);
                lastAttackTime = Time.time;
            }
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

            // (If using EnemyBaseNavMesh, it can also fall back to Gate target)
            return closest;
        }
    }
}
