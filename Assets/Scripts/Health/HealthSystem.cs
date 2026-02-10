using System;
using UnityEngine;

namespace Havengard.HealthSystem
{
    /// <summary>
    /// Pure data/logic health system (no Unity component dependencies beyond Mathf).
    /// Supports damage/heal, max HP scaling, normalized health, and death events.
    /// </summary>
    [Serializable]
    public class HealthSystem
    {
        [SerializeField] private int maxHealth;
        [SerializeField] private int currentHealth;

        // Fired when current/max changes (UI bars, etc.)
        public event Action<int, int> OnHealthChanged; // (current, max)

        // Fired when damage/heal is actually applied (positive amounts)
        public event Action<int> OnDamaged; // amount
        public event Action<int> OnHealed;  // amount

        // Fired once when HP reaches 0
        public event Action OnDeath;

        private bool deathFired;

        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public bool IsDead => currentHealth <= 0;
        public bool IsAlive => currentHealth > 0; // NEW: Added for MeleeAbility compatibility

        // Backwards compat helpers (if older code calls these)
        public int GetHealth() => currentHealth;
        public int GetMaxHealth() => maxHealth;
        public int Current => currentHealth;
        public int Max => maxHealth;

        public HealthSystem(int maxHealth)
        {
            this.maxHealth = Mathf.Max(1, maxHealth);
            currentHealth = this.maxHealth;
            deathFired = false;

            OnHealthChanged?.Invoke(currentHealth, this.maxHealth);
        }

        public float GetHealthNormalized()
        {
            if (maxHealth <= 0) return 0f;
            return Mathf.Clamp01((float)currentHealth / maxHealth);
        }

        /// <summary>Applies damage. Returns actual damage applied.</summary>
        public int Damage(int amount)
        {
            if (amount <= 0)
            {
                // no-op
                return 0;
            }

            if (IsDead) return 0;

            int before = currentHealth;
            currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
            int applied = before - currentHealth;

            if (applied > 0)
                OnDamaged?.Invoke(applied);

            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= 0 && !deathFired)
            {
                deathFired = true;
                OnDeath?.Invoke();
            }

            return applied;
        }

        /// <summary>Applies healing. Returns actual heal applied.</summary>
        public int Heal(int amount)
        {
            if (amount <= 0) return 0;
            if (IsDead) return 0;

            int before = currentHealth;
            currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
            int applied = currentHealth - before;

            if (applied > 0)
                OnHealed?.Invoke(applied);

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            return applied;
        }

        /// <summary>
        /// Sets max health, optionally refilling. Preserves percent by default.
        /// </summary>
        public void SetMaxHealth(int newMax, bool refill = false)
        {
            float percent = maxHealth > 0 ? (float)currentHealth / maxHealth : 1f;

            maxHealth = Mathf.Max(1, newMax);

            if (refill) currentHealth = maxHealth;
            else currentHealth = Mathf.RoundToInt(maxHealth * percent);

            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            // If we raised max HP above 0 and have HP now, allow death to fire again later
            if (currentHealth > 0)
                deathFired = false;

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>Force-set current HP. Returns the delta (new - old).</summary>
        public int SetHealth(int newHealth)
        {
            int before = currentHealth;
            currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);

            int delta = currentHealth - before;
            if (delta < 0) OnDamaged?.Invoke(-delta);
            else if (delta > 0) OnHealed?.Invoke(delta);

            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= 0 && !deathFired)
            {
                deathFired = true;
                OnDeath?.Invoke();
            }
            if (currentHealth > 0)
            {
                deathFired = false;
            }

            return delta;
        }
    }
}
