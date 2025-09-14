/*using UnityEngine;

namespace Havengard.Health
{
    public class HealthSystem : MonoBehaviour, IHealth
    {
        [SerializeField] private float maxHealth = 100f;
        private float currentHealth;

        private FactionProvider factionProvider;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;

        public event System.Action OnDamaged;
        public event System.Action OnHealed;
        public event System.Action OnDeath;

        private void Awake()
        {
            currentHealth = maxHealth;
            factionProvider = GetComponent<FactionProvider>();
        }

        public void TakeDamage(float amount)
        {
            if (currentHealth <= 0) return;

            currentHealth -= amount;
            OnDamaged?.Invoke();

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                OnDeath?.Invoke();
            }
        }

        public void Heal(float amount)
        {
            if (currentHealth <= 0) return;

            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            OnHealed?.Invoke();
        }

        public Faction GetFaction()
        {
            return factionProvider != null ? factionProvider.Faction : Faction.Neutral;
        }
    }
}
*/
