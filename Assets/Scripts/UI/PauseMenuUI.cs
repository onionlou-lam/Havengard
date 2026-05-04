using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Havengard.Items;

namespace Havengard.UI
{
    /// <summary>
    /// Pause menu UI controller with game state management
    /// </summary>
    public class PauseMenuUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject pausePanel;
        
        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button inventoryButton;
        [SerializeField] private Button characterButton;
        [SerializeField] private Button followersButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button exitToMainMenuButton;
        [SerializeField] private Button exitGameButton;

        [Header("UI Panels")]
        [SerializeField] private ItemCacheUI itemCacheUI;

        private bool isPaused = false;

        private void Awake()
        {
            Debug.Log("[PauseMenuUI] Awake called - script is active");
        }

        private void Start()
        {
            Debug.Log("[PauseMenuUI] Start called");
            
            // Hook up button listeners
            if (resumeButton != null)
            {
                resumeButton.onClick.AddListener(Resume);
                Debug.Log("[PauseMenuUI] Resume button hooked up");
            }
            else
            {
                Debug.LogWarning("[PauseMenuUI] Resume button is NULL!");
            }
            
            if (inventoryButton != null)
            {
                inventoryButton.onClick.AddListener(OpenInventory);
                Debug.Log("[PauseMenuUI] Inventory button hooked up");
            }
            
            if (characterButton != null)
                characterButton.onClick.AddListener(OpenCharacter);
            
            if (followersButton != null)
                followersButton.onClick.AddListener(OpenFollowers);
            
            if (optionsButton != null)
                optionsButton.onClick.AddListener(OpenOptions);
            
            if (exitToMainMenuButton != null)
                exitToMainMenuButton.onClick.AddListener(ExitToMainMenu);
            
            if (exitGameButton != null)
                exitGameButton.onClick.AddListener(ExitGame);

            // Find references
            if (itemCacheUI == null)
            {
                itemCacheUI = FindFirstObjectByType<ItemCacheUI>();
                Debug.Log($"[PauseMenuUI] Auto-found ItemCacheUI: {itemCacheUI != null}");
            }

            // Subscribe to ItemCacheUI close event
            if (itemCacheUI != null)
            {
                itemCacheUI.OnRequestClose += OnInventoryRequestClose;
                Debug.Log("[PauseMenuUI] Subscribed to ItemCacheUI close event");
            }

            // Start with menu hidden
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
                Debug.Log("[PauseMenuUI] Pause panel hidden on start");
            }
            else
            {
                Debug.LogError("[PauseMenuUI] Pause panel reference is NULL!");
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (itemCacheUI != null)
            {
                itemCacheUI.OnRequestClose -= OnInventoryRequestClose;
            }
        }

        private void Update()
        {
            // Toggle pause menu with 'P' key
            if (Input.GetKeyDown(KeyCode.P))
            {
                Debug.Log($"[PauseMenuUI] P key pressed! isPaused={isPaused}");
                
                if (isPaused)
                    Resume();
                else
                    Pause();
            }

            // Allow ESC to close submenus or pause menu
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // Check if inventory is open
                if (itemCacheUI != null && itemCacheUI.gameObject.activeSelf && itemCacheUI.IsShowing)
                {
                    Debug.Log("[PauseMenuUI] ESC pressed - closing inventory");
                    itemCacheUI.Hide();
                    ShowPauseMenu();
                }
                else if (isPaused)
                {
                    Debug.Log("[PauseMenuUI] ESC pressed - resuming game");
                    Resume();
                }
                else
                {
                    Debug.Log("[PauseMenuUI] ESC pressed - pausing game");
                    Pause();
                }
            }
        }

        /// <summary>
        /// Pause the game and show menu
        /// </summary>
        public void Pause()
        {
            isPaused = true;
            Time.timeScale = 0f;
            
            if (pausePanel != null)
                pausePanel.SetActive(true);
            
            Debug.Log("[PauseMenu] Game paused");
        }

        /// <summary>
        /// Resume the game
        /// </summary>
        public void Resume()
        {
            isPaused = false;
            Time.timeScale = 1f;
            
            if (pausePanel != null)
                pausePanel.SetActive(false);
            
            // Close any open submenus
            CloseAllSubmenus();
            
            Debug.Log("[PauseMenu] Game resumed");
        }

        /// <summary>
        /// Show the pause menu (used when returning from submenus)
        /// </summary>
        private void ShowPauseMenu()
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(true);
                Debug.Log("[PauseMenuUI] Pause menu shown");
            }
        }

        /// <summary>
        /// Hide the pause menu (used when opening submenus)
        /// </summary>
        private void HidePauseMenu()
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
                Debug.Log("[PauseMenuUI] Pause menu hidden");
            }
        }

        /// <summary>
        /// Open inventory menu (item cache + equipped items)
        /// </summary>
        private void OpenInventory()
        {
            Debug.Log("[PauseMenu] Opening Inventory");
            
            // Hide the pause menu
            HidePauseMenu();
            
            // Show item cache UI
            if (itemCacheUI != null)
            {
                itemCacheUI.Show();
                Debug.Log("[PauseMenu] ItemCacheUI opened");
            }
            else
            {
                Debug.LogWarning("[PauseMenu] ItemCacheUI reference is missing!");
            }
        }

        /// <summary>
        /// Called when ItemCacheUI requests to be closed (back/close button clicked)
        /// </summary>
        private void OnInventoryRequestClose()
        {
            Debug.Log("[PauseMenuUI] Inventory requested close - returning to pause menu");
            ShowPauseMenu();
        }

        /// <summary>
        /// Open character screen
        /// </summary>
        private void OpenCharacter()
        {
            Debug.Log("[PauseMenu] Character button pressed - Not yet implemented");
            // TODO: Hide pause menu and show character screen
            // HidePauseMenu();
        }

        /// <summary>
        /// Open followers screen
        /// </summary>
        private void OpenFollowers()
        {
            Debug.Log("[PauseMenu] Followers button pressed - Not yet implemented");
            // TODO: Hide pause menu and show followers screen
            // HidePauseMenu();
        }

        /// <summary>
        /// Open options/settings screen
        /// </summary>
        private void OpenOptions()
        {
            Debug.Log("[PauseMenu] Options button pressed - Not yet implemented");
            // TODO: Hide pause menu and show options screen
            // HidePauseMenu();
        }

        /// <summary>
        /// Exit to main menu
        /// </summary>
        private void ExitToMainMenu()
        {
            Debug.Log("[PauseMenu] Exiting to Main Menu");
            
            // Reset time scale
            Time.timeScale = 1f;
            
            // Load main menu scene
            SceneManager.LoadScene("MainMenu");
        }

        /// <summary>
        /// Exit the game
        /// </summary>
        private void ExitGame()
        {
            Debug.Log("[PauseMenu] Exiting Game");
            
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        /// <summary>
        /// Close all submenu panels
        /// </summary>
        private void CloseAllSubmenus()
        {
            if (itemCacheUI != null)
                itemCacheUI.Hide();
        }

        /// <summary>
        /// Check if game is currently paused
        /// </summary>
        public bool IsPaused => isPaused;
    }
}