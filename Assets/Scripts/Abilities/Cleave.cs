using UnityEngine;
using Havengard.HealthSystem;
using Havengard.Units;
using Havengard.Combat;

namespace Havengard.Abilities
{
    /// <summary>
    /// DEPRECATED: Use MeleeAbility with Circle hit shape instead.
    /// This script is kept for backward compatibility with existing ScriptableObject assets.
    /// To migrate: Create a new MeleeAbility asset with hitShape = Circle and radius = 1.5f
    /// </summary>
    [CreateAssetMenu(menuName = "Havengard/Abilities/Cleave (Legacy)")]
    public class Cleave : AbilityBase
    {
        [SerializeField] private float radius = 1.5f;
        [SerializeField] private bool friendlyFire = false;

        public override bool CanCast(GameObject caster, GameObject target)
        {
            return caster != null;
        }

        public override void Cast(GameObject caster, GameObject target)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(caster.transform.position, radius);
            var casterFaction = caster.GetComponent<IHealth>()?.GetFaction() ?? Faction.Neutral;

            foreach (var hit in hits)
            {
                var health = hit.GetComponent<IHealth>();
                if (FactionUtility.CanDamage(casterFaction, health, friendlyFire))
                {
                    // Use CombatCalculator for damage
                    int finalDamage = CombatCalculator.CalculateDamage(caster, hit.gameObject);
                    health.GetHealthSystem().Damage(finalDamage);
                }
            }
        }
    }
}
