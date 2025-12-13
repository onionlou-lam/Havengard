using Havengard.Combat;
using Havengard.HealthSystem;
using UnityEngine;

namespace Havengard.Units
{
    public class MeleeEnemy : UnitBase
    {
        [Header("Melee")]
        [SerializeField] private int meleeDamage = 10;
        [SerializeField] private float attackCooldown = 1.1f;
        private float lastAttackTime;

        protected override void PerformAttack(GameObject target)
        {
            if (target == null) return;
            if (Time.time < lastAttackTime + attackCooldown) return;

            var h = target.GetComponent<IHealth>();
            if (h != null && FactionUtility.CanDamage(GetMyFaction(), h, false))
            {
                h.GetHealthSystem().Damage(meleeDamage);
                Debug.Log($"[MeleeEnemy] {name} attacks {target.name}, dealing {meleeDamage} damage.");
                lastAttackTime = Time.time;
            }
        }

        // No FindTarget override – we use the base UnitBase.FindTarget (Physics2D.OverlapCircleAll)
    }
}
