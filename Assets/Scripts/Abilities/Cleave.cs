using UnityEngine;
using Havengard.HealthSystem;
using Havengard.Units;
using Havengard.Combat;

namespace Havengard.Abilities
{
    [CreateAssetMenu(menuName = "Havengard/Abilities/Cleave")]
    public class Cleave : AbilityBase
    {
        public int damage = 15;
        public float radius = 1.5f;
        public bool friendlyFire = false;

        public override void Execute(GameObject caster, GameObject target)
        {
            var sourceFaction = caster.GetComponent<IHealth>()?.GetFaction() ?? Faction.Neutral;

            Collider2D[] hits = Physics2D.OverlapCircleAll(caster.transform.position, radius);
            foreach (var hit in hits)
            {
                var health = hit.GetComponent<IHealth>();
                if (FactionUtility.CanDamage(sourceFaction, health, friendlyFire))
                {
                    health.GetHealthSystem().Damage(damage);
                }
            }
        }
    }
}
