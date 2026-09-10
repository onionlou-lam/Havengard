using UnityEngine;
using System;

namespace Havengard.Core.HealthManagement
{
    /// <summary>
    /// Core health system logic (non-MonoBehaviour)
    /// Can be used by any entity that needs health tracking
    /// </summary>
    [System.Serializable]
    public class HealthSystem
    {
        // Events
        public event Action<int> OnDamaged;
        public event Action<int> OnHealed;
        public event Action<int, int> OnHealthChanged;
        public event Action OnDeath;

        // State
        private int currentHealth;
        private int maxHealth;
        private bool isDead;

        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public bool IsDead => isDead;

        public HealthSystem(int initialMaxHealth)
        {
            maxHealth = Mathf.Max(1, initialMaxHealth);
            currentHealth = maxHealth;
            isDead = false;
        }

        /// <summary>
        /// Apply damage and return actual damage dealt
        /// </summary>
        public int Damage(int amount)
        {
            if (isDead || amount <= 0) return 0;

            int actualDamage = Mathf.Min(amount, currentHealth);
            currentHealth -= actualDamage;

            OnDamaged?.Invoke(actualDamage);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= 0 && !isDead)
            {
                isDead = true;
                OnDeath?.Invoke();
            }

            return actualDamage;
        }

        /// <summary>
        /// Apply healing and return actual healing done
        /// </summary>
        public int Heal(int amount)
        {
            if (isDead || amount <= 0) return 0;

            int actualHealing = Mathf.Min(amount, maxHealth - currentHealth);
            currentHealth += actualHealing;

            OnHealed?.Invoke(actualHealing);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            return actualHealing;
        }

        /// <summary>
        /// Set max health (optionally refills to new max)
        /// </summary>
        public void SetMaxHealth(int newMax, bool refill = false)
        {
            maxHealth = Mathf.Max(1, newMax);

            if (refill)
            {
                currentHealth = maxHealth;
            }
            else
            {
                currentHealth = Mathf.Min(currentHealth, maxHealth);
            }

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>
        /// Increases max health by the specified amount (preserves current health percentage)
        /// </summary>
        public void IncreaseMaxHealth(int amount)
        {
            if (amount <= 0) return;

            int oldMax = maxHealth;
            int newMax = maxHealth + amount;
            
            maxHealth = newMax;
            
            // Increase current health proportionally to maintain the same percentage
            // OR you can choose to add the amount to current health as well
            // Option 1: Maintain percentage (recommended for stat increases)
            float healthPercentage = (float)currentHealth / oldMax;
            currentHealth = Mathf.RoundToInt(newMax * healthPercentage);
            
            // Option 2: Add to current health (uncomment if you prefer this)
            // currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>
        /// Returns health percentage (0-1)
        /// </summary>
        public float GetHealthPercentage()
        {
            return maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
        }

        /// <summary>
        /// Returns health normalized (0-1) - Alias for GetHealthPercentage for backwards compatibility
        /// </summary>
        public float GetHealthNormalized()
        {
            return GetHealthPercentage();
        }
    }
}