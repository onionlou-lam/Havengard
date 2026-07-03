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
        [SerializeField] private GameObject skillTreePanel;

        [Header("HUD Behavior")]
        [SerializeField] private bool autoHideHUDWhenMenusOpen = true;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            InitializeCanvases();
        }

        private void InitializeCanvases()
        {
            // HUD is always visible during gameplay
            if (hudCanvas != null)
                hudCanvas.enabled = true;

            // Menus start hidden
            if (menusCanvas != null)
                menusCanvas.enabled = true;

            // Pre-wave starts hidden
            if (preWaveCanvas != null)
                preWaveCanvas.enabled = true;

            // Tooltips always enabled
            if (tooltipsCanvas != null)
                tooltipsCanvas.enabled = true;

            // Hide all menu panels initially
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(false);
            if (inventoryPanel != null)
                inventoryPanel.SetActive(false);
            if (itemCachePanel != null)
                itemCachePanel.SetActive(false);
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

        public bool IsHUDVisible()
        {
            return hudCanvas != null && hudCanvas.enabled;
        }
        #endregion

        #region Menu Control
        public void ShowPauseMenu()
        {
            HideAllMenuPanels();
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(true);

            if (autoHideHUDWhenMenusOpen)
                HideHUD();

            Time.timeScale = 0f;
        }

        public void HidePauseMenu()
        {
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(false);

            if (autoHideHUDWhenMenusOpen && !IsAnyMenuOpen())
                ShowHUD();

            Time.timeScale = 1f;
        }

        public void ShowInventory()
        {
            HideAllMenuPanels();
            if (inventoryPanel != null)
                inventoryPanel.SetActive(true);

            if (autoHideHUDWhenMenusOpen)
                HideHUD();
        }

        public void HideInventory()
        {
            if (inventoryPanel != null)
                inventoryPanel.SetActive(false);

            if (autoHideHUDWhenMenusOpen && !IsAnyMenuOpen())
                ShowHUD();
        }

        public void ShowSkillTree()
        {
            HideAllMenuPanels();
            if (skillTreePanel != null)
                skillTreePanel.SetActive(true);

            if (autoHideHUDWhenMenusOpen)
                HideHUD();

            Time.timeScale = 0f; // Pause game
        }

        public void HideSkillTree()
        {
            if (skillTreePanel != null)
                skillTreePanel.SetActive(false);

            if (autoHideHUDWhenMenusOpen && !IsAnyMenuOpen())
                ShowHUD();

            Time.timeScale = 1f; // Resume game
        }

        public void ShowItemCache()
        {
            HideAllMenuPanels();
            if (itemCachePanel != null)
                itemCachePanel.SetActive(true);

            if (autoHideHUDWhenMenusOpen)
                HideHUD();
        }

        public void HideItemCache()
        {
            if (itemCachePanel != null)
                itemCachePanel.SetActive(false);

            if (autoHideHUDWhenMenusOpen && !IsAnyMenuOpen())
                ShowHUD();
        }

        private void HideAllMenuPanels()
        {
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(false);
            if (inventoryPanel != null)
                inventoryPanel.SetActive(false);
            if (itemCachePanel != null)
                itemCachePanel.SetActive(false);
            if (skillTreePanel != null)
                skillTreePanel.SetActive(false);
        }

        private bool IsAnyMenuOpen()
        {
            return (pauseMenuPanel != null && pauseMenuPanel.activeSelf) ||
                   (inventoryPanel != null && inventoryPanel.activeSelf) ||
                   (itemCachePanel != null && itemCachePanel.activeSelf) ||
                   (skillTreePanel != null && skillTreePanel.activeSelf);
        }
        #endregion

        #region PreWave Control
        public void ShowPreWaveUI()
        {
            if (preWavePhasePanel != null)
                preWavePhasePanel.SetActive(true);

            // HUD remains visible during pre-wave phase
        }

        public void HidePreWaveUI()
        {
            if (preWavePhasePanel != null)
                preWavePhasePanel.SetActive(false);
        }
        #endregion
    }
}