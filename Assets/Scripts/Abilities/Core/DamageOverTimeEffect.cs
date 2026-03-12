using UnityEngine;
using System.Collections;
using Havengard.Core.HealthSystem;

namespace Havengard.Combat
{
    /// <summary>
    /// A reusable, stackable Damage-Over-Time effect.
    /// </summary>
    public class DamageOverTimeEffect : MonoBehaviour
    {
        private IHealth targetHealth;
        private int damagePerTick;
        private float tickInterval;
        private float duration;

        public void Init(IHealth target, int dmgPerTick, float tickInterval, float duration)
        {
            targetHealth = target;
            damagePerTick = dmgPerTick;
            this.tickInterval = tickInterval;
            this.duration = duration;
            StartCoroutine(DoDamage());
        }

        private IEnumerator DoDamage()
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (targetHealth == null) yield break;
                targetHealth.GetHealthSystem().Damage(damagePerTick);

                yield return new WaitForSeconds(tickInterval);
                elapsed += tickInterval;
            }

            Destroy(this);
        }

        /// <summary>
        /// Static helper for applying DoT directly.
        /// </summary>
        public static void ApplyTo(GameObject target, int dmgPerTick, float tickInterval, float duration)
        {
            var health = target.GetComponent<IHealth>();
            if (health == null) return;

            // Each call stacks independently
            var dot = target.AddComponent<DamageOverTimeEffect>();
            dot.Init(health, dmgPerTick, tickInterval, duration);
        }
    }
}
