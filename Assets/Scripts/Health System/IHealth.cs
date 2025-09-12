using UnityEngine;
using Havengard.Combat;

namespace Havengard.HealthSystem
{
    public interface IHealth
    {
        void TakeDamage(float amount, Faction sourceFaction);
        void ApplyDoT(float totalDamage, float duration, Faction sourceFaction);
        float GetCurrentHealth();
        float GetMaxHealth();
        Faction GetFaction();
    }
}
