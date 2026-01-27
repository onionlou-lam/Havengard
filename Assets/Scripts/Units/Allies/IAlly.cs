using UnityEngine;

namespace Havengard.Units
{
    /// <summary>
    /// Interface for ally units to support behavior changes and commands.
    /// </summary>
    public interface IAlly
    {
        /// <summary>
        /// Change the ally's behavior mode.
        /// </summary>
        void SetBehavior(AllyBehaviorMode mode, Transform followTarget = null);

        /// <summary>
        /// Get the current behavior mode.
        /// </summary>
        AllyBehaviorMode GetBehaviorMode();
    }
}
