using UnityEngine;
using Havengard.HealthSystem;

namespace Havengard.Statuses
{
    public static class StatusEffectApplier
    {
        /// <summary>
        /// Applies an effect to a target gameObject (expects IHealth on the object).
        /// If an instance exists, it refreshes or stacks (with optional stack cap).
        /// </summary>
        public static void ApplyEffect(GameObject target, StatusEffectData effect, int maxStacks = int.MaxValue)
        {
            if (target == null || effect == null) return;

            var health = target.GetComponent<IHealth>();
            if (health == null) return;

            var existing = target.GetComponent<StatusEffectInstance>();
            if (existing != null)
            {
                existing.RefreshOrStack(effect, maxStacks);
                return;
            }

            var instance = target.AddComponent<StatusEffectInstance>();
            instance.Apply(effect, health);
        }
    }
}
