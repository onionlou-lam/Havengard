using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingSystem : MonoBehaviour
{
    public static BuildingSystem current;

    public GridLayout gridLayout;
    private Grid grid;
    [SerializeField] private Tilemap MainTilemap;
    [SerializeField] private TileBase buildableTile;

    public GameObject[] placeableObjects;
    private GameObject selectedObject;

    private PlaceableObject objectToPlace;
    private GameObject objectPlacing;
    public bool isPlacingObject;

    #region Unity methods
    private void Awake()
    {
        current = this;
        grid = gridLayout.gameObject.GetComponent<Grid>();
        selectedObject = placeableObjects[0];
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            InitializeWithObject(selectedObject);
            isPlacingObject = true;
        }
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPosition = MainTilemap.WorldToCell(worldPosition);
            TileBase tile = MainTilemap.GetTile(cellPosition);
            if(tile != null)
            {
                Debug.Log("tile exists:", tile);
            }
            else
            {
                Debug.Log("tile doesn't exist:", tile);

            }
        }
        if (isPlacingObject)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3Int cellPosition = MainTilemap.WorldToCell(mousePosition);
                TileBase tile = MainTilemap.GetTile(cellPosition);
                if(tile != null)
                {
                    isPlacingObject = false;
                    objectToPlace = null;
                }
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                Destroy(objectPlacing);
                isPlacingObject = false;
            }
            else
            {
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3 position = SnapCoordinateToGrid(mousePosition);
                objectToPlace.transform.position = position;
            }
        }
    }

    #endregion

    #region Utils

    public static Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit)) {
            Debug.Log(hit);
            return hit.point;
        };
        return Vector3.zero;
    }

    public Vector3 SnapCoordinateToGrid(Vector3 position)
    {
        Vector3Int cellPos = gridLayout.WorldToCell(position);
        position = grid.GetCellCenterWorld(cellPos);
        return position;
    }
    #endregion

    #region Building Placement

    public void InitializeWithObject(GameObject prefab)
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 position = SnapCoordinateToGrid(mousePosition);
        objectPlacing = Instantiate(prefab, position, Quaternion.identity);
        objectToPlace = objectPlacing.GetComponent<PlaceableObject>();
        objectPlacing.AddComponent<ObjectDrag>();
    }
    #endregion


}
