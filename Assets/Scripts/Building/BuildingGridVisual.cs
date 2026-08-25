using UnityEngine;

namespace Havengard.Building
{
    /// <summary>
    /// Visualizes the buildable grid overlay
    /// </summary>
    public class BuildingGridVisual : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BuildGrid buildGrid;

        [Header("Visual Settings")]
        [SerializeField] private Color gridLineColor = new Color(0.5f, 0.5f, 1f, 0.3f);
        [SerializeField] private float lineWidth = 0.05f;
        [SerializeField] private Material lineMaterial;

        [Header("Performance")]
        [SerializeField] private bool useLineRenderers = false; // If false, uses simple quads

        private GameObject gridContainer;
        private bool isVisible = false;

        private void Awake()
        {
            if (buildGrid == null)
                buildGrid = FindFirstObjectByType<BuildGrid>();
        }

        public void ShowGrid()
        {
            if (isVisible)
                return;

            CreateGridVisual();
            isVisible = true;
        }

        public void HideGrid()
        {
            if (!isVisible)
                return;

            if (gridContainer != null)
                Destroy(gridContainer);

            isVisible = false;
        }

        private void CreateGridVisual()
        {
            if (buildGrid == null)
                return;

            // Cleanup existing
            if (gridContainer != null)
                Destroy(gridContainer);

            gridContainer = new GameObject("GridVisual");
            gridContainer.transform.SetParent(transform);

            // Create grid background
            CreateGridBackground();

            // Create grid lines
            if (useLineRenderers)
                CreateGridLinesWithLineRenderer();
            else
                CreateGridLinesWithQuads();
        }

        private void CreateGridBackground()
        {
            GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Quad);
            bg.name = "GridBackground";
            bg.transform.SetParent(gridContainer.transform);

            // Remove collider
            Destroy(bg.GetComponent<Collider>());

            // Position and scale
            Bounds bounds = buildGrid.GetWorldBounds();
            bg.transform.position = new Vector3(bounds.center.x, bounds.center.y, 0.5f); // Behind grid lines
            bg.transform.localScale = new Vector3(bounds.size.x, bounds.size.y, 1f);

            // Set color
            var renderer = bg.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            renderer.material.color = new Color(0f, 0f, 0f, 0.2f); // Dark semi-transparent
            renderer.sortingLayerName = "Default";
            renderer.sortingOrder = -1;
        }

        private void CreateGridLinesWithQuads()
        {
            Material mat = lineMaterial != null ? lineMaterial : new Material(Shader.Find("Sprites/Default"));
            mat.color = gridLineColor;

            // Vertical lines
            for (int x = 0; x <= buildGrid.GridWidth; x++)
            {
                GameObject line = GameObject.CreatePrimitive(PrimitiveType.Quad);
                line.name = $"GridLine_V_{x}";
                line.transform.SetParent(gridContainer.transform);
                Destroy(line.GetComponent<Collider>());

                float worldX = buildGrid.Origin.x + x * buildGrid.CellSize;
                float worldY = buildGrid.Origin.y + buildGrid.GridHeight * buildGrid.CellSize * 0.5f;

                line.transform.position = new Vector3(worldX, worldY, 0.4f);
                line.transform.localScale = new Vector3(lineWidth, buildGrid.GridHeight * buildGrid.CellSize, 1f);

                var renderer = line.GetComponent<Renderer>();
                renderer.material = mat;
                renderer.sortingLayerName = "UI";
                renderer.sortingOrder = 10;
            }

            // Horizontal lines
            for (int y = 0; y <= buildGrid.GridHeight; y++)
            {
                GameObject line = GameObject.CreatePrimitive(PrimitiveType.Quad);
                line.name = $"GridLine_H_{y}";
                line.transform.SetParent(gridContainer.transform);
                Destroy(line.GetComponent<Collider>());

                float worldX = buildGrid.Origin.x + buildGrid.GridWidth * buildGrid.CellSize * 0.5f;
                float worldY = buildGrid.Origin.y + y * buildGrid.CellSize;

                line.transform.position = new Vector3(worldX, worldY, 0.4f);
                line.transform.localScale = new Vector3(buildGrid.GridWidth * buildGrid.CellSize, lineWidth, 1f);

                var renderer = line.GetComponent<Renderer>();
                renderer.material = mat;
                renderer.sortingLayerName = "UI";
                renderer.sortingOrder = 10;
            }
        }

        private void CreateGridLinesWithLineRenderer()
        {
            // Alternative implementation using LineRenderer (more performance-friendly for large grids)
            // TODO: Implement if needed for optimization
            Debug.LogWarning("[BuildingGridVisual] LineRenderer mode not yet implemented, falling back to quads");
            CreateGridLinesWithQuads();
        }

        private void OnDestroy()
        {
            if (gridContainer != null)
                Destroy(gridContainer);
        }
    }
}