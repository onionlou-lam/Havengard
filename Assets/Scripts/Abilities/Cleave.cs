using UnityEngine;
using Havengard.HealthSystem;

namespace Havengard.Abilities
{
    [CreateAssetMenu(menuName = "Havengard/Abilities/Cleave")]
    public class Cleave : AbilityBase
    {
        [SerializeField] private float radius = 2f;
        [SerializeField] private float damage = 15f;


        protected override void Execute(GameObject caster, GameObject target)
        {
            IHealth health = obj.GetComponent<IHealth>();
            var casterHealth = caster.GetComponent<Health>();
            if (casterHealth == null) return;

            Collider2D[] hits = Physics2D.OverlapCircleAll(caster.transform.position, radius);
            foreach (var hit in hits)
            {

                if (hit.gameObject == caster) continue;
                if (health != null && health.GetFaction() != GetComponent<FactionProvider>().GetFaction())
                {
                    health.TakeDamage(damage);
                }
            }
        }
    }
}
