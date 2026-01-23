using UnityEngine;
using Havengard.HealthSystem;
using Havengard.Heroes;
using Havengard.Items;

namespace Havengard.Units.Enemies
{
    /// <summary>
    /// Handles all enemy death rewards including EXP, currency, and items.
    /// Consolidates EnemyRewards functionality.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class EnemyDeathRewards : MonoBehaviour
    {
        [Header("Rewards")]
        [SerializeField] private int expValue = 20;
        [SerializeField] private int goldValue = 5;
        [SerializeField] private int celestiumValue = 0;
        
        private Health health;
        private ItemDropper itemDropper;

        private void Awake()
        {
            health = GetComponent<Health>();
            itemDropper = GetComponent<ItemDropper>();
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
            // Grant EXP to player
            HeroInstance player = FindPlayerHero();
            if (player != null)
            {
                Debug.Log($"[EnemyDeathRewards] {name} died. Granting {expValue} EXP to {player.name}.");
                player.GrantEXP(expValue);
            }
            else
            {
                Debug.LogWarning("[EnemyDeathRewards] No player HeroInstance found (is the player tagged 'Player'?).");
            }

            // Grant Gold
            if (goldValue > 0 && Havengard.Resources.GoldSystem.Instance != null)
            {
                Havengard.Resources.GoldSystem.Instance.AddGold(goldValue);
            }

            // Grant Celestium
            if (celestiumValue > 0 && Havengard.Resources.CelestiumSystem.Instance != null)
            {
                Havengard.Resources.CelestiumSystem.Instance.AddCelestium(celestiumValue);
            }

            Debug.Log($"[EnemyDeathRewards] {name} rewards: EXP={expValue}, Gold={goldValue}, Celestium={celestiumValue}");

            // Handle item drops
            HandleItemDrop();
        }

        private void HandleItemDrop()
        {
            if (itemDropper != null)
            {
                itemDropper.TryDropItem(transform.position);
            }
        }

        private HeroInstance FindPlayerHero()
        {
            foreach (var hero in Object.FindObjectsByType<HeroInstance>(FindObjectsSortMode.None))
            {
                if (hero != null && hero.CompareTag("Player"))
                    return hero;
            }
            return null;
        }

        // Public properties for inspector visibility
        public int ExpValue => expValue;
        public int GoldValue => goldValue;
        public int CelestiumValue => celestiumValue;
    }
}
