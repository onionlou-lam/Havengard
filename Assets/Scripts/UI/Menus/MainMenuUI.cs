using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Havengard.Save;
using Havengard.Audio;

namespace Havengard.UI
{
    /// <summary>
    /// Main menu UI controller - handles navigation between main menu panels
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject loadGamePanel;
        [SerializeField] private GameObject optionsPanel;

        [Header("Main Menu Buttons")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button loadGameButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button exitGameButton;

        [Header("Settings")]
        [SerializeField] private string gameSceneName = "GameScene";
        [SerializeField] private int maxSaveSlots = 6;

        private void Start()
        {
            // Hook up main menu buttons
            if (newGameButton != null)
            {
                newGameButton.onClick.AddListener(OnNewGameClicked);
                AddButtonSounds(newGameButton);
            }

            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinueClicked);
                AddButtonSounds(continueButton);
            }

            if (loadGameButton != null)
            {
                loadGameButton.onClick.AddListener(OnLoadGameClicked);
                AddButtonSounds(loadGameButton);
            }

            if (optionsButton != null)
            {
                optionsButton.onClick.AddListener(OnOptionsClicked);
                AddButtonSounds(optionsButton);
            }

            if (exitGameButton != null)
            {
                exitGameButton.onClick.AddListener(OnExitGameClicked);
                AddButtonSounds(exitGameButton);
            }

            // Show main menu by default
            ShowMainMenu();
            UpdateContinueButton();
        }

        /// <summary>
        /// Add UI sounds to a button
        /// </summary>
        private void AddButtonSounds(Button button)
        {
            if (button == null) return;

            // Add click sound via event trigger
            var trigger = button.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (trigger == null)
                trigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

            // Pointer enter (hover)
            var hoverEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
            hoverEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
            hoverEntry.callback.AddListener((data) => {
                if (UIAudioManager.Instance != null && button.interactable)
                    UIAudioManager.Instance.PlayButtonHover();
            });
            trigger.triggers.Add(hoverEntry);

            // Click sound is played via button listener
        }

        /// <summary>
        /// Show the main menu panel
        /// </summary>
        public void ShowMainMenu()
        {
            SetActivePanel(mainMenuPanel);

            if (UIAudioManager.Instance != null)
                UIAudioManager.Instance.PlayPanelClose();
        }

        /// <summary>
        /// Show the load game panel
        /// </summary>
        public void ShowLoadGamePanel()
        {
            SetActivePanel(loadGamePanel);

            if (UIAudioManager.Instance != null)
                UIAudioManager.Instance.PlayPanelOpen();
        }

        /// <summary>
        /// Show the options panel
        /// </summary>
        public void ShowOptionsPanel()
        {
            SetActivePanel(optionsPanel);

            if (UIAudioManager.Instance != null)
                UIAudioManager.Instance.PlayPanelOpen();
        }

        /// <summary>
        /// Set active panel and hide all others
        /// </summary>
        private void SetActivePanel(GameObject activePanel)
        {
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(mainMenuPanel == activePanel);

            if (loadGamePanel != null)
                loadGamePanel.SetActive(loadGamePanel == activePanel);

            if (optionsPanel != null)
                optionsPanel.SetActive(optionsPanel == activePanel);
        }

        /// <summary>
        /// Update continue button state - enabled if any save exists
        /// </summary>
        private void UpdateContinueButton()
        {
            if (continueButton == null) return;

            // Check if any save slot has data
            bool anySaveExists = false;
            for (int i = 1; i <= maxSaveSlots; i++)
            {
                if (SaveUtility.SaveFileExists($"SaveSlot{i}"))
                {
                    anySaveExists = true;
                    break;
                }
            }

            continueButton.interactable = anySaveExists;
        }

        #region Button Handlers

        /// <summary>
        /// Handle New Game button - directly load game scene for now
        /// </summary>
        private void OnNewGameClicked()
        {
            if (UIAudioManager.Instance != null)
                UIAudioManager.Instance.PlayButtonClick();

            Debug.Log("[MainMenuUI] New Game clicked - loading game scene");

            // For now, just load the game scene directly
            // TODO: Later this will show character creation
            SceneManager.LoadScene(gameSceneName);
        }

        /// <summary>
        /// Handle Continue button - load most recent save
        /// </summary>
        private void OnContinueClicked()
        {
            if (UIAudioManager.Instance != null)
                UIAudioManager.Instance.PlayButtonClick();

            Debug.Log("[MainMenuUI] Continue clicked");

            // Find most recent save
            GameSaveData mostRecentSave = null;
            int mostRecentSlot = -1;

            for (int i = 1; i <= maxSaveSlots; i++)
            {
                GameSaveData saveData = SaveUtility.LoadFromFile<GameSaveData>($"SaveSlot{i}");
                if (saveData != null)
                {
                    if (mostRecentSave == null ||
                        System.DateTime.Parse(saveData.saveDate) > System.DateTime.Parse(mostRecentSave.saveDate))
                    {
                        mostRecentSave = saveData;
                        mostRecentSlot = i;
                    }
                }
            }

            if (mostRecentSlot != -1)
            {
                // Set active save slot and load game
                if (SaveSlotManager.Instance != null)
                    SaveSlotManager.Instance.SetActiveSlot(mostRecentSlot);

                if (UIAudioManager.Instance != null)
                    UIAudioManager.Instance.PlaySuccess();

                SceneManager.LoadScene(gameSceneName);
            }
            else
            {
                Debug.LogWarning("[MainMenuUI] No save found!");

                if (UIAudioManager.Instance != null)
                    UIAudioManager.Instance.PlayError();
            }
        }

        /// <summary>
        /// Handle Load Game button - show load game panel
        /// </summary>
        private void OnLoadGameClicked()
        {
            if (UIAudioManager.Instance != null)
                UIAudioManager.Instance.PlayButtonClick();

            Debug.Log("[MainMenuUI] Load Game clicked");
            ShowLoadGamePanel();
        }

        /// <summary>
        /// Handle Options button
        /// </summary>
        private void OnOptionsClicked()
        {
            if (UIAudioManager.Instance != null)
                UIAudioManager.Instance.PlayButtonClick();

            Debug.Log("[MainMenuUI] Options clicked");
            ShowOptionsPanel();
        }

        /// <summary>
        /// Handle Exit Game button
        /// </summary>
        private void OnExitGameClicked()
        {
            if (UIAudioManager.Instance != null)
                UIAudioManager.Instance.PlayButtonClick();

            Debug.Log("[MainMenuUI] Exit Game clicked");
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        #endregion
    }
}