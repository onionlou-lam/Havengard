// Assets/Scripts/Abilities/Core Scripts/IResource.cs
using UnityEngine;

namespace Havengard.Abilities
{
    /// <summary>
    /// Generic resource interface (mana, stamina, rage etc.).
    /// Float-based to allow smooth changes.
    /// </summary>
    public interface IResource
    {
        float Current { get; }
        float Max { get; }

        bool TryConsume(float amount); // Consume resources
        void Regenerate(float amount); // Regenerate resources
        void SetToMax(); // Set to Max resources
        void SetMax(float newMax, bool refill = true); // Set the amount
        void Set(float value);
    }
}
