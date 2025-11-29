using UnityEngine;
using Havengard.Units;

namespace Havengard.Enemies
{
    /// <summary>
    /// Enemy that attacks using AbilityUser (slot 0).
    /// </summary>
    public class AbilityEnemy : UnitBase
    {
        /// <summary>
        /// Casts the first assigned ability at the current target.
        /// </summary>
        protected override void PerformAttack(GameObject target)
        {
            if (abilityUser == null || target == null) return;
            abilityUser.UseAbility(0, target);
        }
    }
}
