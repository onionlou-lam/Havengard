using UnityEngine;
using System.Collections.Generic;

namespace Havengard.Building
{
    /// <summary>
    /// Logical grid representing the buildable area.
    /// Single source of truth for grid bounds and cell occupation.
    /// </summary>
    public class BuildGrid : MonoBehaviour
    {
        [Header("Grid Configuration")]
        [SerializeField] private Vector2 origin = Vector2.zero;
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private int gridWidth = 50;
        [SerializeField] private int gridHeight = 50;

        private Dictionary<Vector2Int, GridCell> cells = new Dictionary<Vector2Int, GridCell>();

        public Vector2 Origin => origin;
        public float CellSize => cellSize;
        public int GridWidth => gridWidth;
        public int GridHeight => gridHeight;

        public Bounds GetWorldBounds()
        {
            Vector3 center = new Vector3(
                origin.x + (gridWidth * cellSize) * 0.5f,
                origin.y + (gridHeight * cellSize) * 0.5f,
                0f
            );

            Vector3 size = new Vector3(
                gridWidth * cellSize,
                gridHeight * cellSize,
                10f // Arbitrary depth for camera bounds
            );

            return new Bounds(center, size);
        }

        /// <summary>
        /// Convert world position to grid coordinates
        /// </summary>
        public Vector2Int WorldToGrid(Vector3 worldPosition)
        {
            float localX = worldPosition.x - origin.x;
            float localY = worldPosition.y - origin.y;

            int gridX = Mathf.FloorToInt(localX / cellSize);
            int gridY = Mathf.FloorToInt(localY / cellSize);

            return new Vector2Int(gridX, gridY);
        }

        /// <summary>
        /// Convert grid coordinates to world position (cell center)
        /// </summary>
        public Vector3 GridToWorld(Vector2Int gridPosition)
        {
            float worldX = origin.x + (gridPosition.x * cellSize) + (cellSize * 0.5f);
            float worldY = origin.y + (gridPosition.y * cellSize) + (cellSize * 0.5f);

            return new Vector3(worldX, worldY, 0f);
        }

        /// <summary>
        /// Check if grid coordinates are within bounds
        /// </summary>
        public bool IsWithinBounds(Vector2Int gridPosition)
        {
            return gridPosition.x >= 0 && gridPosition.x < gridWidth &&
                   gridPosition.y >= 0 && gridPosition.y < gridHeight;
        }

        /// <summary>
        /// Check if a footprint fits within grid bounds
        /// </summary>
        public bool IsFootprintWithinBounds(Vector2Int gridPosition, int width, int height)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector2Int cell = new Vector2Int(gridPosition.x + x, gridPosition.y + y);
                    if (!IsWithinBounds(cell))
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Check if a cell is occupied
        /// </summary>
        public bool IsCellOccupied(Vector2Int gridPosition)
        {
            if (cells.TryGetValue(gridPosition, out GridCell cell))
                return cell.IsOccupied;
            return false;
        }

        /// <summary>
        /// Check if all cells in a footprint are unoccupied
        /// </summary>
        public bool IsFootprintClear(Vector2Int gridPosition, int width, int height)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector2Int cell = new Vector2Int(gridPosition.x + x, gridPosition.y + y);
                    if (IsCellOccupied(cell))
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Occupy cells for a tower
        /// </summary>
        public void OccupyCells(Vector2Int gridPosition, int width, int height, GameObject tower)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector2Int cell = new Vector2Int(gridPosition.x + x, gridPosition.y + y);

                    if (!cells.ContainsKey(cell))
                        cells[cell] = new GridCell(cell);

                    cells[cell].OccupyingTower = tower;
                    cells[cell].IsOccupied = true;
                }
            }
        }

        /// <summary>
        /// Free cells previously occupied by a tower
        /// </summary>
        public void FreeCells(Vector2Int gridPosition, int width, int height)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector2Int cell = new Vector2Int(gridPosition.x + x, gridPosition.y + y);

                    if (cells.TryGetValue(cell, out GridCell gridCell))
                    {
                        gridCell.OccupyingTower = null;
                        gridCell.IsOccupied = false;
                    }
                }
            }
        }

        /// <summary>
        /// Get the tower occupying a specific cell
        /// </summary>
        public GameObject GetTowerAtCell(Vector2Int gridPosition)
        {
            if (cells.TryGetValue(gridPosition, out GridCell cell))
                return cell.OccupyingTower;
            return null;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Draw grid bounds
            Gizmos.color = Color.cyan;
            Vector3 bottomLeft = new Vector3(origin.x, origin.y, 0f);
            Vector3 bottomRight = new Vector3(origin.x + gridWidth * cellSize, origin.y, 0f);
            Vector3 topRight = new Vector3(origin.x + gridWidth * cellSize, origin.y + gridHeight * cellSize, 0f);
            Vector3 topLeft = new Vector3(origin.x, origin.y + gridHeight * cellSize, 0f);

            Gizmos.DrawLine(bottomLeft, bottomRight);
            Gizmos.DrawLine(bottomRight, topRight);
            Gizmos.DrawLine(topRight, topLeft);
            Gizmos.DrawLine(topLeft, bottomLeft);

            // Draw grid lines (only in play mode to avoid clutter)
            if (Application.isPlaying)
            {
                Gizmos.color = new Color(0f, 1f, 1f, 0.1f);

                // Vertical lines
                for (int x = 0; x <= gridWidth; x++)
                {
                    Vector3 start = new Vector3(origin.x + x * cellSize, origin.y, 0f);
                    Vector3 end = new Vector3(origin.x + x * cellSize, origin.y + gridHeight * cellSize, 0f);
                    Gizmos.DrawLine(start, end);
                }

                // Horizontal lines
                for (int y = 0; y <= gridHeight; y++)
                {
                    Vector3 start = new Vector3(origin.x, origin.y + y * cellSize, 0f);
                    Vector3 end = new Vector3(origin.x + gridWidth * cellSize, origin.y + y * cellSize, 0f);
                    Gizmos.DrawLine(start, end);
                }
            }
        }
#endif
    }

    public class GridCell
    {
        public Vector2Int GridPosition;
        public bool IsOccupied;
        public GameObject OccupyingTower;

        public GridCell(Vector2Int position)
        {
            GridPosition = position;
            IsOccupied = false;
            OccupyingTower = null;
        }
    }
}