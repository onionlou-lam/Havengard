using System.Collections;
using UnityEngine;

namespace Havengard.Health
{
    [RequireComponent(typeof(FactionProvider))]
    public class Health : MonoBehaviour, IHealth
    {
        [SerializeField] private float maxHealth = 100f;
        private float currentHealth;
        private FactionProvider factionProvider;

        public event System.Action<float> OnDamaged;
        public event System.Action<float> OnHealed;
        public event System.Action OnDeath;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;

        private void Awake()
        {
            currentHealth = maxHealth;
            factionProvider = GetComponent<FactionProvider>();
        }

        public void TakeDamage(float amount)
        {
            if (amount <= 0f) return;
            if (currentHealth <= 0f) return;

            currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);
            OnDamaged?.Invoke(amount);

            if (currentHealth <= 0f)
            {
                currentHealth = 0f;
                OnDeath?.Invoke();
            }
        }

        public void Heal(float amount)
        {
            if (amount <= 0f) return;
            if (currentHealth <= 0f) return; // dead units can't be healed here (changeable)

            currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
            OnHealed?.Invoke(amount);
        }

        public Faction GetFaction()
        {
            return factionProvider != null ? factionProvider.Faction : Faction.Neutral;
        }

        public void ApplyDoT(float totalDamage, float duration, float tickInterval = 1f)
        {
            if (totalDamage <= 0f || duration <= 0f) return;
            StartCoroutine(DoTCoroutine(totalDamage, duration, tickInterval));
        }

        private IEnumerator DoTCoroutine(float totalDamage, float duration, float tickInterval)
        {
            float ticks = Mathf.Max(1, Mathf.Floor(duration / tickInterval));
            float damagePerTick = totalDamage / ticks;

            float elapsed = 0f;
            while (elapsed < duration && currentHealth > 0f)
            {
                yield return new WaitForSeconds(tickInterval);
                TakeDamage(damagePerTick);
                elapsed += tickInterval;
            }
        }
    }
}
