using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Havengard.Items;
using Havengard.Save;
using Havengard.UI.Notifications;

namespace Havengard.UI
{
    public class PauseMenuUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject pausePanel;

        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button inventoryButton;
        [SerializeField] private Button skillTreeButton; // Changed from characterButton
        [SerializeField] private Button followersButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button saveGameButton;
        [SerializeField] private Button loadGameButton;
        [SerializeField] private Button exitToMainMenuButton;
        [SerializeField] private Button exitGameButton;

        [Header("UI Panels")]
        [SerializeField] private ItemCacheUI itemCacheUI;
        [SerializeField] private SkillTreeUI skillTreeUI; // Add SkillTree reference

        private bool isPaused = false;

        private void Start()
        {
            // Hook up button listeners
            if (resumeButton != null)
                resumeButton.onClick.AddListener(Resume);

            if (inventoryButton != null)
                inventoryButton.onClick.AddListener(OpenInventory);

            if (skillTreeButton != null) // Changed from characterButton
                skillTreeButton.onClick.AddListener(OpenSkillTree); // Changed method

            if (followersButton != null)
                followersButton.onClick.AddListener(OpenFollowers);

            if (optionsButton != null)
                optionsButton.onClick.AddListener(OpenOptions);

            if (saveGameButton != null)
                saveGameButton.onClick.AddListener(SaveGame);

            if (loadGameButton != null)
                loadGameButton.onClick.AddListener(LoadGame);

            if (exitToMainMenuButton != null)
                exitToMainMenuButton.onClick.AddListener(ExitToMainMenu);

            if (exitGameButton != null)
                exitGameButton.onClick.AddListener(ExitGame);

            // Find references
            if (itemCacheUI == null)
                itemCacheUI = FindFirstObjectByType<ItemCacheUI>();

            if (skillTreeUI == null)
                skillTreeUI = FindFirstObjectByType<SkillTreeUI>();

            // Subscribe to ItemCacheUI close event
            if (itemCacheUI != null)
            {
                itemCacheUI.OnRequestClose += ReturnToPauseMenu;
            }

            // Subscribe to SkillTreeUI close event
            if (skillTreeUI != null)
            {
                skillTreeUI.OnRequestClose += ReturnToPauseMenu;
            }
        }

        private void OnDestroy()
        {
            if (itemCacheUI != null)
            {
                itemCacheUI.OnRequestClose -= ReturnToPauseMenu;
            }

            if (skillTreeUI != null)
            {
                skillTreeUI.OnRequestClose -= ReturnToPauseMenu;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // Let sub-panels handle their own Escape key and close via OnRequestClose.
                // Without this guard, PauseMenuUI and SkillTreeUI/ItemCacheUI would both
                // react to the same key press in the same frame, causing a state race.
                if ((itemCacheUI != null && itemCacheUI.IsShowing) ||
                    (skillTreeUI != null && SkillTreeUI.IsOpen))
                    return;

                if (isPaused)
                    Resume();
                else
                    Pause();
            }
        }

        public void Pause()
        {
            pausePanel.SetActive(true);
            Time.timeScale = 0f;
            isPaused = true;

            if (Havengard.Audio.GameAudioManager.Instance != null)
                Havengard.Audio.GameAudioManager.Instance.PauseAudio();
        }

        public void Resume()
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1f;
            isPaused = false;

            if (Havengard.Audio.GameAudioManager.Instance != null)
                Havengard.Audio.GameAudioManager.Instance.ResumeAudio();
        }

        /// <summary>
        /// Re-shows the pause panel after a sub-panel (skill tree, item cache) closes itself.
        /// Unlike Resume(), this keeps the game paused since we're still inside the pause flow.
        /// </summary>
        private void ReturnToPauseMenu()
        {
            pausePanel.SetActive(true);
            Time.timeScale = 0f;
            isPaused = true;
        }

        private void OpenInventory()
        {
            if (itemCacheUI != null)
            {
                pausePanel.SetActive(false);
                // isPaused stays true — we're still within the pause flow, just showing a different panel
                itemCacheUI.Show();
            }
        }

        private void OpenSkillTree() // New method
        {
            if (skillTreeUI != null)
            {
                pausePanel.SetActive(false);
                // isPaused stays true — we're still within the pause flow, just showing a different panel
                // CALL PUBLIC OpenSkillTree directly instead of ToggleSkillTree
                skillTreeUI.OpenSkillTree();
            }
            else
            {
                Debug.LogWarning("[PauseMenuUI] SkillTreeUI reference not found!");
            }
        }

        private void OpenFollowers()
        {
            Debug.Log("[PauseMenuUI] Followers screen not implemented yet");
        }

        private void OpenOptions()
        {
            Debug.Log("[PauseMenuUI] Options screen not implemented yet");
        }

        /// <summary>
        /// Save the current game state
        /// </summary>
        private void SaveGame()
        {
            if (SaveManager.Instance == null)
            {
                NotificationManager.Instance?.Show("Save Manager not found!", NotificationType.Error);
                return;
            }

            SaveManager.Instance.SaveGame();

            // Show notification
            NotificationManager.Instance?.Show("Game Saved!", NotificationType.Success);
        }

        /// <summary>
        /// Load the saved game state
        /// </summary>
        private void LoadGame()
        {
            if (SaveManager.Instance == null)
            {
                NotificationManager.Instance?.Show("Save Manager not found!", NotificationType.Error);
                return;
            }

            if (!SaveManager.Instance.SaveExists())
            {
                NotificationManager.Instance?.Show("No save file found!", NotificationType.Error);
                return;
            }

            SaveManager.Instance.LoadGame();

            // Show notification
            NotificationManager.Instance?.Show("Game Loaded!", NotificationType.Success);

            // Resume game after loading
            Resume();
        }

        /// <summary>
        /// Exit to the main menu scene
        /// </summary>
        private void ExitToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }

        /// <summary>
        /// Exit the game application
        /// </summary>
        private void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}