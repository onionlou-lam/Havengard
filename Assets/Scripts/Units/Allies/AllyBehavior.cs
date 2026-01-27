using UnityEngine;

namespace Havengard.Units
{
    /// <summary>
    /// Base class for ally AI behaviors.
    /// Similar to how MeleeEnemy/RangedEnemy inherit from UnitBase.
    /// </summary>
    public abstract class AllyBehavior : MonoBehaviour
    {
        protected UnitBase unit;
        protected Transform followTarget;

        public virtual void Initialize(UnitBase unitBase)
        {
            unit = unitBase;
        }

        /// <summary>
        /// Set the target to follow (usually the player).
        /// </summary>
        public virtual void SetFollowTarget(Transform target)
        {
            followTarget = target;
        }

        /// <summary>
        /// Custom target finding logic for this behavior.
        /// Returns null if using default UnitBase targeting.
        /// </summary>
        public abstract GameObject FindBehaviorTarget();

        /// <summary>
        /// Called when the unit has no target.
        /// </summary>
        public abstract void OnNoTarget();
    }
}