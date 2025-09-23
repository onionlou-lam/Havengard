using UnityEngine;
using Havengard.HealthSystem;

namespace Havengard.Units
{
    /// <summary>
    /// Ally unit: guards its spawn, but chases and attacks enemies in range.
    /// </summary>
    public class AllyUnit : UnitBase
    {
        private Vector2 spawnPoint;

        protected override void Awake()
        {
            base.Awake();
            spawnPoint = transform.position;
        }

        protected override GameObject FindTarget()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, aggroRange);
            GameObject closest = null;
            float closestDist = Mathf.Infinity;

            foreach (var hit in hits)
            {
                if (hit.TryGetComponent<Health>(out var h))
                {
                    if (h.GetFaction() == Faction.Enemy)
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

            // No enemy in range → return to spawn
            if (closest == null && Vector2.Distance(transform.position, spawnPoint) > 0.1f)
                return new GameObject("ReturnPoint") { transform = { position = spawnPoint } };

            return closest;
        }
    }
}
