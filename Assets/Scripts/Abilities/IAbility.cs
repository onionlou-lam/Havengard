using UnityEngine;

namespace Havengard.Abilities
{
    public interface IAbility
    {
        string AbilityName { get; }
        float Cooldown { get; }
        int ResourceCost { get; }

        bool CanCast(GameObject caster, GameObject target);
        void Cast(GameObject caster, GameObject target);
    }
}
