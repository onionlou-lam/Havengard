using UnityEngine;
using Havengard.Abilities;

namespace Havengard.Units
{
    public class MeleeEnemy : UnitBase
    {
        [Header("Melee")]
        [SerializeField] private AbilityBase attackAbility; // assign Cleave here
        private AbilityUser abilityUser;
        private float lastAttackTime;

        protected override void Awake()
        {
            base.Awake();
            abilityUser = GetComponent<AbilityUser>();
        }

        protected override void PerformAttack(GameObject target)
        {
            if (abilityUser == null || attackAbility == null || target == null) return;

            // Cooldown gate
            if (Time.time < lastAttackTime + attackAbility.baseCooldown) return;

            TriggerAttackAnim();
            
            // Use AbilityUser to activate the ability
            Vector3 targetPos = target.transform.position;
            attackAbility.Activate(abilityUser, targetPos, target);

            lastAttackTime = Time.time;
        }
    }
}
