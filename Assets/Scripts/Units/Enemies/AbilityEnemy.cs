using UnityEngine;

namespace Havengard.Enemies
{
    /// <summary>
    /// Enemy that attacks using AbilityUser (slot 0).
    /// </summary>
    public class AbilityEnemy : EnemyBase
    {
        public override void PerformAttack(GameObject target)
        {
            if (abilityUser == null || target == null) return;
            abilityUser.UseAbility(0, target);
        }
    }
}
