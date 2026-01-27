using UnityEngine;

namespace Havengard.Units
{
    /// <summary>
    /// Follow ally behavior: follows the player and assists in combat.
    /// </summary>
    public class FollowBehavior : AllyBehavior
    {
        [SerializeField] private float followDistance = 3f;
        [SerializeField] private float followStoppingDistance = 1.5f;

        public override GameObject FindBehaviorTarget()
        {
            // Use default UnitBase targeting for combat
            return null;
        }

        public override void OnNoTarget()
        {
            // Follow the player when not in combat
            if (followTarget == null || unit == null) return;

            float distance = Vector2.Distance(unit.transform.position, followTarget.position);

            if (distance > followDistance)
            {
                // Too far, move closer
                unit.agent.isStopped = false;
                unit.agent.SetDestination(followTarget.position);
            }
            else if (distance < followStoppingDistance)
            {
                // Close enough, stop
                unit.agent.isStopped = true;
            }
        }
    }
}