using Havengard.Statuses;
using UnityEngine;

namespace Havengard.Abilities
{
    /// <summary>
    /// Base ScriptableObject class for all abilities.
    /// Implements IAbility and adds shared ability behavior.
    /// </summary>
    public abstract class AbilityBase : ScriptableObject, IAbility
    {
        [Header("General Settings")]
        [SerializeField] private string abilityName = "New Ability";
        [SerializeField] private float cooldown = 1f;
        [SerializeField] private int resourceCost = 0;
        [SerializeField] private Sprite icon;

        [Header("Buff/Debuff Config (Optional)")]
        [SerializeField] protected StatusEffectData statusEffect; // Optional status effect on impact
        [SerializeField] protected int maxStatusStacks = 1;

        public string AbilityName => abilityName;
        public float Cooldown => cooldown;
        public int ResourceCost => resourceCost;

        // Dynamic tagging mechanism (optional, if classification is needed)
        public virtual string AbilityTag => "Generic";

        // Provide default handling of buff/debuff logic
        protected void ApplyBuffDebuff(GameObject target)
        {
            if (statusEffect != null)
            {
                StatusEffectApplier.ApplyEffect(target, statusEffect, maxStatusStacks);
            }
        }

        // Implementors must override these methods for custom ability behavior
        public abstract bool CanCast(GameObject caster, GameObject target);
        public abstract void Cast(GameObject caster, GameObject target);
    }
}