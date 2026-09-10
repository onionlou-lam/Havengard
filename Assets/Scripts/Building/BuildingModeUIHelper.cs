using UnityEngine;

namespace Havengard.Building
{
    /// <summary>
    /// Helper to manage UI visibility during building mode
    /// </summary>
    public class BuildingModeUIHelper : MonoBehaviour
    {
        [Header("UI Elements to Hide in Building Mode")]
        [SerializeField] private GameObject[] hideInBuildingMode;
        
        [Header("Blocking UI to Disable")]
        [SerializeField] private GameObject pausePanel;

        private bool[] originalStates;

        public void HideUIElements()
        {
            // Hide blocking UI first
            if (pausePanel != null && pausePanel.activeSelf)
            {
                Debug.Log("[BuildingModeUIHelper] Hiding PausePanel");
                pausePanel.SetActive(false);
            }
            
            if (hideInBuildingMode == null || hideInBuildingMode.Length == 0)
                return;

            // Store original states
            originalStates = new bool[hideInBuildingMode.Length];
            
            for (int i = 0; i < hideInBuildingMode.Length; i++)
            {
                if (hideInBuildingMode[i] != null)
                {
                    originalStates[i] = hideInBuildingMode[i].activeSelf;
                    hideInBuildingMode[i].SetActive(false);
                }
            }

            Debug.Log($"[BuildingModeUIHelper] Hid {hideInBuildingMode.Length} UI elements");
        }

        public void RestoreUIElements()
        {
            // Restore pause panel if it was active
            // (usually we don't want to restore it)
            
            if (hideInBuildingMode == null || originalStates == null)
                return;

            for (int i = 0; i < hideInBuildingMode.Length && i < originalStates.Length; i++)
            {
                if (hideInBuildingMode[i] != null)
                {
                    hideInBuildingMode[i].SetActive(originalStates[i]);
                }
            }

            Debug.Log($"[BuildingModeUIHelper] Restored {hideInBuildingMode.Length} UI elements");
        }
    }
}