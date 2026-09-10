using UnityEngine;
using Havengard.UI;
using Havengard.Expeditions;

namespace Havengard.Expeditions.UI
{
    /// <summary>
    /// Main controller for the Expedition Map UI
    /// Handles opening/closing and input
    /// </summary>
    public class ExpeditionMapController : MonoBehaviour
    {
        public static ExpeditionMapController Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private Canvas expeditionMapCanvas;
        [SerializeField] private GameObject mapPanel;
        [SerializeField] private ExpeditionSetupPanel setupPanel;
        [SerializeField] private FollowerListUI followerListUI;

        [Header("HUD Display")]
        [Tooltip("Reference to Canvas_HUD to hide it while the expedition map is open")]
        [SerializeField] private GameObject hudCanvas;

        [Tooltip("Optional: specific HUD elements to hide (wave counter, currency, etc)")]
        [SerializeField] private GameObject[] additionalHUDElementsToHide;

        [Header("Input")]
        [SerializeField] private KeyCode toggleMapKey = KeyCode.M;
        [SerializeField] private KeyCode closeKey = KeyCode.Escape;

        private bool isMapOpen = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Start with map closed
            CloseMap();
        }

        private void Update()
        {
            // Toggle map with M key
            if (Input.GetKeyDown(toggleMapKey))
            {
                if (isMapOpen)
                    CloseMap();
                else
                    OpenMap();
            }

            // Close with Escape
            if (isMapOpen && Input.GetKeyDown(closeKey))
            {
                CloseMap();
            }
        }

        /// <summary>
        /// Opens the expedition map
        /// </summary>
        public void OpenMap()
        {
            if (!CanOpenMap())
            {
                Debug.LogWarning("[ExpeditionMap] Cannot open map in current game state");
                return;
            }

            Debug.Log("[ExpeditionMap] Map opened");
            isMapOpen = true;
            GameplayUIBlocker.Push();

            if (expeditionMapCanvas != null)
                expeditionMapCanvas.enabled = true;

            if (mapPanel != null)
                mapPanel.SetActive(true);

            // Hide HUD while the map is open
            if (hudCanvas != null) hudCanvas.SetActive(false);
            if (additionalHUDElementsToHide != null)
            {
                foreach (var e in additionalHUDElementsToHide)
                    if (e != null) e.SetActive(false);
            }

            // Refresh follower list
            if (followerListUI != null)
                followerListUI.RefreshFollowerList();

            // Close setup panel
            if (setupPanel != null)
                setupPanel.ClosePanel();

            // Pause game or handle time scale if needed
            // Time.timeScale = 0f;
        }

        /// <summary>
        /// Closes the expedition map
        /// </summary>
        public void CloseMap()
        {
            Debug.Log("[ExpeditionMap] Map closed");

            // Only pop the blocker if we were actually open, to avoid double-decrementing
            // if CloseMap() is called multiple times (e.g. from Start()).
            if (isMapOpen)
                GameplayUIBlocker.Pop();

            isMapOpen = false;

            if (expeditionMapCanvas != null)
                expeditionMapCanvas.enabled = false;

            if (mapPanel != null)
                mapPanel.SetActive(false);

            // Restore HUD
            if (hudCanvas != null) hudCanvas.SetActive(true);
            if (additionalHUDElementsToHide != null)
            {
                foreach (var e in additionalHUDElementsToHide)
                    if (e != null) e.SetActive(true);
            }

            // Close any open panels
            if (setupPanel != null)
                setupPanel.ClosePanel();

            // Resume game
            // Time.timeScale = 1f;
        }

        /// <summary>
        /// Checks if the map can be opened (e.g., not during combat)
        /// </summary>
        private bool CanOpenMap()
        {
            // Add conditions here:
            // - Not during active wave combat
            // - Not in dialogue
            // etc.

            // For now, always allow
            return true;
        }

        /// <summary>
        /// Opens the expedition setup panel for a specific dungeon
        /// </summary>
        public void OpenExpeditionSetup(ExpeditionData data)
        {
            if (setupPanel != null)
            {
                setupPanel.OpenPanel(data);
            }
        }

        public bool IsMapOpen => isMapOpen;
    }
}