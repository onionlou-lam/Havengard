using UnityEngine;
using Havengard.HealthSystem;
using Havengard.Heroes;

namespace Havengard.Units
{
    [RequireComponent(typeof(Health))]
    public class EnemyRewards : MonoBehaviour
    {
        [Header("Rewards")]
        [SerializeField] private int expValue = 20;
        [SerializeField] private int goldValue = 0;

        // Standardised public read-only accessors used by other scripts
        public int ExpValue => expValue;
        public int GoldValue => goldValue;

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
            Debug.Log($"[EnemyRewards] {name} died. Granting {expValue} EXP.");

            // Find player hero instance
            HeroInstance player = FindPlayerHero();
            if (player == null)
            {
                Debug.LogWarning("[EnemyRewards] No HeroInstance found for EXP reward.");
                return;
            }

            player.GrantEXP(expValue);
        }

        private HeroInstance FindPlayerHero()
        {
            // Simple + safe for now
            foreach (var hero in FindObjectsOfType<HeroInstance>())
            {
                if (hero.CompareTag("Player"))
                    return hero;
            }

            return null;
        }
    }
}
