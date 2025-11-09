using UnityEngine;
using Havengard.HealthSystem;
using Havengard.Abilities;
using Havengard.Combat;

namespace Havengard.Units
{
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(AbilityUser))]
    public class AllyHero : UnitBase
    {
        [Header("Ally Hero")]
        [SerializeField] private float attackCooldown = 1f;
        private float lastAttackTime;

        protected override void PerformAttack(GameObject target)
        {
            if (Time.time < lastAttackTime + attackCooldown || target == null) return;

            var h = target.GetComponent<IHealth>();
            if (h != null && FactionUtility.CanDamage(GetMyFaction(), h, false))
            {
                // Use first equipped ability
                abilityUser?.UseAbility(0, target);
                lastAttackTime = Time.time;
            }
        }

        // Optional: if you want custom target priority (closest enemy), override:
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

            return closest; // base UnitBase handles moving/attacking once we return this
        }
    }
}
