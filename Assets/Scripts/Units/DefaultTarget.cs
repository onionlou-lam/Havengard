using UnityEngine;
using Havengard.Core.HealthSystem;

namespace Havengard.Units
{
    /// <summary>
    /// Marks a GameObject as a default target for enemies when no players/allies are in range.
    /// Useful for gates, objectives, etc.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class DefaultTarget : MonoBehaviour
    {
        [Header("Target Settings")]
        [Tooltip("Priority for targeting (higher = more preferred)")]
        [SerializeField] private int targetPriority = 1;

        [Tooltip("If true, enemies will always target this over players/allies")]
        [SerializeField] private bool alwaysPriority = false;

        public int TargetPriority => targetPriority;
        public bool AlwaysPriority => alwaysPriority;

        private void OnDrawGizmos()
        {
            // Visual indicator in editor
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}