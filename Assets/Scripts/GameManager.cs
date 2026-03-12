using UnityEngine;
using System.Collections.Generic;
using Havengard.Resources;
using Havengard.Core.Heroes;

namespace Havengard.Core
{
    /// <summary>
    /// Persistent manager that survives scene loads.
    /// Stores team-wide data (gold, celestium, recruited heroes).
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Resources")]
        public GoldSystem goldSystem;
        public CelestiumSystem celestiumSystem;

        [Header("Hero Roster")]
        public List<HeroInstance> recruitedHeroes = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Ensure resource systems exist
            if (goldSystem == null)
            {
                goldSystem = gameObject.AddComponent<GoldSystem>();
            }
            if (celestiumSystem == null)
            {
                celestiumSystem = gameObject.AddComponent<CelestiumSystem>();
            }
        }
        private void Start()
        {
            if (DayNightCycleSystem.Instance != null)
            {
                DayNightCycleSystem.Instance.OnDayStarted += HandleDayStart;
                DayNightCycleSystem.Instance.OnNightEnded += HandleNightEnd;
            }
        }
        private void HandleDayStart(int day)
        {
            // Add daily passive Celestium
            celestiumSystem.AddCelestium(1);

            // Add gold income from houses (Phase 1 teammate’s system)
            // GoldSystem.Instance.AddGold(cityEconomy.GetDailyIncome());
        }

        private void HandleNightEnd(int day)
        {
            // Reset wave counters, prep next day
        }

        public void RegisterHero(HeroInstance hero)
        {
            if (!recruitedHeroes.Contains(hero))
                recruitedHeroes.Add(hero);
        }

        public void UnregisterHero(HeroInstance hero)
        {
            if (recruitedHeroes.Contains(hero))
                recruitedHeroes.Remove(hero);
        }
    }
}
