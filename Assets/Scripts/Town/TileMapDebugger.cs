using UnityEngine;
using UnityEngine.Tilemaps;

namespace Havengard.Town
{
    /// <summary>
    /// Debug tool to inspect tilemaps in detail
    /// </summary>
    public class TilemapDebugger : MonoBehaviour
    {
        [ContextMenu("Debug All Tilemaps")]
        public void DebugAllTilemaps()
        {
            Debug.Log("========== TILEMAP DETAILED DEBUG ==========");

            // Find all tilemaps
            Tilemap[] tilemaps = FindObjectsOfType<Tilemap>();
            Grid grid = FindObjectOfType<Grid>();

            Debug.Log($"Found {tilemaps.Length} tilemaps in scene");
            if (grid != null)
            {
                Debug.Log($"Grid Cell Size: {grid.cellSize}");
                Debug.Log($"Grid Cell Layout: {grid.cellLayout}");
            }

            foreach (var tilemap in tilemaps)
            {
                DebugTilemap(tilemap);
            }

            Debug.Log("========== DEBUG COMPLETE ==========");
        }

        private void DebugTilemap(Tilemap tilemap)
        {
            Debug.Log($"\n=== Tilemap: {tilemap.name} ===");
            Debug.Log($"GameObject Layer: {LayerMask.LayerToName(tilemap.gameObject.layer)}");
            Debug.Log($"Sorting Layer: {tilemap.GetComponent<TilemapRenderer>()?.sortingLayerName}");

            // Bounds info
            BoundsInt bounds = tilemap.cellBounds;
            Debug.Log($"Cell Bounds: {bounds}");
            Debug.Log($"Local Bounds: {tilemap.localBounds}");

            // Tile count
            int tileCount = 0;
            int visibleTileCount = 0;

            foreach (var pos in bounds.allPositionsWithin)
            {
                if (tilemap.HasTile(pos))
                {
                    tileCount++;
                    TileBase tile = tilemap.GetTile(pos);
                    if (tile != null)
                    {
                        visibleTileCount++;
                    }
                }
            }

            Debug.Log($"Total Tiles with HasTile(): {tileCount}");
            Debug.Log($"Non-null Tiles: {visibleTileCount}");
            Debug.Log($"GetUsedTilesCount(): {tilemap.GetUsedTilesCount()}");

            // Collider info
            TilemapCollider2D tilemapCollider = tilemap.GetComponent<TilemapCollider2D>();
            if (tilemapCollider != null)
            {
                Debug.Log($"✓ Has TilemapCollider2D");
                Debug.Log($"  - Is Trigger: {tilemapCollider.isTrigger}");
                Debug.Log($"  - Used By Composite: {tilemapCollider.usedByComposite}");
                Debug.Log($"  - Offset: {tilemapCollider.offset}");
            }
            else
            {
                Debug.LogWarning($"✗ NO TilemapCollider2D!");
            }

            CompositeCollider2D composite = tilemap.GetComponent<CompositeCollider2D>();
            if (composite != null)
            {
                Debug.Log($"✓ Has CompositeCollider2D");
                Debug.Log($"  - Geometry Type: {composite.geometryType}");
                Debug.Log($"  - Point Count: {composite.pointCount}");
            }

            // Check if tilemap is actually visible
            TilemapRenderer renderer = tilemap.GetComponent<TilemapRenderer>();
            if (renderer != null)
            {
                Debug.Log($"Renderer Mode: {renderer.mode}");
                Debug.Log($"Detect Chunk Culling Bounds: {renderer.detectChunkCullingBounds}");
            }

            // Sample some tiles
            if (visibleTileCount > 0)
            {
                Debug.Log($"Sample tile positions (first 5):");
                int count = 0;
                foreach (var pos in bounds.allPositionsWithin)
                {
                    if (tilemap.HasTile(pos) && count < 5)
                    {
                        Vector3 worldPos = tilemap.CellToWorld(pos);
                        Debug.Log($"  Tile at {pos} -> World: {worldPos}");
                        count++;
                    }
                }
            }
        }

        [ContextMenu("List All Collider2Ds")]
        public void ListAllColliders()
        {
            Debug.Log("========== ALL COLLIDER2D IN SCENE ==========");

            Collider2D[] colliders = FindObjectsOfType<Collider2D>();
            Debug.Log($"Found {colliders.Length} total Collider2D components");

            int boxCount = 0;
            int tilemapCount = 0;
            int compositeCount = 0;
            int otherCount = 0;

            foreach (var col in colliders)
            {
                string type = col.GetType().Name;
                bool isTrigger = col.isTrigger;
                string layer = LayerMask.LayerToName(col.gameObject.layer);

                if (col is BoxCollider2D) boxCount++;
                else if (col is TilemapCollider2D) tilemapCount++;
                else if (col is CompositeCollider2D) compositeCount++;
                else otherCount++;

                Debug.Log($"{col.gameObject.name}: {type}, Layer={layer}, Trigger={isTrigger}");
            }

            Debug.Log($"\nSummary:");
            Debug.Log($"  BoxCollider2D: {boxCount}");
            Debug.Log($"  TilemapCollider2D: {tilemapCount}");
            Debug.Log($"  CompositeCollider2D: {compositeCount}");
            Debug.Log($"  Other: {otherCount}");
        }
    }
}