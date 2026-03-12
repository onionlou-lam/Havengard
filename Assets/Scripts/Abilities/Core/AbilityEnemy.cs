using UnityEngine;
using Havengard.Abilities; // Add this using directive if AbilityUser is in this namespace

namespace Havengard.Units
{
    /// <summary>
    /// Enemy that attacks only with AbilityUser (slot 0).
    /// </summary>
    public class AbilityEnemy : UnitBase
    {
        // Add this field or property if not present in UnitBase
        protected AbilityUser abilityUser;

        protected override void PerformAttack(GameObject target)
        {
            if (abilityUser == null || target == null) return;
            abilityUser.UseAbility(0, target);
        }
    }
}
