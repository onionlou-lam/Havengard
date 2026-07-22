using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapBoundsDebug : MonoBehaviour
{
    [ContextMenu("Debug All Tilemap Bounds")]
    void DebugTilemapBounds()
    {
        var tilemaps = FindObjectsOfType<Tilemap>();
        
        Debug.Log($"=== Found {tilemaps.Length} Tilemaps ===");
        
        foreach (var tilemap in tilemaps)
        {
            var cellBounds = tilemap.cellBounds;
            var localBounds = tilemap.localBounds;
            
            Debug.Log($"\n[{tilemap.name}]");
            Debug.Log($"  Cell Bounds: {cellBounds}");
            Debug.Log($"  Local Bounds: Center={localBounds.center}, Size={localBounds.size}");
            Debug.Log($"  Tile Count: {tilemap.GetUsedTilesCount()}");
            Debug.Log($"  GameObject Position: {tilemap.transform.position}");
            Debug.Log($"  Parent: {(tilemap.transform.parent != null ? tilemap.transform.parent.name : "None")}");
            
            // Check for NavMeshModifier
            var modifier = tilemap.GetComponent<NavMeshPlus.Components.NavMeshModifier>();
            var modifierTilemap = tilemap.GetComponent<NavMeshPlus.Components.NavMeshModifierTilemap>();
            
            Debug.Log($"  Has NavMeshModifier: {modifier != null}");
            if (modifier != null)
            {
                Debug.Log($"    - Override Area: {modifier.overrideArea}");
                Debug.Log($"    - Area: {modifier.area}");
                Debug.Log($"    - Ignore From Build: {modifier.ignoreFromBuild}");
            }
            
            Debug.Log($"  Has NavMeshModifierTilemap: {modifierTilemap != null}");
        }
    }
    
    [ContextMenu("Debug Grid Bounds")]
    void DebugGridBounds()
    {
        var grid = FindObjectOfType<Grid>();
        if (grid == null)
        {
            Debug.LogError("No Grid found!");
            return;
        }
        
        Debug.Log($"=== Grid: {grid.name} ===");
        Debug.Log($"  Position: {grid.transform.position}");
        Debug.Log($"  Cell Size: {grid.cellSize}");
        Debug.Log($"  Cell Layout: {grid.cellLayout}");
        
        // Get bounds of all child renderers
        var renderers = grid.GetComponentsInChildren<Renderer>();
        Debug.Log($"  Child Renderers: {renderers.Length}");
        
        if (renderers.Length > 0)
        {
            Bounds combinedBounds = renderers[0].bounds;
            foreach (var r in renderers)
            {
                combinedBounds.Encapsulate(r.bounds);
                Debug.Log($"    - {r.gameObject.name}: {r.bounds}");
            }
            Debug.Log($"  Combined Renderer Bounds: {combinedBounds}");
        }
    }
}