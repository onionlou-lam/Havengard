using UnityEngine;
using Havengard.Core.Character;

namespace Havengard.Combat
{
    public static class CombatCalculator
    {
        private const float DAMAGE_VARIANCE = 0.1f; // ±10%

        public static int CalculateDamage(GameObject attacker, GameObject defender)
        {
            var atkStats = attacker.GetComponent<StatsComponent>()?.CurrentStats;
            var defStats = defender.GetComponent<StatsComponent>()?.CurrentStats;

            int attackValue = atkStats?.Attack ?? 1; //CHANGE THIS AFTER TESTING
            int defenseValue = defStats?.Defense ?? 0;

            // Crit stats
            float critChance = atkStats?.CritChance ?? 0.1f;       // fallback 10%
            float critMult = atkStats?.CritMultiplier ?? 2f;       // fallback 2x

            // Base damage
            float rawDamage = Mathf.Max(1, attackValue - defenseValue);

            // Apply variance
            float variance = Random.Range(1f - DAMAGE_VARIANCE, 1f + DAMAGE_VARIANCE);
            rawDamage *= variance;

            // Apply crit
            bool isCrit = Random.value < critChance;
            if (isCrit)
            {
                rawDamage *= critMult;
                Debug.Log($"CRITICAL HIT! {attacker.name} dealt {Mathf.RoundToInt(rawDamage)} damage to {defender.name}");
            }

            return Mathf.RoundToInt(rawDamage);
        }
    }
}
