using UnityEngine;
using Havengard.Units;

namespace Havengard.Building
{
    /// <summary>
    /// Extension methods and integration for TowerUnit with building system
    /// This allows tracking damage without modifying TowerUnit directly
    /// </summary>
    public static class TowerUnitBuildingExtension
    {
        /// <summary>
        /// Setup tower stats from build data
        /// </summary>
        public static void ApplyBuildData(this Havengard.Units.TowerUnit tower, TowerBuildData buildData, int level)
        {
            var levelData = buildData.GetLevelData(level);
            if (levelData == null)
            {
                Debug.LogWarning($"[TowerUnit] No level data for level {level}");
                return;
            }

            // Use reflection or direct field access to set TowerUnit stats
            // This depends on TowerUnit's field accessibility
            // For now, we'll log what would be set
            Debug.Log($"[TowerUnit] Would apply: Damage={levelData.damage}, Range={levelData.attackRange}, Speed={levelData.attackSpeed}");

            // NOTE: TowerUnit uses protected/serialized fields, so we need to either:
            // 1. Make TowerUnit fields public/internal
            // 2. Add setter methods to TowerUnit
            // 3. Use reflection (not recommended)
            // 
            // For this implementation, I'll create a method below that should be added to TowerUnit
        }

        /// <summary>
        /// Register damage dealt to investment tracker
        /// </summary>
        public static void RecordDamageDealt(GameObject towerObject, float damage)
        {
            var tracker = towerObject.GetComponent<TowerInvestmentTracker>();
            if (tracker != null)
            {
                tracker.RecordDamage(damage);
            }
        }
    }
}