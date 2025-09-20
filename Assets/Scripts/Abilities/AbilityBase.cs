using UnityEngine;

namespace Havengard.Abilities
{
    /// <summary>
    /// Base ScriptableObject for abilities. Cooldowns/resources enforced by AbilityUser.
    /// </summary>
    public abstract class AbilityBase : ScriptableObject, IAbility
    {
        [Header("General Settings")]
        [SerializeField] private string abilityName = "New Ability";
        [Min(0f)][SerializeField] private float cooldown = 1f;
        [Min(0f)][SerializeField] private float resourceCost = 0f;

        public string AbilityName => abilityName;
        public float Cooldown => cooldown;
        public float ResourceCost => resourceCost;

        /// <summary>Ability-specific gates (range/LOS/state). No cooldown/resource checks here.</summary>
        public virtual bool CanCast(GameObject caster, GameObject target) => true;

        /// <summary>Legacy direct-cast path (no gates). Prefer AbilityUser.UseAbility.</summary>
        public void Cast(GameObject caster, GameObject target) => Execute(caster, target);

        /// <summary>Effect implementation. Keep this public so overrides can be public override.</summary>
        public abstract void Execute(GameObject caster, GameObject target);
    }
}
