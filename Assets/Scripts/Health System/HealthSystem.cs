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
        private int maxHealth;
        private int currentHealth;

        public event Action OnHealthChanged;
        public event Action OnDeath;

        public HealthSystem(int maxHealth)
        {
            this.maxHealth = Mathf.Max(1, maxHealth);
            currentHealth = this.maxHealth;
        }

        // ---- Queries ----
        public int GetHealth() => currentHealth;
        public int GetMaxHealth() => maxHealth;
        public float GetHealthNormalized() => (float)currentHealth / maxHealth;

        // ---- Modification ----
        public void Damage(int amount)
        {
            if (amount <= 0) return;

            currentHealth -= amount;
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                OnHealthChanged?.Invoke();
                OnDeath?.Invoke();
            }
            else
            {
                OnHealthChanged?.Invoke();
            }
        }

        public void Heal(int amount)
        {
            if (amount <= 0) return;

            currentHealth += amount;
            if (currentHealth > maxHealth) currentHealth = maxHealth;
            OnHealthChanged?.Invoke();
        }

        /// <summary>
        /// Directly sets the current health (clamped).
        /// </summary>
        public void SetHealth(int newValue)
        {
            currentHealth = Mathf.Clamp(newValue, 0, maxHealth);
            OnHealthChanged?.Invoke();
            if (currentHealth == 0)
                OnDeath?.Invoke();
        }

        /// <summary>
        /// Changes the maximum health. Optionally refill to full HP or preserve % HP.
        /// </summary>
        public void SetMaxHealth(int newMax, bool refill = true)
        {
            if (newMax < 1) newMax = 1;

            float percent = (maxHealth > 0) ? (float)currentHealth / maxHealth : 1f;
            maxHealth = newMax;

            if (refill)
                currentHealth = maxHealth;
            else
                currentHealth = Mathf.RoundToInt(maxHealth * percent);

            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            OnHealthChanged?.Invoke();
        }
    }
}
