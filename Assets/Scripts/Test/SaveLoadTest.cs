using UnityEngine;
using Havengard.Save;

namespace Havengard.Test
{
    /// <summary>
    /// Debug script to test save/load functionality with keyboard shortcuts
    /// Attach to SaveManager GameObject
    /// </summary>
    public class SaveLoadTest : MonoBehaviour
    {
        [Header("Key Bindings")]
        [SerializeField] private KeyCode saveKey = KeyCode.F5;
        [SerializeField] private KeyCode loadKey = KeyCode.F9;
        [SerializeField] private KeyCode checkKey = KeyCode.F10;
        [SerializeField] private KeyCode deleteSaveKey = KeyCode.F11;
        [SerializeField] private KeyCode toggleUIKey = KeyCode.F12;

        [Header("UI Settings")]
        [SerializeField] private bool showOnScreenUI = true;
        [SerializeField] private bool showDebugLogs = true;

        [Header("UI Style")]
        [SerializeField] private int fontSize = 14;
        [SerializeField] private Color boxColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        [SerializeField] private Color textColor = Color.white;

        private GUIStyle boxStyle;
        private GUIStyle labelStyle;
        private bool stylesInitialized = false;

        private void Start()
        {
            Debug.Log($"[SaveLoadTest] Initialized. Press {toggleUIKey} to toggle UI visibility.");
        }

        private void Update()
        {
            // Toggle UI visibility
            if (Input.GetKeyDown(toggleUIKey))
            {
                showOnScreenUI = !showOnScreenUI;
                Debug.Log($"[SaveLoadTest] On-screen UI: {(showOnScreenUI ? "ENABLED" : "DISABLED")}");
            }

            // Save game
            if (Input.GetKeyDown(saveKey))
            {
                if (showDebugLogs)
                    Debug.Log($"=== MANUAL SAVE TRIGGERED ({saveKey}) ===");

                if (SaveManager.Instance != null)
                {
                    SaveManager.Instance.SaveGame();
                }
                else
                {
                    Debug.LogError("[SaveLoadTest] SaveManager.Instance is NULL!");
                }
            }

            // Load game
            if (Input.GetKeyDown(loadKey))
            {
                if (showDebugLogs)
                    Debug.Log($"=== MANUAL LOAD TRIGGERED ({loadKey}) ===");

                if (SaveManager.Instance != null)
                {
                    SaveManager.Instance.LoadGame();
                }
                else
                {
                    Debug.LogError("[SaveLoadTest] SaveManager.Instance is NULL!");
                }
            }

            // Check save files
            if (Input.GetKeyDown(checkKey))
            {
                if (showDebugLogs)
                    Debug.Log($"=== CHECKING SAVE FILES ({checkKey}) ===");

                if (SaveManager.Instance != null)
                {
                    bool exists = SaveManager.Instance.SaveExists();
                    Debug.Log($"<color=cyan>Default save file exists: {exists}</color>");

                    string[] saves = SaveManager.Instance.GetAllSaveFiles();
                    Debug.Log($"<color=cyan>Total save files: {saves.Length}</color>");

                    if (saves.Length > 0)
                    {
                        Debug.Log("<color=yellow>Save files found:</color>");
                        foreach (string save in saves)
                        {
                            Debug.Log($"  <color=yellow>→ {save}</color>");
                        }
                    }

                    Debug.Log($"<color=green>Save directory: {Application.persistentDataPath}/Saves/</color>");
                }
                else
                {
                    Debug.LogError("[SaveLoadTest] SaveManager.Instance is NULL!");
                }
            }

            // Delete default save (useful for testing)
            if (Input.GetKeyDown(deleteSaveKey))
            {
                if (showDebugLogs)
                    Debug.Log($"=== DELETE SAVE TRIGGERED ({deleteSaveKey}) ===");

                if (SaveManager.Instance != null)
                {
                    bool deleted = SaveManager.Instance.DeleteSave("GameSave");
                    if (deleted)
                    {
                        Debug.Log("<color=red>Default save file deleted!</color>");
                    }
                    else
                    {
                        Debug.LogWarning("No save file to delete.");
                    }
                }
                else
                {
                    Debug.LogError("[SaveLoadTest] SaveManager.Instance is NULL!");
                }
            }
        }

        private void InitializeStyles()
        {
            if (stylesInitialized) return;

            // Box style
            boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.background = MakeTex(2, 2, boxColor);
            boxStyle.fontSize = fontSize;
            boxStyle.normal.textColor = textColor;
            boxStyle.padding = new RectOffset(10, 10, 10, 10);

            // Label style
            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = fontSize;
            labelStyle.normal.textColor = textColor;
            labelStyle.padding = new RectOffset(5, 5, 2, 2);

            stylesInitialized = true;
        }

        private void OnGUI()
        {
            if (!showOnScreenUI) return;

            InitializeStyles();

            // Main box
            float boxWidth = 350;
            float boxHeight = 170;
            float margin = 10;

            GUI.Box(new Rect(margin, margin, boxWidth, boxHeight), "SAVE SYSTEM DEBUG", boxStyle);

            float yPos = margin + 30;
            float lineHeight = 22;

            // Instructions
            GUI.Label(new Rect(margin + 10, yPos, boxWidth - 20, lineHeight),
                $"{saveKey} - Save Game", labelStyle);
            yPos += lineHeight;

            GUI.Label(new Rect(margin + 10, yPos, boxWidth - 20, lineHeight),
                $"{loadKey} - Load Game", labelStyle);
            yPos += lineHeight;

            GUI.Label(new Rect(margin + 10, yPos, boxWidth - 20, lineHeight),
                $"{checkKey} - Check Save Files", labelStyle);
            yPos += lineHeight;

            GUI.Label(new Rect(margin + 10, yPos, boxWidth - 20, lineHeight),
                $"{deleteSaveKey} - Delete Default Save", labelStyle);
            yPos += lineHeight;

            GUI.Label(new Rect(margin + 10, yPos, boxWidth - 20, lineHeight),
                $"{toggleUIKey} - Toggle This UI", labelStyle);
            yPos += lineHeight;

            // Status
            string status = SaveManager.Instance != null ? "READY" : "ERROR";
            Color statusColor = SaveManager.Instance != null ? Color.green : Color.red;

            GUIStyle statusStyle = new GUIStyle(labelStyle);
            statusStyle.normal.textColor = statusColor;
            statusStyle.fontStyle = FontStyle.Bold;

            GUI.Label(new Rect(margin + 10, yPos, boxWidth - 20, lineHeight),
                $"Status: {status}", statusStyle);
        }

        // Helper to create colored texture
        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}