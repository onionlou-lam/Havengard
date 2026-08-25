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
                return PlacementValidationResult.OutOfBounds;

            // Check occupation
            if (!grid.IsFootprintClear(gridPosition, towerData.gridWidth, towerData.gridHeight))
                return PlacementValidationResult.Occupied;

            // Check gold
            var levelData = towerData.GetLevelData(level);
            if (levelData == null)
                return PlacementValidationResult.Invalid;

            if (GoldSystem.Instance != null)
            {
                if (GoldSystem.Instance.Current < levelData.buildCost)
                    return PlacementValidationResult.InsufficientFunds;
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
        Invalid
    }
}