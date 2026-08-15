using UnityEngine;
using Havengard.Statuses;

namespace Havengard.Abilities
{
    /// <summary>
    /// Helper to calculate and apply resource generation from all sources
    /// (status effects, stats, abilities, etc.)
    /// </summary>
    public static class ResourceGenerationHandler
    {
        /// <summary>
        /// Calculates total resource generation percentage from all active status effects
        /// </summary>
        public static float GetTotalResourceGenerationPercent(GameObject unit)
        {
            float total = 0f;

            // Get resource generation from all active status effects
            var statusEffects = unit.GetComponents<StatusEffectInstance>();
            if (statusEffects != null)
            {
                foreach (var effect in statusEffects)
                {
                    if (effect.Data != null)
                    {
                        // Assuming StatusEffectData might have a resourceGenerationPercent field
                        // If not, this can be removed or added later
                        // total += effect.Data.resourceGenerationPercent;
                    }
                }
            }

            // Optional: Add resource generation from Stats if you add it there later
            // var stats = unit.GetComponent<StatsComponent>();
            // if (stats != null && stats.CurrentStats != null)
            // {
            //     total += stats.CurrentStats.ResourceGenerationPercent;
            // }

            return Mathf.Max(0f, total);
        }

        /// <summary>
        /// Applies resource generation to the attacker based on damage dealt
        /// </summary>
        public static void ApplyResourceGeneration(GameObject attacker, int damageDealt, float additionalPercent = 0f, int additionalFlat = 0)
        {
            if (damageDealt <= 0) return;

            float totalPercent = GetTotalResourceGenerationPercent(attacker) + additionalPercent;
            int resourceAmount = Mathf.RoundToInt(damageDealt * totalPercent) + additionalFlat;

            if (resourceAmount <= 0) return;

            var resourceSystem = attacker.GetComponent<ResourceSystem>();
            if (resourceSystem != null)
            {
                resourceSystem.AddResource(resourceAmount);
                Debug.Log($"[ResourceGen] {attacker.name} gained {resourceAmount} resource ({totalPercent * 100f}% of {damageDealt} damage + {additionalFlat} flat)");
            }
        }

        /// <summary>
        /// Applies flat resource generation regardless of damage
        /// </summary>
        public static void ApplyFlatResourceGeneration(GameObject unit, int amount)
        {
            if (amount <= 0) return;

            var resourceSystem = unit.GetComponent<ResourceSystem>();
            if (resourceSystem != null)
            {
                resourceSystem.AddResource(amount);
                Debug.Log($"[ResourceGen] {unit.name} gained {amount} flat resource");
            }
        }
    }
}