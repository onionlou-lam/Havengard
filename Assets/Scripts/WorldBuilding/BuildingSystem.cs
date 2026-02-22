using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingSystem : MonoBehaviour
{
    public static BuildingSystem current;

    public GridLayout gridLayout;
    private Grid grid;
    [SerializeField] private Tilemap MainTilemap;
    [SerializeField] private TileBase whiteTile;

    public GameObject[] placeableObjects;
    private GameObject selectedObject;

    private PlaceableObject objectToPlace;
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
        if(isPlacingObject)
        {
            if (Input.GetMouseButtonDown(0))
            {
                isPlacingObject = false;
                objectToPlace = null;
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
        GameObject obj = Instantiate(prefab, position, Quaternion.identity);
        objectToPlace = obj.GetComponent<PlaceableObject>();
        obj.AddComponent<ObjectDrag>();
    }
    #endregion


}
