using UnityEngine;
using System.Collections.Generic;
using Havengard.Units;

namespace Havengard.Building
{
    /// <summary>
    /// Handles the actual placement and tracking of towers
    /// </summary>
    public class TowerPlacementSystem
    {
        private BuildGrid grid;
        private TowerBuildDatabase database;
        private List<GameObject> allPlacedTowers = new List<GameObject>();

        public TowerPlacementSystem(BuildGrid grid, TowerBuildDatabase database)
        {
            this.grid = grid;
            this.database = database;
        }

        /// <summary>
        /// Place a tower on the grid
        /// </summary>
        public GameObject PlaceTower(TowerBuildData towerData, Vector2Int gridPosition, int waveNumber)
        {
            if (towerData == null || towerData.towerPrefab == null)
            {
                Debug.LogError("[TowerPlacement] Invalid tower data or prefab");
                return null;
            }

            // Get level data
            var levelData = towerData.GetLevelData(0);
            if (levelData == null)
            {
                Debug.LogError("[TowerPlacement] No level 0 data");
                return null;
            }

            // Calculate world position (center of footprint)
            Vector3 bottomLeftWorld = grid.GridToWorld(gridPosition);
            float footprintWorldWidth = towerData.gridWidth * grid.CellSize;
            float footprintWorldHeight = towerData.gridHeight * grid.CellSize;

            Vector3 centerOffset = new Vector3(
                footprintWorldWidth * 0.5f - grid.CellSize * 0.5f,
                footprintWorldHeight * 0.5f - grid.CellSize * 0.5f,
                0f
            );

            Vector3 worldPosition = bottomLeftWorld + centerOffset;

            // Instantiate tower
            GameObject towerObject = Object.Instantiate(towerData.towerPrefab, worldPosition, Quaternion.identity);
            towerObject.name = $"{towerData.displayName}_{gridPosition.x}_{gridPosition.y}";

            // Add investment tracker
            var tracker = towerObject.AddComponent<TowerInvestmentTracker>();
            tracker.Initialize(towerData, gridPosition, waveNumber, levelData.buildCost);

            // Apply stats to TowerUnit
            var towerUnit = towerObject.GetComponent<TowerUnit>();
            if (towerUnit != null)
            {
                towerUnit.ApplyLevelStats(
                    levelData.damage,
                    levelData.attackRange,
                    levelData.attackSpeed,
                    levelData.projectileSpeed
                );
            }
            else
            {
                Debug.LogWarning($"[TowerPlacement] Tower prefab missing TowerUnit component");
            }

            // Occupy grid cells
            grid.OccupyCells(gridPosition, towerData.gridWidth, towerData.gridHeight, towerObject);

            // Track tower
            allPlacedTowers.Add(towerObject);

            Debug.Log($"[TowerPlacement] Placed {towerData.displayName} at world pos {worldPosition}, grid pos {gridPosition}");

            return towerObject;
        }

        /// <summary>
        /// Remove a tower from tracking (used when selling/destroying)
        /// </summary>
        public void RemoveTower(GameObject towerObject)
        {
            if (allPlacedTowers.Contains(towerObject))
            {
                allPlacedTowers.Remove(towerObject);
            }
        }

        /// <summary>
        /// Get total investment across all towers
        /// </summary>
        public int GetTotalInvestment()
        {
            int total = 0;
            foreach (var tower in allPlacedTowers)
            {
                if (tower == null) continue;

                var tracker = tower.GetComponent<TowerInvestmentTracker>();
                if (tracker != null)
                    total += tracker.totalInvestment;
            }
            return total;
        }

        /// <summary>
        /// Get all placed towers
        /// </summary>
        public List<GameObject> GetAllTowers()
        {
            // Clean up null references
            allPlacedTowers.RemoveAll(t => t == null);
            return new List<GameObject>(allPlacedTowers);
        }
    }
}