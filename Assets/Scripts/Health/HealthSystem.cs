using UnityEngine;
using System;

namespace Havengard.HealthSystem
{
    /// <summary>
    /// A flexible health system that supports taking damage, healing, max HP scaling, and death events.
    /// Used by all units (Player, Heroes, Allies, Enemies).
    /// </summary>
    [Serializable]
    public class HealthSystem
    {
        [SerializeField] private int maxHealth;
        [SerializeField] private int currentHealth;

        public event Action OnHealthChanged;
        public event Action OnDeath;

        // --- Compatibility / Public API ---
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;

        public int GetHealth() => currentHealth;
        public int GetMaxHealth() => maxHealth;

        public float GetHealthNormalized()
        {
            if (maxHealth <= 0) return 0f;
            return Mathf.Clamp01((float)currentHealth / maxHealth);
        }
        public int Current => currentHealth;
        public int Max => maxHealth;
        public HealthSystem(int maxHealth)
        {
            this.maxHealth = Mathf.Max(1, maxHealth);
            currentHealth = this.maxHealth;
        }

        public void Damage(int amount)
        {
            if (amount <= 0) { OnHealthChanged?.Invoke(); return; }

            currentHealth -= amount;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            OnHealthChanged?.Invoke();

            if (currentHealth <= 0)
                OnDeath?.Invoke();
        }

        public void Heal(int amount)
        {
            if (amount <= 0) return;

            currentHealth += amount;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            OnHealthChanged?.Invoke();
        }

        public void SetMaxHealth(int newMax, bool refill = false)
        {
            float percent = maxHealth > 0 ? (float)currentHealth / maxHealth : 1f;

            maxHealth = Mathf.Max(1, newMax);

            if (refill) currentHealth = maxHealth;
            else currentHealth = Mathf.RoundToInt(maxHealth * percent);

            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            OnHealthChanged?.Invoke();
        }
    }
}
