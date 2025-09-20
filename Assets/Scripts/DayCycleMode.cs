using UnityEngine;
using System;

namespace Havengard.Core
{
    public enum DayCycleMode { Manual, Timed }

    /// <summary>
    /// Handles transitions between Day (city build) and Night (defence).
    /// Default = Manual end of day, player chooses when to continue.
    /// </summary>
    public class DayNightCycleSystem : MonoBehaviour
    {
        public static DayNightCycleSystem Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private DayCycleMode mode = DayCycleMode.Manual;
        [SerializeField] private float dayDurationSeconds = 300f; // only used if mode == Timed

        public int CurrentDay { get; private set; } = 1;
        private float timer;

        public event Action<int> OnDayStarted;
        public event Action<int> OnDayEnded;
        public event Action<int> OnNightStarted;
        public event Action<int> OnNightEnded;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (DayNightCycleSystem.Instance != null)
            {
                DayNightCycleSystem.Instance.OnDayEnded += _ => SceneTransitionManager.Instance.LoadDefenceScene();
                DayNightCycleSystem.Instance.OnNightEnded += _ => SceneTransitionManager.Instance.LoadCityScene();
            }
        }

        private void Update()
        {
            if (mode == DayCycleMode.Timed && !IsNight())
            {
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    EndDay();
                }
            }
        }

        public void StartDay()
        {
            Debug.Log($"Day {CurrentDay} started.");
            timer = dayDurationSeconds;
            OnDayStarted?.Invoke(CurrentDay);
        }

        public void EndDay()
        {
            Debug.Log($"Day {CurrentDay} ended.");
            OnDayEnded?.Invoke(CurrentDay);

            StartNight();
        }

        public void StartNight()
        {
            Debug.Log($"Night {CurrentDay} started.");
            OnNightStarted?.Invoke(CurrentDay);

            // TODO: Load Defence Scene here
        }

        public void EndNight()
        {
            Debug.Log($"Night {CurrentDay} ended.");
            OnNightEnded?.Invoke(CurrentDay);

            CurrentDay++;
            StartDay();
        }

        public bool IsNight()
        {
            // Basic state check: Night is between StartNight and EndNight
            // You could expand this with an explicit bool flag if needed
            return false; // Placeholder if needed
        }

        // Manual override for player choice
        public void PlayerEndDayEarly()
        {
            if (!IsNight())
            {
                EndDay();
            }
        }
    }
}
