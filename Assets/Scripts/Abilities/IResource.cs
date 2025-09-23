using UnityEngine;

namespace Havengard.Abilities
{
    /// <summary>
    /// Optional resource interface. If present on the caster, AbilityUser will use it.
    /// Implement Current and TryConsume in your mana/energy component.
    /// </summary>
    public interface IResource
    {
        float Current { get; }
        bool TryConsume(float amount);
    }
}
