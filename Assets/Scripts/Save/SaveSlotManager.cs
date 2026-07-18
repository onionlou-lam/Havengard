using UnityEngine;

namespace Havengard.Save
{
    /// <summary>
    /// Manages the currently active save slot
    /// </summary>
    public class SaveSlotManager : MonoBehaviour
    {
        public static SaveSlotManager Instance { get; private set; }

        private int activeSlot = 1;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Set the active save slot
        /// </summary>
        public void SetActiveSlot(int slotIndex)
        {
            activeSlot = Mathf.Clamp(slotIndex, 1, 6);
            Debug.Log($"[SaveSlotManager] Active slot set to: {activeSlot}");
        }

        /// <summary>
        /// Get the active save slot
        /// </summary>
        public int GetActiveSlot()
        {
            return activeSlot;
        }

        /// <summary>
        /// Get the save file name for the active slot
        /// </summary>
        public string GetActiveSaveFileName()
        {
            return $"SaveSlot{activeSlot}";
        }
    }
}