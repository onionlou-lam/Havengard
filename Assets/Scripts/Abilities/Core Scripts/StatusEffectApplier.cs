using UnityEngine;
using Havengard.HealthSystem;

namespace Havengard.Statuses
{
    public static class StatusEffectApplier
    {
        public static void ApplyEffect(GameObject target, StatusEffectData effect)
        {
            var health = target.GetComponent<IHealth>();
            if (health == null) return;

            var existing = target.GetComponent<StatusEffectInstance>();
            if (existing != null && existing.Data == effect)
            {
                existing.RefreshOrStack(effect);
                return;
            }

            var instance = target.AddComponent<StatusEffectInstance>();
            instance.Apply(effect, health);
        }
    }
}
