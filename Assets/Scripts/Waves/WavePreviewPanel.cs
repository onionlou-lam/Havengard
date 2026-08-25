using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace Havengard.Waves.UI
{
    /// <summary>
    /// Displays condensed wave preview with timer and start button
    /// </summary>
    public class WavePreviewPanel : MonoBehaviour
    {
        [Header("Panel References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TextMeshProUGUI waveNumberText;
        [SerializeField] private TextMeshProUGUI totalEnemiesText;

        [Header("Enemy List")]
        [SerializeField] private Transform enemyListContainer;
        [SerializeField] private GameObject enemyPreviewItemPrefab;

        [Header("Timer & Button")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Button startWaveButton;
        [SerializeField] private TextMeshProUGUI startButtonText;
        [SerializeField] private GameObject timerObject; // Hide if no timer

        [Header("Rewards (Compact)")]
        [SerializeField] private TextMeshProUGUI rewardsText; // Single line: "💰50  ⭐25  💎10"

        [Header("Animation")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.2f;

        [Header("Integration")]
        [SerializeField] private Havengard.Waves.PreWavePhase preWavePhase;

        private CanvasGroup canvasGroup;
        private List<GameObject> spawnedEnemyItems = new List<GameObject>();
        private Coroutine fadeCoroutine;
        private Coroutine timerCoroutine;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            // Auto-find PreWavePhase
            if (preWavePhase == null)
                preWavePhase = FindFirstObjectByType<Havengard.Waves.PreWavePhase>();

            // Hook up start button
            if (startWaveButton != null)
            {
                startWaveButton.onClick.AddListener(OnStartWaveClicked);
            }

            if (panelRoot != null)
                panelRoot.SetActive(false);
        }

        /// <summary>
        /// Show wave preview
        /// </summary>
        public void ShowPreview(WavePreviewData data, bool hasTimer, float timerDuration)
        {
            if (data == null)
            {
                Debug.LogWarning("[WavePreviewPanel] Cannot show null preview data");
                return;
            }

            // Clear previous enemy items
            ClearEnemyList();

            // Set wave info (compact)
            if (waveNumberText != null)
                waveNumberText.text = $"Wave {data.waveNumber}: {data.waveName}";

            if (totalEnemiesText != null)
                totalEnemiesText.text = $"{data.totalEnemyCount} Enemies";

            // Populate enemy list (only show top 5 most common)
            var topEnemies = GetTopEnemies(data.enemies, 5);
            foreach (var enemy in topEnemies)
            {
                CreateEnemyPreviewItem(enemy);
            }

            // Set rewards (compact single line)
            if (rewardsText != null)
            {
                string rewardString = "";
                if (data.goldReward > 0) rewardString += $"💰{data.goldReward}  ";
                if (data.expReward > 0) rewardString += $"⭐{data.expReward}  ";
                if (data.celestiumReward > 0) rewardString += $"💎{data.celestiumReward}";
                rewardsText.text = rewardString.TrimEnd();
            }

            // Setup timer
            if (timerObject != null)
                timerObject.SetActive(hasTimer);

            if (hasTimer && timerDuration > 0)
            {
                if (timerCoroutine != null)
                    StopCoroutine(timerCoroutine);
                timerCoroutine = StartCoroutine(UpdateTimerRoutine(timerDuration));
            }
            else if (timerText != null)
            {
                timerText.text = "";
            }

            // Update button text
            if (startButtonText != null)
            {
                startButtonText.text = hasTimer ? "Start Wave Now" : "Start Wave";
            }

            // Show panel with fade in
            if (panelRoot != null)
                panelRoot.SetActive(true);

            FadeIn();
        }

        /// <summary>
        /// Hide wave preview
        /// </summary>
        public void HidePreview()
        {
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                timerCoroutine = null;
            }

            FadeOut(() =>
            {
                if (panelRoot != null)
                    panelRoot.SetActive(false);
            });
        }

        private void OnStartWaveClicked()
        {
            if (preWavePhase != null)
            {
                preWavePhase.ManuallyStartWave();
            }
        }

        private IEnumerator UpdateTimerRoutine(float duration)
        {
            float remaining = duration;

            while (remaining > 0)
            {
                remaining -= Time.deltaTime;

                if (timerText != null)
                {
                    int seconds = Mathf.CeilToInt(remaining);
                    timerText.text = $"Auto-start in: {seconds}s";

                    // Change color when time is running out
                    if (seconds <= 5)
                        timerText.color = Color.red;
                    else if (seconds <= 10)
                        timerText.color = Color.yellow;
                    else
                        timerText.color = Color.white;
                }

                yield return null;
            }

            if (timerText != null)
                timerText.text = "Starting...";

            timerCoroutine = null;
        }

        private List<WavePreviewData.EnemyPreview> GetTopEnemies(List<WavePreviewData.EnemyPreview> enemies, int maxCount)
        {
            if (enemies == null || enemies.Count <= maxCount)
                return enemies;

            // Sort by count descending
            var sorted = new List<WavePreviewData.EnemyPreview>(enemies);
            sorted.Sort((a, b) => b.count.CompareTo(a.count));

            return sorted.GetRange(0, Mathf.Min(maxCount, sorted.Count));
        }

        private void CreateEnemyPreviewItem(WavePreviewData.EnemyPreview enemy)
        {
            if (enemyPreviewItemPrefab == null || enemyListContainer == null)
            {
                Debug.LogWarning("[WavePreviewPanel] Enemy preview item prefab or container not assigned!");
                return;
            }

            GameObject item = Instantiate(enemyPreviewItemPrefab, enemyListContainer);
            spawnedEnemyItems.Add(item);

            // Set up the item
            var itemComponent = item.GetComponent<WavePreviewEnemyItem>();
            if (itemComponent != null)
            {
                itemComponent.Setup(enemy);
            }
        }

        private void ClearEnemyList()
        {
            foreach (var item in spawnedEnemyItems)
            {
                if (item != null)
                    Destroy(item);
            }
            spawnedEnemyItems.Clear();
        }

        private void FadeIn()
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            fadeCoroutine = StartCoroutine(FadeCoroutine(0f, 1f, fadeInDuration));
        }

        private void FadeOut(System.Action onComplete = null)
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            fadeCoroutine = StartCoroutine(FadeCoroutine(1f, 0f, fadeOutDuration, onComplete));
        }

        private IEnumerator FadeCoroutine(float from, float to, float duration, System.Action onComplete = null)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                canvasGroup.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }

            canvasGroup.alpha = to;
            onComplete?.Invoke();
            fadeCoroutine = null;
        }
    }
}