using UnityEngine;
using Havengard.Core.HealthSystem;
using Havengard.Statuses;
using Havengard.Core.Character;

namespace Havengard.Abilities
{
    /// <summary>
    /// Helper to calculate and apply lifesteal from all sources
    /// (status effects, stats, abilities, etc.)
    /// </summary>
    public static class LifestealHandler
    {
        /// <summary>
        /// Calculates total lifesteal percentage from all active status effects
        /// </summary>
        public static float GetTotalLifesteal(GameObject unit)
        {
            float total = 0f;

            // Get lifesteal from all active status effects
            var statusEffects = unit.GetComponents<StatusEffectInstance>();
            if (statusEffects != null)
            {
                foreach (var effect in statusEffects)
                {
                    total += effect.GetLifestealPercent();
                }
            }

            // Optional: Add lifesteal from Stats if you add it there later
            // var stats = unit.GetComponent<StatsComponent>();
            // if (stats != null && stats.CurrentStats != null)
            // {
            //     total += stats.CurrentStats.LifestealPercent;
            // }

            return Mathf.Clamp01(total); // Cap at 100%
        }

        /// <summary>
        /// Applies lifesteal healing to the attacker based on damage dealt
        /// </summary>
        public static void ApplyLifesteal(GameObject attacker, int damageDealt, float additionalLifesteal = 0f)
        {
            if (damageDealt <= 0) return;

            float totalLifesteal = GetTotalLifesteal(attacker) + additionalLifesteal;
            if (totalLifesteal <= 0f) return;

            int healAmount = Mathf.RoundToInt(damageDealt * totalLifesteal);
            if (healAmount <= 0) return;

            var health = attacker.GetComponent<IHealth>();
            if (health != null)
            {
                health.GetHealthSystem().Heal(healAmount);
                Debug.Log($"[Lifesteal] {attacker.name} healed {healAmount} HP ({totalLifesteal * 100f}% of {damageDealt} damage)");
            }
        }
    }
}