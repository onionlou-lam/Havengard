using System;
using Havengard.Units;

namespace Havengard.HealthSystem
{
    public interface IHealth
    {
        HealthSystem GetHealthSystem();
        bool TryGetHealthSystem(out HealthSystem system);

        Faction GetFaction();

        int CurrentHealth { get; }
        int MaxHealth { get; }
        bool IsDead { get; }

        /// <summary>Raised when damage is applied (amount is positive).</summary>
        event Action<int> OnDamaged;

        /// <summary>Raised when healing is applied (amount is positive).</summary>
        event Action<int> OnHealed;

        /// <summary>Raised any time HP/max HP changes.</summary>
        event Action<int, int> OnHealthChanged; // (current, max)

        /// <summary>Raised once when HP reaches 0.</summary>
        event Action OnDeath;
    }
}
