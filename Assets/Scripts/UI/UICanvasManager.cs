using UnityEngine;

namespace Havengard.UI
{
    /// <summary>
    /// Manages multiple UI canvases and their visibility
    /// </summary>
    public class UICanvasManager : MonoBehaviour
    {
        public static UICanvasManager Instance { get; private set; }

        [Header("Canvas References")]
        [SerializeField] private Canvas hudCanvas;
        [SerializeField] private Canvas menusCanvas;
        [SerializeField] private Canvas preWaveCanvas;
        [SerializeField] private Canvas tooltipsCanvas;

        [Header("Panel References")]
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private GameObject itemCachePanel;
        [SerializeField] private GameObject preWavePhasePanel;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Initialize canvas states
            InitializeCanvases();
        }

        private void InitializeCanvases()
        {
            // HUD is always visible during gameplay
            if (hudCanvas != null)
                hudCanvas.enabled = true;

            // Menus start hidden
            if (menusCanvas != null)
                menusCanvas.enabled = true; // Canvas stays enabled, panels control visibility

            // Pre-wave starts hidden
            if (preWaveCanvas != null)
                preWaveCanvas.enabled = true;

            // Tooltips always enabled
            if (tooltipsCanvas != null)
                tooltipsCanvas.enabled = true;

            // Hide all menu panels initially
            HideAllMenuPanels();
        }

        #region HUD Control
        public void ShowHUD()
        {
            if (hudCanvas != null)
                hudCanvas.enabled = true;
        }

        public void HideHUD()
        {
            if (hudCanvas != null)
                hudCanvas.enabled = false;
        }
        #endregion

        #region Menu Control
        public void ShowPauseMenu()
        {
            HideAllMenuPanels();
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(true);
            Time.timeScale = 0f;
        }

        public void HidePauseMenu()
        {
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(false);
            Time.timeScale = 1f;
        }

        public void ShowInventory()
        {
            HideAllMenuPanels();
            if (inventoryPanel != null)
                inventoryPanel.SetActive(true);
        }

        public void HideInventory()
        {
            if (inventoryPanel != null)
                inventoryPanel.SetActive(false);
        }

        public void ShowItemCache()
        {
            HideAllMenuPanels();
            if (itemCachePanel != null)
                itemCachePanel.SetActive(true);
        }

        public void HideItemCache()
        {
            if (itemCachePanel != null)
                itemCachePanel.SetActive(false);
        }

        private void HideAllMenuPanels()
        {
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(false);
            if (inventoryPanel != null)
                inventoryPanel.SetActive(false);
            if (itemCachePanel != null)
                itemCachePanel.SetActive(false);
        }
        #endregion

        #region Pre-Wave Control
        public void ShowPreWavePhase()
        {
            if (preWavePhasePanel != null)
                preWavePhasePanel.SetActive(true);
        }

        public void HidePreWavePhase()
        {
            if (preWavePhasePanel != null)
                preWavePhasePanel.SetActive(false);
        }
        #endregion

        #region Tooltip Control
        public Canvas GetTooltipCanvas()
        {
            return tooltipsCanvas;
        }
        #endregion
    }
}