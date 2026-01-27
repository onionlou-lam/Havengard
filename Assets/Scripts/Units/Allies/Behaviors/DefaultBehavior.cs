using UnityEngine;

namespace Havengard.Units
{
    /// <summary>
    /// Default ally behavior: chase and attack enemies within aggro range.
    /// Returns to spawn point when no enemies are nearby.
    /// </summary>
    public class DefaultBehavior : AllyBehavior
    {
        private Vector2 spawnPoint;

        public override void Initialize(UnitBase unitBase)
        {
            base.Initialize(unitBase);
            spawnPoint = unitBase.transform.position;
        }

        public override GameObject FindBehaviorTarget()
        {
            // Use default UnitBase targeting
            return null;
        }

        public override void OnNoTarget()
        {
            // Return to spawn point when idle
            if (unit != null && Vector2.Distance(unit.transform.position, spawnPoint) > 0.1f)
            {
                unit.agent.isStopped = false;
                unit.agent.SetDestination(spawnPoint);
            }
        }
    }
}