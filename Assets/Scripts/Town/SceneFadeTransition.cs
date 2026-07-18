using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

namespace Havengard.Town
{
    /// <summary>
    /// Handles smooth fade transitions between scenes.
    /// Attach to a Canvas with a full-screen black Image.
    /// </summary>
    public class SceneFadeTransition : MonoBehaviour
    {
        public static SceneFadeTransition Instance { get; private set; }

        [SerializeField] private Image fadeImage;
        [SerializeField] private float fadeDuration = 0.5f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (fadeImage != null)
            {
                fadeImage.color = new Color(0, 0, 0, 0);
            }
        }

        public void LoadSceneWithFade(string sceneName)
        {
            StartCoroutine(FadeAndLoad(sceneName));
        }

        private IEnumerator FadeAndLoad(string sceneName)
        {
            // Fade out
            yield return StartCoroutine(Fade(0f, 1f));

            // Load scene
            SceneManager.LoadScene(sceneName);

            // Fade in
            yield return StartCoroutine(Fade(1f, 0f));
        }

        private IEnumerator Fade(float startAlpha, float endAlpha)
        {
            if (fadeImage == null) yield break;

            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);
                fadeImage.color = new Color(0, 0, 0, alpha);
                yield return null;
            }

            fadeImage.color = new Color(0, 0, 0, endAlpha);
        }
    }
}