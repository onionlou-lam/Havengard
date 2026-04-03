using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingSystem : MonoBehaviour
{
    public static BuildingSystem _currentBuildingSystem;

    public GridLayout gridLayout;
    private Grid _grid;
    [SerializeField] private Tilemap MainTilemap;
    [SerializeField] private TileBase buildableTile;

    [SerializeField] private GameObject[] placeableObjects;
    private GameObject _selectedObject;

    private PlaceableObject _objectToPlace;
    private GameObject _objectPlacing;
    private bool _isPlacingObject;
    private Dictionary<Vector3Int, bool> _placedObjects = new Dictionary<Vector3Int, bool>();

    #region Unity methods
    private void Awake()
    {
        _currentBuildingSystem = this;
        _grid = gridLayout.gameObject.GetComponent<Grid>();
        _selectedObject = placeableObjects[0];
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && !_isPlacingObject)
        {
            InitializeWithObject(_selectedObject);
            _isPlacingObject = true;
        }
        if (_isPlacingObject)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // Attemt to place building
            if (Input.GetMouseButtonDown(0))
            {
                Vector3Int cellPosition = MainTilemap.WorldToCell(mousePosition);
                TileBase tile = MainTilemap.GetTile(cellPosition);
                
                if (tile != null && !_placedObjects.ContainsKey(cellPosition))
                {
                    _isPlacingObject = false;
                    _objectToPlace = null;
                    _placedObjects[cellPosition] = true;
                }
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                Destroy(_objectPlacing);
                _isPlacingObject = false;
            }
            else
            {
                Vector3 position = SnapCoordinateToGrid(mousePosition);
                _objectToPlace.transform.position = position - _objectToPlace.getBuildingCenter();
            }
        }
    }

    #endregion

    #region Utils

    public static Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log(hit);
            return hit.point;
        };
        return Vector3.zero;
    }

    public Vector3 SnapCoordinateToGrid(Vector3 position)
    {
        Vector3Int cellPos = gridLayout.WorldToCell(position);
        position = _grid.GetCellCenterWorld(cellPos);
        return position;
    }
    #endregion

    #region Building Placement

    public void InitializeWithObject(GameObject prefab)
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 position = SnapCoordinateToGrid(mousePosition);
        _objectPlacing = Instantiate(prefab, position, Quaternion.identity);
        _objectToPlace = _objectPlacing.GetComponent<PlaceableObject>();
        _objectPlacing.transform.position += _objectToPlace.getBuildingCenter();
        _objectPlacing.AddComponent<ObjectDrag>();
    }
    #endregion


}
