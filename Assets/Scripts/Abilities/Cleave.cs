using UnityEngine;
using Havengard.HealthSystem;
using Havengard.Units;
using Havengard.Combat;

namespace Havengard.Abilities
{
    [CreateAssetMenu(menuName = "Havengard/Abilities/Cleave")]
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
