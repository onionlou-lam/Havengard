namespace Havengard.Units
{
    /// <summary>
    /// Defines the AI behavior mode for ally units.
    /// </summary>
    public enum AllyBehaviorMode
    {
        /// <summary>
        /// Chases and attacks enemies within aggro range.
        /// </summary>
        Default,

        /// <summary>
        /// Attacks enemies within range but doesn't chase them.
        /// </summary>
        Stationary,

        /// <summary>
        /// Follows the player and assists in combat.
        /// </summary>
        Follow
    }
}