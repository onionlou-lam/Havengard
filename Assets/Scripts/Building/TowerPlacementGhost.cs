using UnityEngine;

namespace Havengard.Building
{
    /// <summary>
    /// Visual preview/ghost for tower placement
    /// Shows valid/invalid placement with color feedback
    /// </summary>
    public class TowerPlacementGhost : MonoBehaviour
    {
        [Header("Visual Feedback")]
        [SerializeField] private SpriteRenderer towerSprite;
        [SerializeField] private Color validColor = new Color(0f, 1f, 0f, 0.5f);
        [SerializeField] private Color invalidColor = new Color(1f, 0f, 0f, 0.5f);

        [Header("Footprint Visualization")]
        [SerializeField] private GameObject footprintCellPrefab;
        [SerializeField] private Transform footprintContainer;

        private TowerBuildData towerData;
        private BuildGrid grid;
        private PlacementValidator validator;
        private Vector2Int currentGridPosition;
        private bool isValid;

        private SpriteRenderer[] footprintCells;

        public Vector2Int CurrentGridPosition => currentGridPosition;
        public bool IsValid => isValid;

        public void Initialize(TowerBuildData data, BuildGrid buildGrid, PlacementValidator placementValidator)
        {
            towerData = data;
            grid = buildGrid;
            validator = placementValidator;

            // Setup sprite
            if (towerSprite == null)
                towerSprite = GetComponent<SpriteRenderer>();

            // If no sprite renderer found, try to get from prefab
            if (towerSprite == null && data.towerPrefab != null)
            {
                var prefabSprite = data.towerPrefab.GetComponentInChildren<SpriteRenderer>();
                if (prefabSprite != null && towerSprite == null)
                {
                    // Create a sprite renderer for the ghost
                    towerSprite = gameObject.AddComponent<SpriteRenderer>();
                    towerSprite.sprite = prefabSprite.sprite;
                    towerSprite.sortingLayerName = "UI"; // Render on top
                    towerSprite.sortingOrder = 100;
                }
            }

            // Create footprint visualization
            CreateFootprintVisuals();
        }

        private void CreateFootprintVisuals()
        {
            if (footprintContainer == null)
            {
                GameObject container = new GameObject("FootprintContainer");
                container.transform.SetParent(transform);
                footprintContainer = container.transform;
            }

            // Create footprint cell visuals
            footprintCells = new SpriteRenderer[towerData.gridWidth * towerData.gridHeight];

            for (int x = 0; x < towerData.gridWidth; x++)
            {
                for (int y = 0; y < towerData.gridHeight; y++)
                {
                    int index = x + y * towerData.gridWidth;

                    GameObject cellObj;
                    SpriteRenderer spriteRenderer;
                    
                    if (footprintCellPrefab != null)
                    {
                        cellObj = Instantiate(footprintCellPrefab, footprintContainer);
                        spriteRenderer = cellObj.GetComponent<SpriteRenderer>();
                        
                        if (spriteRenderer == null)
                        {
                            Debug.LogWarning("[TowerPlacementGhost] Footprint prefab missing SpriteRenderer, creating default");
                            Destroy(cellObj);
                            cellObj = CreateDefaultFootprintCell();
                            spriteRenderer = cellObj.GetComponent<SpriteRenderer>();
                        }
                    }
                    else
                    {
                        // Create simple sprite-based cell
                        cellObj = CreateDefaultFootprintCell();
                        spriteRenderer = cellObj.GetComponent<SpriteRenderer>();
                    }

                    cellObj.name = $"FootprintCell_{x}_{y}";
                    cellObj.transform.SetParent(footprintContainer);
                    
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.sortingLayerName = "UI";
                        spriteRenderer.sortingOrder = 99;
                    }

                    // Position relative to ghost
                    float offsetX = (x - towerData.gridWidth * 0.5f + 0.5f) * grid.CellSize;
                    float offsetY = (y - towerData.gridHeight * 0.5f + 0.5f) * grid.CellSize;
                    cellObj.transform.localPosition = new Vector3(offsetX, offsetY, 0.1f);
                    cellObj.transform.localScale = Vector3.one * grid.CellSize * 0.9f;

                    footprintCells[index] = spriteRenderer;
                }
            }
        }

        private GameObject CreateDefaultFootprintCell()
        {
            GameObject cellObj = new GameObject("FootprintCell");
            
            // Add SpriteRenderer
            SpriteRenderer sr = cellObj.AddComponent<SpriteRenderer>();
            
            // Create a simple white square sprite
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            sr.sprite = sprite;
            
            return cellObj;
        }

        public void UpdatePosition(Vector2Int gridPosition)
        {
            currentGridPosition = gridPosition;

            // Convert to world position (center of footprint)
            Vector3 bottomLeftWorld = grid.GridToWorld(gridPosition);
            float footprintWorldWidth = towerData.gridWidth * grid.CellSize;
            float footprintWorldHeight = towerData.gridHeight * grid.CellSize;

            Vector3 centerOffset = new Vector3(
                footprintWorldWidth * 0.5f - grid.CellSize * 0.5f,
                footprintWorldHeight * 0.5f - grid.CellSize * 0.5f,
                0f
            );

            transform.position = bottomLeftWorld + centerOffset;

            // Validate placement
            var validationResult = validator.ValidatePlacement(towerData, gridPosition, 0);
            isValid = validationResult == PlacementValidationResult.Valid;

            // Update visual feedback
            UpdateVisualFeedback();
        }

        private void UpdateVisualFeedback()
        {
            Color feedbackColor = isValid ? validColor : invalidColor;

            // Update tower sprite
            if (towerSprite != null)
            {
                towerSprite.color = feedbackColor;
            }

            // Update footprint cells
            if (footprintCells != null)
            {
                foreach (var cell in footprintCells)
                {
                    if (cell != null)
                    {
                        cell.color = feedbackColor;
                    }
                }
            }
        }

        private void OnDestroy()
        {
            // Cleanup
            if (footprintContainer != null)
                Destroy(footprintContainer.gameObject);
        }
    }
}