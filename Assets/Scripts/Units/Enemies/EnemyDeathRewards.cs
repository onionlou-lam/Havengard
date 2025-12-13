using UnityEngine;
using Havengard.HealthSystem;
using Havengard.Heroes;

namespace Havengard.Units.Enemies
{
    [RequireComponent(typeof(Health))]
    public class EnemyDeathRewards : MonoBehaviour
    {
        private Health health;
        private EnemyRewards rewards;

        private void Awake()
        {
            health = GetComponent<Health>();
            rewards = GetComponent<EnemyRewards>();
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
            if (rewards == null)
            {
                Debug.LogWarning($"[EnemyDeathRewards] {name} has no EnemyRewards component. No EXP granted.");
                return;
            }

            // Find the player hero instance (tagged Player)
            HeroInstance player = FindPlayerHero();
            if (player == null)
            {
                Debug.LogWarning("[EnemyDeathRewards] No player HeroInstance found (is the player tagged 'Player'?).");
                return;
            }

            int exp = rewards.ExpValue;
            Debug.Log($"[EnemyDeathRewards] {name} died. Granting {exp} EXP to {player.name}.");
            player.GrantEXP(exp);
        }

        private HeroInstance FindPlayerHero()
        {
            // Simple and reliable for now
            foreach (var hero in Object.FindObjectsByType<HeroInstance>(FindObjectsSortMode.None))
            {
                if (hero != null && hero.CompareTag("Player"))
                    return hero;
            }
            return null;
        }
    }
}
