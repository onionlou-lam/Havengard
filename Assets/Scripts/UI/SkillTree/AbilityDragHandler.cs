using Havengard.Abilities;
using UnityEngine;

namespace Havengard.UI
{
    /// <summary>
    /// Static helper to track ability drag operations from skill tree to ability bar.
    /// </summary>
    public static class AbilityDragHandler
    {
        private static AbilityBase draggedAbility;
        private static bool isDragging;

        public static void StartDrag(AbilityBase ability)
        {
            draggedAbility = ability;
            isDragging = true;
            Debug.Log($"[AbilityDragHandler] StartDrag - {(ability != null ? ability.abilityName : "NULL")}");
        }

        public static void EndDrag()
        {
            isDragging = false;
            Debug.Log($"[AbilityDragHandler] EndDrag - ability still tracked: {(draggedAbility != null ? draggedAbility.abilityName : "NULL")}");
        }

        public static void ClearDraggedAbility()
        {
            Debug.Log($"[AbilityDragHandler] ClearDraggedAbility - was: {(draggedAbility != null ? draggedAbility.abilityName : "NULL")}");
            draggedAbility = null;
            isDragging = false;
        }

        public static AbilityBase GetDraggedAbility()
        {
            Debug.Log($"[AbilityDragHandler] GetDraggedAbility - returning: {(draggedAbility != null ? draggedAbility.abilityName : "NULL")}");
            return draggedAbility;
        }

        public static bool IsDragging()
        {
            return isDragging;
        }
    }
}