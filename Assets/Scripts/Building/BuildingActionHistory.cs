using UnityEngine;
using System.Collections.Generic;
using Havengard.Resources;

namespace Havengard.Building
{
    /// <summary>
    /// Tracks building actions for undo functionality and current-phase reset
    /// </summary>
    public class BuildingActionHistory
    {
        private BuildingModeController controller;
        private TowerPlacementSystem placementSystem;

        private List<BuildingAction> currentPhaseActions = new List<BuildingAction>();
        private List<BuildingAction> allTimeActions = new List<BuildingAction>();

        public BuildingActionHistory(BuildingModeController controller, TowerPlacementSystem placementSystem)
        {
            this.controller = controller;
            this.placementSystem = placementSystem;
        }

        /// <summary>
        /// Record a tower placement action
        /// </summary>
        public void RecordPlacement(GameObject tower, TowerBuildData towerData, Vector2Int gridPosition, int cost)
        {
            var action = new BuildTowerAction(tower, towerData, gridPosition, cost, controller.Grid, placementSystem);
            currentPhaseActions.Add(action);
            allTimeActions.Add(action);
        }

        /// <summary>
        /// Record a tower upgrade action
        /// </summary>
        public void RecordUpgrade(GameObject tower, int cost)
        {
            var action = new UpgradeTowerAction(tower, cost);
            currentPhaseActions.Add(action);
            allTimeActions.Add(action);
        }

        /// <summary>
        /// Undo the most recent action in current phase
        /// </summary>
        public void Undo()
        {
            if (currentPhaseActions.Count == 0)
                return;

            var lastAction = currentPhaseActions[currentPhaseActions.Count - 1];
            lastAction.Undo();
            currentPhaseActions.RemoveAt(currentPhaseActions.Count - 1);
        }

        /// <summary>
        /// Check if undo is available
        /// </summary>
        public bool CanUndo()
        {
            return currentPhaseActions.Count > 0;
        }

        /// <summary>
        /// Reset all actions from current defence phase
        /// </summary>
        public void ResetCurrentPhase()
        {
            // Undo all actions in reverse order
            for (int i = currentPhaseActions.Count - 1; i >= 0; i--)
            {
                currentPhaseActions[i].Undo();
            }

            currentPhaseActions.Clear();
        }

        /// <summary>
        /// Start a new phase (clears current phase history)
        /// </summary>
        public void StartNewPhase()
        {
            currentPhaseActions.Clear();
        }

        /// <summary>
        /// Get total investment in current phase
        /// </summary>
        public int GetCurrentPhaseInvestment()
        {
            int total = 0;
            foreach (var action in currentPhaseActions)
            {
                total += action.GetCost();
            }
            return total;
        }

        /// <summary>
        /// Get number of towers placed in current phase
        /// </summary>
        public int GetCurrentPhaseTowerCount()
        {
            int count = 0;
            foreach (var action in currentPhaseActions)
            {
                if (action is BuildTowerAction)
                    count++;
            }
            return count;
        }
    }

    #region Building Actions

    /// <summary>
    /// Base class for reversible building actions
    /// </summary>
    public abstract class BuildingAction
    {
        public abstract void Undo();
        public abstract int GetCost();
    }

    /// <summary>
    /// Action for building a tower
    /// </summary>
    public class BuildTowerAction : BuildingAction
    {
        private GameObject tower;
        private TowerBuildData towerData;
        private Vector2Int gridPosition;
        private int cost;
        private BuildGrid grid;
        private TowerPlacementSystem placementSystem;

        public BuildTowerAction(GameObject tower, TowerBuildData towerData, Vector2Int gridPosition, int cost, BuildGrid grid, TowerPlacementSystem placementSystem)
        {
            this.tower = tower;
            this.towerData = towerData;
            this.gridPosition = gridPosition;
            this.cost = cost;
            this.grid = grid;
            this.placementSystem = placementSystem;
        }

        public override void Undo()
        {
            if (tower == null)
                return;

            // Free grid cells
            grid.FreeCells(gridPosition, towerData.gridWidth, towerData.gridHeight);

            // Remove from placement system
            placementSystem.RemoveTower(tower);

            // Refund gold
            if (GoldSystem.Instance != null)
                GoldSystem.Instance.AddGold(cost);

            // Destroy tower
            Object.Destroy(tower);

            Debug.Log($"[Undo] Removed {towerData.displayName}, refunded {cost} Gold");
        }

        public override int GetCost()
        {
            return cost;
        }
    }

    /// <summary>
    /// Action for upgrading a tower
    /// </summary>
    public class UpgradeTowerAction : BuildingAction
    {
        private GameObject tower;
        private int cost;
        private int previousLevel;

        public UpgradeTowerAction(GameObject tower, int cost)
        {
            this.tower = tower;
            this.cost = cost;

            // Store previous level
            var tracker = tower.GetComponent<TowerInvestmentTracker>();
            if (tracker != null)
                previousLevel = tracker.currentLevel - 1; // Already incremented
        }

        public override void Undo()
        {
            if (tower == null)
                return;

            var tracker = tower.GetComponent<TowerInvestmentTracker>();
            if (tracker == null)
                return;

            // Downgrade level
            tracker.currentLevel = previousLevel;
            tracker.totalInvestment -= cost;

            // Refund gold
            if (GoldSystem.Instance != null)
                GoldSystem.Instance.AddGold(cost);

            // Re-apply previous level stats
            // TODO: This would require storing previous stats or re-fetching from TowerBuildData
            // For now, just update the tracker

            Debug.Log($"[Undo] Downgraded tower to level {previousLevel + 1}, refunded {cost} Gold");
        }

        public override int GetCost()
        {
            return cost;
        }
    }

    #endregion
}