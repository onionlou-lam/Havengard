using Havengard.Health;
using UnityEngine;

public interface IHealth
{
    float CurrentHealth { get; }
    float MaxHealth { get; }

    Faction GetFaction();

    void TakeDamage(float amount);
    void Heal(float amount);
    void ApplyDoT(float damagePerSecond, float duration, float interval);

    // Events for UI / floating text etc.
    event System.Action<float> OnDamaged;
    event System.Action<float> OnHealed;
    event System.Action OnDeath;
}
