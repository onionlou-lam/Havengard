using UnityEngine;
using Havengard.Core.Character;
using Havengard.Core.HealthSystem;

namespace Havengard.Core.Health
{
    /// <summary>
    /// Handles automatic health regeneration based on stats
    /// </summary>
    public class HealthRegenerationSystem : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool enableAutoRegen = false;
        [SerializeField] private bool useStatsForRegen = true;
        [Tooltip("Base regen rate if not using stats (HP per second)")]
        [SerializeField] private float baseRegenRate = 0f;
        [Tooltip("Base regen delay if not using stats (seconds)")]
        [SerializeField] private float baseRegenDelay = 5f;

        private HealthSystem.Health health;
        private StatsComponent statsComponent;
        private float lastDamageTime;

        private void Awake()
        {
            health = GetComponent<HealthSystem.Health>();
            statsComponent = GetComponent<StatsComponent>();

            if (health != null)
            {
                health.OnDamaged += OnDamageTaken;
            }
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.OnDamaged -= OnDamageTaken;
            }
        }

        private void Update()
        {
            if (!enableAutoRegen) return;
            if (health == null || health.IsDead) return;
            if (health.CurrentHealth >= health.MaxHealth) return;

            float regenRate = GetEffectiveRegenRate();
            if (regenRate <= 0f) return;

            float regenDelay = GetEffectiveRegenDelay();

            if (Time.time >= lastDamageTime + regenDelay)
            {
                int regenAmount = Mathf.CeilToInt(regenRate * Time.deltaTime);
                if (regenAmount > 0)
                {
                    health.Heal(regenAmount);
                }
            }
        }

        private void OnDamageTaken(int amount)
        {
            lastDamageTime = Time.time;
        }

        private float GetEffectiveRegenRate()
        {
            if (useStatsForRegen && statsComponent != null && statsComponent.CurrentStats != null)
            {
                return statsComponent.CurrentStats.HealthRegenRate;
            }
            return baseRegenRate;
        }

        private float GetEffectiveRegenDelay()
        {
            if (useStatsForRegen && statsComponent != null && statsComponent.CurrentStats != null)
            {
                return statsComponent.CurrentStats.HealthRegenDelay;
            }
            return baseRegenDelay;
        }

        /// <summary>
        /// Enable or disable auto regeneration at runtime
        /// </summary>
        public void SetAutoRegen(bool enabled)
        {
            enableAutoRegen = enabled;
        }

        /// <summary>
        /// Check if auto regen is enabled
        /// </summary>
        public bool IsAutoRegenEnabled()
        {
            return enableAutoRegen;
        }
    }
}