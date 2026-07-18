using UnityEngine;

namespace Havengard.Save
{
    /// <summary>
    /// Tracks total playtime for the current game session
    /// </summary>
    public class PlaytimeTracker : MonoBehaviour
    {
        public static PlaytimeTracker Instance { get; private set; }

        private float sessionStartTime;
        private float totalPlaytime;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            sessionStartTime = Time.realtimeSinceStartup;
        }

        /// <summary>
        /// Get total playtime in seconds (including loaded time)
        /// </summary>
        public float GetTotalPlaytime()
        {
            float sessionTime = Time.realtimeSinceStartup - sessionStartTime;
            return totalPlaytime + sessionTime;
        }

        /// <summary>
        /// Set the total playtime (when loading a save)
        /// </summary>
        public void SetTotalPlaytime(float playtime)
        {
            totalPlaytime = playtime;
            sessionStartTime = Time.realtimeSinceStartup;
        }

        /// <summary>
        /// Reset playtime (for new game)
        /// </summary>
        public void ResetPlaytime()
        {
            totalPlaytime = 0f;
            sessionStartTime = Time.realtimeSinceStartup;
        }
    }
}