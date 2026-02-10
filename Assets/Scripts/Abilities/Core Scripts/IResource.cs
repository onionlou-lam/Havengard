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
        int CurrentResource { get; }
        int MaxResource { get; }
        
        bool TryConsume(int amount);
        void AddResource(int amount);
        void SetMaxResource(int newMax);
    }
}
