using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using NavMeshPlus.Components;

namespace Havengard.Town
{
    /// <summary>
    /// Compares NavMesh setup between scenes to find differences
    /// </summary>
    public class SceneComparison : MonoBehaviour
    {
        [Header("Analysis")]
        [SerializeField] private string currentSceneName;

        private void Start()
        {
            currentSceneName = SceneManager.GetActiveScene().name;
        }

        [ContextMenu("Full Scene Analysis")]
        public void AnalyzeScene()
        {
            Debug.Log($"========== SCENE ANALYSIS: {currentSceneName} ==========");

            AnalyzeNavMesh();
            AnalyzeTilemaps();
            AnalyzeGrid();

            Debug.Log("========== ANALYSIS COMPLETE ==========");
        }

        private void AnalyzeNavMesh()
        {
            Debug.Log("\n--- NavMesh Analysis ---");

            NavMeshSurface[] surfaces = FindObjectsOfType<NavMeshSurface>();
            if (surfaces.Length == 0)
            {
                Debug.LogError("NO NavMeshSurface found!");
                return;
            }

            foreach (var surface in surfaces)
            {
                Debug.Log($"NavMeshSurface on: {surface.gameObject.name}");
                Debug.Log($"  Transform:");
                Debug.Log($"    Position: {surface.transform.position}");
                Debug.Log($"    Rotation: {surface.transform.eulerAngles}");
                Debug.Log($"    LocalRotation: {surface.transform.localEulerAngles}");
                Debug.Log($"  Settings:");
                Debug.Log($"    Agent Type ID: {surface.agentTypeID}");
                Debug.Log($"    Collect Objects: {surface.collectObjects}");
                Debug.Log($"    Use Geometry: {surface.useGeometry}");
                Debug.Log($"    Layer Mask (int): {surface.layerMask.value}");
                Debug.Log($"    Layer Mask (binary): {System.Convert.ToString(surface.layerMask.value, 2).PadLeft(32, '0')}");
                Debug.Log($"    Default Area: {surface.defaultArea}");
                Debug.Log($"    Override Voxel Size: {surface.overrideVoxelSize}");
                Debug.Log($"    Voxel Size: {surface.voxelSize}");
                Debug.Log($"    Override Tile Size: {surface.overrideTileSize}");
                Debug.Log($"    Tile Size: {surface.tileSize}");

                if (surface.navMeshData != null)
                {
                    Debug.Log($"  NavMesh Data:");
                    Debug.Log($"    Bounds: {surface.navMeshData.sourceBounds}");
                    Debug.Log($"    Position: {surface.navMeshData.position}");
                }
                else
                {
                    Debug.LogWarning("  No NavMesh Data!");
                }

                // Check components on same GameObject
                var components = surface.gameObject.GetComponents<Component>();
                Debug.Log($"  Components on GameObject:");
                foreach (var comp in components)
                {
                    Debug.Log($"    - {comp.GetType().Name}");
                }
            }
        }

        private void AnalyzeTilemaps()
        {
            Debug.Log("\n--- Tilemap Analysis ---");

            Tilemap[] tilemaps = FindObjectsOfType<Tilemap>();
            Debug.Log($"Found {tilemaps.Length} tilemaps");

            foreach (var tilemap in tilemaps)
            {
                Debug.Log($"\nTilemap: {tilemap.gameObject.name}");
                Debug.Log($"  Layer: {LayerMask.LayerToName(tilemap.gameObject.layer)} ({tilemap.gameObject.layer})");
                Debug.Log($"  Tile Count: {tilemap.GetUsedTilesCount()}");
                Debug.Log($"  Cell Bounds: {tilemap.cellBounds}");
                Debug.Log($"  Local Bounds: {tilemap.localBounds}");

                // Check renderer
                TilemapRenderer renderer = tilemap.GetComponent<TilemapRenderer>();
                if (renderer != null)
                {
                    Debug.Log($"  Renderer:");
                    Debug.Log($"    Mode: {renderer.mode}");
                    Debug.Log($"    Detect Chunk Culling Bounds: {renderer.detectChunkCullingBounds}");
                    Debug.Log($"    Sorting Layer: {renderer.sortingLayerName}");
                    Debug.Log($"    Order in Layer: {renderer.sortingOrder}");
                }

                // Check for MeshFilter
                MeshFilter meshFilter = tilemap.GetComponent<MeshFilter>();
                Debug.Log($"  Has MeshFilter: {meshFilter != null}");
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    Debug.Log($"    Mesh Vertex Count: {meshFilter.sharedMesh.vertexCount}");
                }

                // Check colliders
                TilemapCollider2D tilemapCol = tilemap.GetComponent<TilemapCollider2D>();
                Debug.Log($"  Has TilemapCollider2D: {tilemapCol != null}");

                BoxCollider2D boxCol = tilemap.GetComponent<BoxCollider2D>();
                Debug.Log($"  Has BoxCollider2D: {boxCol != null}");
                if (boxCol != null)
                {
                    Debug.Log($"    Size: {boxCol.size}");
                    Debug.Log($"    Offset: {boxCol.offset}");
                    Debug.Log($"    Is Trigger: {boxCol.isTrigger}");
                }

                CompositeCollider2D composite = tilemap.GetComponent<CompositeCollider2D>();
                Debug.Log($"  Has CompositeCollider2D: {composite != null}");

                // Check Navigation components
                NavMeshModifier modifier = tilemap.GetComponent<NavMeshModifier>();
                Debug.Log($"  Has NavMeshModifier: {modifier != null}");
                if (modifier != null)
                {
                    Debug.Log($"    Override Area: {modifier.overrideArea}");
                    Debug.Log($"    Area: {modifier.area}");
                    Debug.Log($"    Ignore From Build: {modifier.ignoreFromBuild}");
                }

                NavMeshModifierTilemap modifierTilemap = tilemap.GetComponent<NavMeshModifierTilemap>();
                Debug.Log($"  Has NavMeshModifierTilemap: {modifierTilemap != null}");

                // List all components
                var components = tilemap.gameObject.GetComponents<Component>();
                Debug.Log($"  All Components:");
                foreach (var comp in components)
                {
                    Debug.Log($"    - {comp.GetType().FullName}");
                }
            }
        }

        private void AnalyzeGrid()
        {
            Debug.Log("\n--- Grid Analysis ---");

            Grid[] grids = FindObjectsOfType<Grid>();
            if (grids.Length == 0)
            {
                Debug.LogWarning("No Grid found!");
                return;
            }

            foreach (var grid in grids)
            {
                Debug.Log($"Grid: {grid.gameObject.name}");
                Debug.Log($"  Cell Size: {grid.cellSize}");
                Debug.Log($"  Cell Gap: {grid.cellGap}");
                Debug.Log($"  Cell Layout: {grid.cellLayout}");
                Debug.Log($"  Cell Swizzle: {grid.cellSwizzle}");
            }
        }

        [ContextMenu("List Layer Mask Details")]
        public void ListLayerMaskDetails()
        {
            NavMeshSurface surface = FindObjectOfType<NavMeshSurface>();
            if (surface == null)
            {
                Debug.LogError("No NavMeshSurface found!");
                return;
            }

            Debug.Log($"Layer Mask Value: {surface.layerMask.value}");
            Debug.Log($"Binary: {System.Convert.ToString(surface.layerMask.value, 2).PadLeft(32, '0')}");
            Debug.Log($"\nLayers included:");

            for (int i = 0; i < 32; i++)
            {
                if ((surface.layerMask.value & (1 << i)) != 0)
                {
                    string layerName = LayerMask.LayerToName(i);
                    Debug.Log($"  Layer {i}: {layerName}");
                }
            }
        }
    }
}