using UnityEngine;

namespace Havengard.Building
{
    /// <summary>
    /// Camera controller for building mode.
    /// Supports edge panning and is constrained to the build grid.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class BuildingCameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BuildGrid buildGrid;

        [Header("Pan Settings")]
        [SerializeField] private float panSpeed = 10f;
        [SerializeField] private float edgePanThreshold = 50f; // Pixels from screen edge
        [SerializeField] private bool enableEdgePan = true;
        [SerializeField] private bool enableKeyboardPan = true;

        [Header("Zoom Settings")]
        [SerializeField] private float minZoom = 5f;
        [SerializeField] private float maxZoom = 20f;
        [SerializeField] private float zoomSpeed = 2f;

        private Camera cam;
        private Bounds gridBounds;
        private float currentZoom;

        private void Awake()
        {
            cam = GetComponent<Camera>();

            if (buildGrid == null)
                buildGrid = FindFirstObjectByType<BuildGrid>();

            if (buildGrid != null)
                gridBounds = buildGrid.GetWorldBounds();

            currentZoom = cam.orthographicSize;
        }

        private void Update()
        {
            HandlePanning();
            HandleZoom();
            ClampToGridBounds();
        }

        private void HandlePanning()
        {
            Vector3 movement = Vector3.zero;

            // Edge panning with mouse
            if (enableEdgePan)
            {
                Vector3 mousePos = Input.mousePosition;

                if (mousePos.x < edgePanThreshold)
                    movement.x -= 1f;
                else if (mousePos.x > Screen.width - edgePanThreshold)
                    movement.x += 1f;

                if (mousePos.y < edgePanThreshold)
                    movement.y -= 1f;
                else if (mousePos.y > Screen.height - edgePanThreshold)
                    movement.y += 1f;
            }

            // Keyboard panning (WASD)
            if (enableKeyboardPan)
            {
                if (Input.GetKey(KeyCode.W))
                    movement.y += 1f;
                if (Input.GetKey(KeyCode.S))
                    movement.y -= 1f;
                if (Input.GetKey(KeyCode.A))
                    movement.x -= 1f;
                if (Input.GetKey(KeyCode.D))
                    movement.x += 1f;
            }

            // Apply movement
            if (movement != Vector3.zero)
            {
                movement.Normalize();
                transform.position += movement * panSpeed * Time.deltaTime;
            }
        }

        private void HandleZoom()
        {
            float scrollDelta = Input.mouseScrollDelta.y;

            if (scrollDelta != 0f)
            {
                currentZoom -= scrollDelta * zoomSpeed;
                currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
                cam.orthographicSize = currentZoom;
            }
        }

        private void ClampToGridBounds()
        {
            if (buildGrid == null)
                return;

            // Calculate camera bounds based on orthographic size
            float camHeight = cam.orthographicSize * 2f;
            float camWidth = camHeight * cam.aspect;

            float halfCamWidth = camWidth * 0.5f;
            float halfCamHeight = camHeight * 0.5f;

            // Grid bounds
            float minX = gridBounds.min.x + halfCamWidth;
            float maxX = gridBounds.max.x - halfCamWidth;
            float minY = gridBounds.min.y + halfCamHeight;
            float maxY = gridBounds.max.y - halfCamHeight;

            // Handle case where camera is larger than grid
            if (minX > maxX)
            {
                minX = maxX = gridBounds.center.x;
            }

            if (minY > maxY)
            {
                minY = maxY = gridBounds.center.y;
            }

            // Clamp position
            Vector3 pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            transform.position = pos;
        }

        /// <summary>
        /// Focus camera on the center of the grid
        /// </summary>
        public void FocusOnGrid()
        {
            if (buildGrid != null)
            {
                gridBounds = buildGrid.GetWorldBounds();
                Vector3 centerPos = gridBounds.center;
                centerPos.z = transform.position.z; // Maintain camera Z
                transform.position = centerPos;
            }
        }
    }
}