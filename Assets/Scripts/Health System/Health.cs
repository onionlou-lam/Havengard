using UnityEngine;
using Havengard.Combat;
using System;

namespace Havengard.HealthSystem
{
    public class Health : MonoBehaviour, IHealth
    {
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private Faction faction = Faction.Enemy;

        private float currentHealth;

        // ✅ Events for UI / gameplay
        public event Action<float> OnDamaged; // sends damage amount
        public event Action<float> OnHealed;  // sends heal amount
        public event Action OnDeath;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        public Faction GetFaction() => faction;
        public float GetCurrentHealth() => currentHealth;
        public float GetMaxHealth() => maxHealth; // useful for UI bars

        public void TakeDamage(float amount, Faction sourceFaction)
        {
            // Prevent friendly fire
            if (sourceFaction == faction) return;

            currentHealth -= amount;
            OnDamaged?.Invoke(amount);

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        public void ApplyDoT(float totalDamage, float duration, Faction sourceFaction)
        {
            if (sourceFaction == faction) return;
            StartCoroutine(DoTDamage(totalDamage, duration));
        }

        private System.Collections.IEnumerator DoTDamage(float totalDamage, float duration)
        {
            float tickRate = 1f;
            float damagePerTick = totalDamage / duration * tickRate;

            for (float t = 0; t < duration; t += tickRate)
            {
                currentHealth -= damagePerTick;
                OnDamaged?.Invoke(damagePerTick);

                if (currentHealth <= 0f)
                {
                    Die();
                    yield break;
                }
                yield return new WaitForSeconds(tickRate);
            }
        }

        public void Heal(float amount)
        {
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            OnHealed?.Invoke(amount);
        }

        private void Die()
        {
            OnDeath?.Invoke();
            Debug.Log($"{gameObject.name} has died.");
            Destroy(gameObject);
        }
    }
}
