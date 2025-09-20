using UnityEngine;

namespace Havengard.Units
{
    /// <summary>
    /// Enemy that attacks only with AbilityUser (slot 0).
    /// </summary>
    public class AbilityEnemy : EnemyUnit
    {
        protected override void PerformAttack(GameObject target)
        {
            if (abilityUser == null || target == null) return;
            abilityUser.UseAbility(0, target);
        }
    }
}
