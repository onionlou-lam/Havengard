using UnityEngine;

namespace Havengard.Units
{
    /// <summary>
    /// Stationary ally behavior: attacks enemies in range but doesn't move.
    /// </summary>
    public class StationaryBehavior : AllyBehavior
    {
        private Vector2 stationaryPosition;

        public override void Initialize(UnitBase unitBase)
        {
            base.Initialize(unitBase);
            stationaryPosition = unitBase.transform.position;
        }

        public override GameObject FindBehaviorTarget()
        {
            // Use default UnitBase targeting
            return null;
        }

        public override void OnNoTarget()
        {
            // Stay at stationary position
            if (unit != null && Vector2.Distance(unit.transform.position, stationaryPosition) > 0.1f)
            {
                unit.agent.isStopped = false;
                unit.agent.SetDestination(stationaryPosition);
            }
            else if (unit != null)
            {
                unit.agent.isStopped = true;
            }
        }
    }
}