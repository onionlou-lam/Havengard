using System;
using UnityEngine;

namespace Havengard.HealthSystem
{
    public enum Faction
    {
        Neutral,
        Player,
        Enemy
    }
    [RequireComponent(typeof(Collider2D))]
    public class Health : MonoBehaviour, IHealth
    {
        [Header("Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private Faction faction = Faction.Neutral;

        private HealthSystem healthSystem;

        public event Action OnDamaged;
        public event Action OnHealed;
        public event Action OnDeath;

        public Faction GetFaction() => faction;
        public HealthSystem GetHealthSystem() => healthSystem;

        private void Awake()
        {
            healthSystem = new HealthSystem(maxHealth);

            healthSystem.OnDamaged += (s, e) => OnDamaged?.Invoke();
            healthSystem.OnHealed += (s, e) => OnHealed?.Invoke();
            healthSystem.OnDead += (s, e) =>
            {
                OnDeath?.Invoke();
                Destroy(gameObject); // You can swap this for animation/respawn
            };
        }

        public void TakeDamage(float amount, Faction sourceFaction)
        {
            if (sourceFaction == faction) return; // ?? Prevent friendly fire
            healthSystem.Damage(amount);
        }

        public void Heal(float amount) => healthSystem.Heal(amount);
    }
}
