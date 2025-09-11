using System;
using UnityEngine;

namespace Havengard.HealthSystem
{
    /// <summary>
    /// Core Health System logic.
    /// Manages damage, healing, death, and broadcasts events for UI or gameplay systems.
    /// </summary>
    public class HealthSystem
    {
        public event EventHandler OnHealthChanged;
        public event EventHandler OnHealthMaxChanged;
        public event EventHandler OnDamaged;
        public event EventHandler OnHealed;
        public event EventHandler OnDead;

        private float healthMax;
        private float health;

        public HealthSystem(float healthMax)
        {
            this.healthMax = healthMax;
            health = healthMax;
        }

        public float GetHealth() => health;
        public float GetHealthMax() => healthMax;
        public float GetHealthNormalized() => healthMax > 0 ? health / healthMax : 0f;

        public void Damage(float amount)
        {
            if (IsDead()) return;

            health = Mathf.Max(0, health - amount);
            OnHealthChanged?.Invoke(this, EventArgs.Empty);
            OnDamaged?.Invoke(this, EventArgs.Empty);

            if (health <= 0) Die();
        }

        public void Heal(float amount)
        {
            if (IsDead()) return;

            health = Mathf.Min(healthMax, health + amount);
            OnHealthChanged?.Invoke(this, EventArgs.Empty);
            OnHealed?.Invoke(this, EventArgs.Empty);
        }

        public void HealToFull()
        {
            if (IsDead()) return;

            health = healthMax;
            OnHealthChanged?.Invoke(this, EventArgs.Empty);
            OnHealed?.Invoke(this, EventArgs.Empty);
        }

        public void SetHealthMax(float newMax, bool resetToFull = true)
        {
            healthMax = newMax;
            if (resetToFull) health = healthMax;

            OnHealthMaxChanged?.Invoke(this, EventArgs.Empty);
            OnHealthChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SetHealth(float newHealth)
        {
            health = Mathf.Clamp(newHealth, 0, healthMax);
            OnHealthChanged?.Invoke(this, EventArgs.Empty);

            if (health <= 0) Die();
        }

        public bool IsDead() => health <= 0;

        private void Die()
        {
            health = 0;
            OnDead?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Utility: tries to retrieve a HealthSystem from a GameObject.
        /// </summary>
        public static bool TryGet(GameObject obj, out HealthSystem healthSystem, bool logErrors = false)
        {
            healthSystem = null;

            if (obj == null)
            {
                if (logErrors) Debug.LogError("HealthSystem.TryGet failed: target GameObject is null.");
                return false;
            }

            if (obj.TryGetComponent(out IHealth provider))
            {
                healthSystem = provider.GetHealthSystem();
                if (healthSystem != null) return true;

                if (logErrors) Debug.LogError($"HealthSystem on '{obj.name}' is null. Check initialization order.");
                return false;
            }

            if (logErrors) Debug.LogError($"GameObject '{obj.name}' does not provide a HealthSystem.");
            return false;
        }
    }
}
