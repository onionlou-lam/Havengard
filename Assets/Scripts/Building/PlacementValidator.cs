using UnityEngine;
using Havengard.Resources;

namespace Havengard.Building
{
    /// <summary>
    /// Validates tower placement based on grid state, bounds, and resources
    /// </summary>
    public class PlacementValidator
    {
        private BuildGrid grid;

        public PlacementValidator(BuildGrid grid)
        {
            this.grid = grid;
        }

        public PlacementValidationResult ValidatePlacement(TowerBuildData towerData, Vector2Int gridPosition, int level = 0)
        {
            if (towerData == null)
                return PlacementValidationResult.Invalid;

            // Check bounds
            if (!grid.IsFootprintWithinBounds(gridPosition, towerData.gridWidth, towerData.gridHeight))
            {
                Debug.Log("[PlacementValidator] Out of bounds");
                return PlacementValidationResult.OutOfBounds;
            }

            // NEW: Check if on buildable tiles
            if (!grid.IsFootprintBuildable(gridPosition, towerData.gridWidth, towerData.gridHeight))
            {
                Debug.Log("[PlacementValidator] Not on buildable tile");
                return PlacementValidationResult.NotBuildable;
            }

            // Check occupation
            if (!grid.IsFootprintClear(gridPosition, towerData.gridWidth, towerData.gridHeight))
            {
                Debug.Log("[PlacementValidator] Occupied");
                return PlacementValidationResult.Occupied;
            }

            // Check gold
            var levelData = towerData.GetLevelData(level);
            if (levelData == null)
                return PlacementValidationResult.Invalid;

            if (GoldSystem.Instance != null)
            {
                if (GoldSystem.Instance.Current < levelData.buildCost)
                {
                    Debug.Log("[PlacementValidator] Insufficient funds");
                    return PlacementValidationResult.InsufficientFunds;
                }
            }

            return PlacementValidationResult.Valid;
        }
    }

    public enum PlacementValidationResult
    {
        Valid,
        OutOfBounds,
        Occupied,
        InsufficientFunds,
        NotBuildable,
        Invalid
    }
}