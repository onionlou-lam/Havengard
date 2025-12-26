using UnityEngine;
using Havengard.HealthSystem;
using Havengard.Heroes;
using Havengard.Resources;

namespace Havengard.Units
{
    [RequireComponent(typeof(Health))]
    public class EnemyRewards : MonoBehaviour
    {
        [Header("Rewards")]
        [SerializeField] private int expValue = 20;
        [SerializeField] private int goldValue = 0;
        [SerializeField] private int celestiumValue = 0;

        public int ExpValue => expValue;
        public int GoldValue => goldValue;
        public int CelestiumValue => celestiumValue;

        private Health health;

        private void Awake()
        {
            health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            if (health != null)
                health.OnDeath += HandleDeath;
        }

        private void OnDisable()
        {
            if (health != null)
                health.OnDeath -= HandleDeath;
        }

        private void HandleDeath()
        {
            // --- EXP to player hero ---
            HeroInstance player = FindPlayerHero();
            if (player != null)
            {
                player.GrantEXP(expValue);
            }
            else
            {
                Debug.LogWarning("[EnemyRewards] No HeroInstance found for EXP reward.");
            }

            // --- Gold (shared pool) ---
            if (goldValue > 0 && GoldSystem.Instance != null)
                GoldSystem.Instance.AddGold(goldValue);

            // --- Celestium (shared pool) ---
            if (celestiumValue > 0 && CelestiumSystem.Instance != null)
                CelestiumSystem.Instance.AddCelestium(celestiumValue);

            Debug.Log($"[EnemyRewards] {name} rewards: EXP={expValue}, Gold={goldValue}, Celestium={celestiumValue}");
        }

        private HeroInstance FindPlayerHero()
        {
            // Prefer FindObjectsByType (newer Unity) with no sorting for speed.
#if UNITY_2023_1_OR_NEWER
            var heroes = Object.FindObjectsByType<HeroInstance>(FindObjectsSortMode.None);
#else
            var heroes = FindObjectsOfType<HeroInstance>();
#endif
            foreach (var hero in heroes)
            {
                if (hero != null && hero.CompareTag("Player"))
                    return hero;
            }

            return null;
        }
    }
}
