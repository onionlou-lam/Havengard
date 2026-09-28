using UnityEngine;

namespace Havengard.Expeditions.UI
{
    /// <summary>
    /// Shared formatting/visual helpers for expedition progress and pending-reward display,
    /// used by both DungeonIconUI (map icon, bound to ExpeditionData) and MissionEntryUI
    /// (side panel list row, bound to a runtime ExpeditionInstance) so the two don't
    /// duplicate the same status/progress logic.
    /// </summary>
    public static class ExpeditionDisplayUtility
    {
        /// <summary>
        /// Formats the "Day X/Y" progress label for an active expedition.
        /// </summary>
        public static string GetProgressLabel(ExpeditionInstance expedition)
        {
            if (expedition == null) return string.Empty;
            return $"Day {expedition.daysElapsed}/{expedition.durationInDays}";
        }

        /// <summary>
        /// Computes a pulsing scale multiplier for drawing attention to a pending-reward badge.
        /// Multiply this by the badge's original localScale.
        /// </summary>
        public static float GetBadgePulseScale(float pulseSpeed, float pulseAmount)
        {
            return 1f + Mathf.Sin(Time.unscaledTime * pulseSpeed) * pulseAmount;
        }
    }
}