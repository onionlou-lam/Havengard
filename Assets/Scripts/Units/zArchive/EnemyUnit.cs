/*using UnityEngine;
using Havengard.HealthSystem;

namespace Havengard.Units
{
    /// <summary>
    /// Enemy unit: prioritizes Player/Allies in range, falls back to Gate.
    /// </summary>
    public class EnemyUnit : UnitBase
    {
        [Header("Enemy Specific")]
        [SerializeField] protected Transform gateTarget;

        protected override GameObject FindTarget()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, aggroRange);
            GameObject closest = null;
            float closestDist = Mathf.Infinity;

            foreach (var hit in hits)
            {
                if (hit.TryGetComponent<IHealth>(out var th))
                {
                    var f = th.GetFaction();
                    if (f == Faction.Player || f == Faction.Ally)
                    {
                        float d = Vector2.Distance(transform.position, hit.transform.position);
                        if (d < closestDist)
                        {
                            closestDist = d;
                            closest = hit.gameObject;
                        }
                    }
                }
            }

            if (closest == null && gateTarget != null) return gateTarget.gameObject;
            return closest;
        }
    }
}
*/