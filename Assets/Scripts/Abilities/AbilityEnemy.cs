using UnityEngine;

namespace Havengard.Enemies
{
    public class AbilityEnemy : EnemyBase
    {
        private Havengard.Abilities.AbilityUser abilityUser;

        protected override void Awake()
        {
            base.Awake();
            abilityUser = GetComponent<Havengard.Abilities.AbilityUser>();
        }

        public override void PerformAttack(GameObject target)
        {
            if (abilityUser == null || target == null) return;

            // Example: use the first ability
            abilityUser.UseAbility(0, target);
        }
    }
}
