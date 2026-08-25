using UnityEngine;
using Havengard.Core.HealthSystem;
using Havengard.Units;

namespace Havengard.Waves
{
    /// <summary>
    /// Applies difficulty scaling to spawned enemies
    /// </summary>
    public static class WaveScaler
    {
        /// <summary>
        /// Apply wave set scaling to an enemy
        /// </summary>
        public static void ApplyScaling(GameObject enemy, WaveSet waveSet, int waveIndex)
        {
            if (enemy == null || waveSet == null) return;

            // Health scaling
            Health health = enemy.GetComponent<Health>();
            if (health != null)
            {
                float healthMultiplier = waveSet.GetHealthMultiplier(waveIndex);
                
                // Get the current max health and scale it
                int currentMaxHealth = health.MaxHealth;
                int scaledMaxHealth = Mathf.RoundToInt(currentMaxHealth * healthMultiplier);
                
                // Use HealthSystem to set the new max health
                HealthSystem healthSystem = health.GetHealthSystem();
                if (healthSystem != null)
                {
                    healthSystem.SetMaxHealth(scaledMaxHealth, true); // true = refill to new max
                    Debug.Log($"[WaveScaler] Scaled {enemy.name} health: {scaledMaxHealth} (x{healthMultiplier:F2})");
                }
            }

            // Speed scaling
            UnitBase unit = enemy.GetComponent<UnitBase>();
            if (unit != null && unit.agent != null)
            {
                float speedMultiplier = waveSet.GetSpeedMultiplier(waveIndex);
                unit.agent.speed *= speedMultiplier;
                Debug.Log($"[WaveScaler] Scaled {enemy.name} speed: {unit.agent.speed} (x{speedMultiplier:F2})");
            }

            // Damage scaling (if you have a damage stats component)
            // Example: If you add a DamageStats component to enemies in the future
            // DamageStats damageStats = enemy.GetComponent<DamageStats>();
            // if (damageStats != null)
            // {
            //     float damageMultiplier = waveSet.GetDamageMultiplier(waveIndex);
            //     damageStats.baseDamage = Mathf.RoundToInt(damageStats.baseDamage * damageMultiplier);
            // }
        }
    }
}