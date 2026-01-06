using UnityEngine;

namespace Havengard.Statuses
{
    public static class StatusEffectApplier
    {
        /// <summary>
        /// Applies an effect to a target gameObject. If it already has a StatusEffectInstance,
        /// we refresh/stack instead of trying to add a duplicate component.
        /// </summary>
        public static void ApplyEffect(GameObject target, StatusEffectData effect)
        {
            if (target == null || effect == null) return;

            var health = target.GetComponent<Havengard.HealthSystem.IHealth>();
            if (health == null) return;

            var inst = target.GetComponent<StatusEffectInstance>();
            if (inst == null)
            {
                inst = target.AddComponent<StatusEffectInstance>();
                inst.Apply(effect, health);
            }
            else
            {
                inst.RefreshOrStack(effect);
            }
        }
    }
}
