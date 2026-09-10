using UnityEngine;

namespace Havengard.UI
{
    /// <summary>
    /// Central gate for blocking gameplay input (movement/abilities) and hiding the HUD
    /// while any full-screen UI panel (skill tree, expedition map, etc.) is open.
    /// Panels call Push() when they open and Pop() when they close.
    /// </summary>
    public static class GameplayUIBlocker
    {
        private static int blockingCount = 0;

        /// <summary>True while at least one blocking panel is open.</summary>
        public static bool IsBlocked => blockingCount > 0;

        public static void Push()
        {
            blockingCount++;
        }

        public static void Pop()
        {
            blockingCount = Mathf.Max(0, blockingCount - 1);
        }
    }
}