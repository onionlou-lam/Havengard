using Havengard.Units;
using Havengard.Core.HealthSystem;

namespace Havengard.Combat
{
    public static class FactionUtility
    {
        /// <summary>
        /// Returns true if the attacker can damage the target, 
        /// considering faction and friendly fire rules.
        /// </summary>
        public static bool CanDamage(Faction sourceFaction, IHealth targetHealth, bool friendlyFire)
        {
            if (targetHealth == null) return false;

            if (!friendlyFire && targetHealth.GetFaction() == sourceFaction)
                return false;

            return true;
        }
    }
}
