using UnityEngine;

namespace Havengard.Items
{
    /// <summary>
    /// Base class for all item effects that can be applied to a character.
    /// </summary>
    public abstract class ItemEffect : ScriptableObject
    {
        [Header("Effect Values")]
        [Tooltip("Base value at level 1")]
        public float baseValue = 10f;

        [Tooltip("Value increase per level")]
        public float perLevelValue = 5f;

        [Tooltip("Use additive (base + level*perLevel) or multiplicative (base * level) scaling")]
        public bool additiveScaling = true;

        /// <summary>
        /// Apply this effect to a character at a specific level.
        /// </summary>
        public abstract void Apply(GameObject character, int level);

        /// <summary>
        /// Remove this effect from a character at a specific level.
        /// </summary>
        public abstract void Remove(GameObject character, int level);

        /// <summary>
        /// Format the description string with actual values at the given level.
        /// </summary>
        public abstract string FormatDescription(string desc, int level);

        /// <summary>
        /// Calculate the total value for this effect at a given level.
        /// </summary>
        public virtual float GetValue(int level)
        {
            if (additiveScaling)
            {
                return baseValue + (perLevelValue * (level - 1));
            }
            else
            {
                return baseValue * level;
            }
        }
    }
}