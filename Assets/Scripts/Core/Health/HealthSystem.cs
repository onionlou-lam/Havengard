using System;
using UnityEngine;

namespace Havengard.Core.HealthSystem
{
    public class HealthSystem
    {
        private int currentHealth;
        private int maxHealth;

        public event Action<int> OnDamaged;
        public event Action<int> OnHealed;
        public event Action<int, int> OnHealthChanged;
        public event Action OnDeath;

        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public bool IsDead => currentHealth <= 0;

        public HealthSystem(int maxHP)
        {
            maxHealth = Mathf.Max(1, maxHP);
            currentHealth = maxHealth;
        }

        public int Damage(int amount)
        {
            if (IsDead) return 0;

            int actualDamage = Mathf.Min(amount, currentHealth);
            currentHealth -= actualDamage;

            OnDamaged?.Invoke(actualDamage);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (IsDead)
            {
                OnDeath?.Invoke();
            }

            return actualDamage;
        }

        public int Heal(int amount)
        {
            if (IsDead) return 0;

            int actualHeal = Mathf.Min(amount, maxHealth - currentHealth);
            currentHealth += actualHeal;

            OnHealed?.Invoke(actualHeal);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            return actualHeal;
        }

        public void SetMaxHealth(int newMax, bool refillCurrent = true)
        {
            maxHealth = Mathf.Max(1, newMax);

            if (refillCurrent)
            {
                currentHealth = maxHealth;
            }
            else
            {
                currentHealth = Mathf.Min(currentHealth, maxHealth);
            }

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        // ADD THIS METHOD for stat allocation system
        public void IncreaseMaxHealth(int amount)
        {
            maxHealth += amount;
            currentHealth += amount; // Also heal by the same amount
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public float GetHealthNormalized()
        {
            return maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
        }

        public void RestoreToFull()
        {
            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }
}