using UnityEngine;

namespace Havengard.Abilities
{
    public enum AbilityType
    {
        Offensive,
        Supportive,
        Utility
    }

    /// <summary>
    /// Base ScriptableObject class for all abilities.
    /// Implements IAbility and adds abilityType tagging.
    /// </summary>
    public abstract class AbilityBase : ScriptableObject, IAbility
    {
        [Header("General Settings")]
        [SerializeField] private string abilityName = "New Ability";
        [SerializeField] private float cooldown = 1f;
        [SerializeField] private int resourceCost = 0;
        [SerializeField] private Sprite icon;

        [Header("Ability Classification")]
        public AbilityType abilityType = AbilityType.Offensive;

        // IAbility interface properties
        public string AbilityName => abilityName;
        public float Cooldown => cooldown;
        public int ResourceCost => resourceCost;

        // Implementors must override
        public abstract bool CanCast(GameObject caster, GameObject target);
        public abstract void Cast(GameObject caster, GameObject target);
    }
}
