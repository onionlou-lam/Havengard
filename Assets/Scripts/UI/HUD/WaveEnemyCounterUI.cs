using UnityEngine;
using TMPro;
using Havengard.Waves;

namespace Havengard.UI
{
    /// <summary>
    /// HUD widget shown during the Pre-Wave phase and active wave that displays
    /// the current wave title and a live count of enemies remaining
    /// (counts down from the total expected as enemies die).
    /// </summary>
    public class WaveEnemyCounterUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private WaveManager waveManager;

        [Header("UI (TMP)")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text waveTitleText;
        [SerializeField] private TMP_Text enemyCountText;

        [Header("Formatting")]
        [SerializeField] private string waveTitleFormat = "Wave {0}";
        [SerializeField] private string enemyCountFormat = "Enemies Remaining: {0}/{1}";

        private void Awake()
        {
            if (waveManager == null)
                waveManager = FindFirstObjectByType<WaveManager>();
        }

        private void OnEnable()
        {
            if (waveManager != null)
            {
                waveManager.waveEvents.OnWaveStarted.AddListener(HandleWaveStarted);
                waveManager.waveEvents.OnEnemyCountChanged.AddListener(HandleEnemyCountChanged);
                waveManager.waveEvents.OnWaveCleared.AddListener(HandleWaveCleared);
            }
        }

        private void OnDisable()
        {
            if (waveManager != null)
            {
                waveManager.waveEvents.OnWaveStarted.RemoveListener(HandleWaveStarted);
                waveManager.waveEvents.OnEnemyCountChanged.RemoveListener(HandleEnemyCountChanged);
                waveManager.waveEvents.OnWaveCleared.RemoveListener(HandleWaveCleared);
            }
        }

        private void HandleWaveStarted(int waveIndex)
        {
            if (panel != null)
                panel.SetActive(true);

            if (waveTitleText != null)
                waveTitleText.text = string.Format(waveTitleFormat, waveIndex + 1);
        }

        private void HandleEnemyCountChanged(int remaining, int total)
        {
            if (enemyCountText != null)
                enemyCountText.text = string.Format(enemyCountFormat, remaining, total);
        }

        private void HandleWaveCleared(int waveIndex)
        {
            if (enemyCountText != null)
                enemyCountText.text = string.Format(enemyCountFormat, 0, 0);
        }

        /// <summary>
        /// Manually show/hide the counter (e.g. hidden entirely outside of combat).
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (panel != null)
                panel.SetActive(visible);
        }
    }
}