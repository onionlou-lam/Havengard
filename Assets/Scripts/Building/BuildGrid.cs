using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

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

        [Header("Tilemap Integration")]
        [SerializeField] private UnityEngine.Tilemaps.Tilemap buildableTilemap;
        [SerializeField] private bool requireBuildableTile = true;

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
            {
                if (cell.OccupyingTower != null)
                {
                    Debug.Log($"[BuildGrid] Found tower at {gridPosition}: {cell.OccupyingTower.name}");
                    return cell.OccupyingTower;
                }
            }
            
            return null;
        }

        /// <summary>
        /// Get all occupied cells (for debugging)
        /// </summary>
        public int GetOccupiedCellCount()
        {
            int count = 0;
            foreach (var cell in cells.Values)
            {
                if (cell.IsOccupied)
                    count++;
            }
            return count;
        }

        /// <summary>
        /// Check if a grid position is on a buildable tile
        /// </summary>
        public bool IsBuildableTile(Vector2Int gridPosition)
        {
            if (!requireBuildableTile || buildableTilemap == null)
                return true; // Allow building everywhere if no tilemap set

            // Convert grid position to tilemap cell position
            Vector3 worldPos = GridToWorld(gridPosition);
            Vector3Int tilemapCell = buildableTilemap.WorldToCell(worldPos);

            // Check if tile exists at this position
            UnityEngine.Tilemaps.TileBase tile = buildableTilemap.GetTile(tilemapCell);

            bool isBuildable = tile != null;
            
            if (!isBuildable)
            {
                Debug.Log($"[BuildGrid] Position {gridPosition} is not buildable (tilemap cell: {tilemapCell}, tile: {tile})");
            }

            return isBuildable;
        }

        /// <summary>
        /// Check if entire footprint is on buildable tiles
        /// </summary>
        public bool IsFootprintBuildable(Vector2Int gridPosition, int width, int height)
        {
            if (!requireBuildableTile || buildableTilemap == null)
                return true;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector2Int cell = new Vector2Int(gridPosition.x + x, gridPosition.y + y);
                    if (!IsBuildableTile(cell))
                        return false;
                }
            }
            return true;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()  // Changed from OnDrawGizmosSelected to always show
        {
            // Draw grid bounds - THICK CYAN OUTLINE
            Gizmos.color = Color.cyan;
            float z = transform.position.z;
            
            Vector3 bottomLeft = new Vector3(origin.x, origin.y, z);
            Vector3 bottomRight = new Vector3(origin.x + gridWidth * cellSize, origin.y, z);
            Vector3 topRight = new Vector3(origin.x + gridWidth * cellSize, origin.y + gridHeight * cellSize, z);
            Vector3 topLeft = new Vector3(origin.x, origin.y + gridHeight * cellSize, z);

            // Draw thick border
            Debug.DrawLine(bottomLeft, bottomRight, Color.cyan);
            Debug.DrawLine(bottomRight, topRight, Color.cyan);
            Debug.DrawLine(topRight, topLeft, Color.cyan);
            Debug.DrawLine(topLeft, bottomLeft, Color.cyan);

            // Also draw with Gizmos
            Gizmos.DrawLine(bottomLeft, bottomRight);
            Gizmos.DrawLine(bottomRight, topRight);
            Gizmos.DrawLine(topRight, topLeft);
            Gizmos.DrawLine(topLeft, bottomLeft);

            // Draw corner markers
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(bottomLeft, cellSize * 0.5f);
            Gizmos.DrawWireSphere(bottomRight, cellSize * 0.5f);
            Gizmos.DrawWireSphere(topRight, cellSize * 0.5f);
            Gizmos.DrawWireSphere(topLeft, cellSize * 0.5f);

            // Draw grid lines (only every 5th to reduce clutter in scene view)
            Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
            
            for (int x = 0; x <= gridWidth; x += 5)
            {
                Vector3 start = new Vector3(origin.x + x * cellSize, origin.y, z);
                Vector3 end = new Vector3(origin.x + x * cellSize, origin.y + gridHeight * cellSize, z);
                Gizmos.DrawLine(start, end);
            }

            for (int y = 0; y <= gridHeight; y += 5)
            {
                Vector3 start = new Vector3(origin.x, origin.y + y * cellSize, z);
                Vector3 end = new Vector3(origin.x + gridWidth * cellSize, origin.y + y * cellSize, z);
                Gizmos.DrawLine(start, end);
            }

            // Draw label using Handles (requires UnityEditor)
            Handles.Label(
                new Vector3(origin.x + (gridWidth * cellSize) / 2, origin.y + gridHeight * cellSize + 2f, z),
                $"BuildGrid: {gridWidth}x{gridHeight}\nOrigin: {origin}\nCell Size: {cellSize}",
                new GUIStyle() 
                { 
                    alignment = TextAnchor.MiddleCenter,
                    normal = new GUIStyleState() { textColor = Color.cyan },
                    fontSize = 14,
                    fontStyle = FontStyle.Bold
                }
            );

            // Draw occupied cells in play mode
            if (Application.isPlaying && cells != null && cells.Count > 0)
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
                foreach (var cell in cells.Values)
                {
                    if (cell.IsOccupied)
                    {
                        Vector3 cellCenter = GridToWorld(cell.GridPosition);
                        cellCenter.z = z;
                        Gizmos.DrawCube(cellCenter, new Vector3(cellSize * 0.9f, cellSize * 0.9f, 0.2f));
                    }
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