using UnityEngine;
using UnityEngine.SceneManagement;

namespace Havengard.Core
{
    /// <summary>
    /// Handles loading Phase 1 (city) and Phase 2 (defence) scenes.
    /// Keeps GameManager alive across loads.
    /// </summary>
    public class SceneTransitionManager : MonoBehaviour
    {
        public static SceneTransitionManager Instance { get; private set; }

        [SerializeField] private string citySceneName = "CityScene";
        [SerializeField] private string defenceSceneName = "DefenceScene";

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

        public void LoadCityScene()
        {
            SceneManager.LoadScene(citySceneName);
        }

        public void LoadDefenceScene()
        {
            SceneManager.LoadScene(defenceSceneName);
        }
    }
}
