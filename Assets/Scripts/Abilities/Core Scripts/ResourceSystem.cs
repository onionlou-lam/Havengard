using UnityEngine;
using Havengard.Abilities;

namespace Havengard.Character
{
    /// <summary>
    /// Resource system (mana, stamina, rage, etc.).
    /// Float-based to align with IResource.
    /// </summary>
    public class ResourceSystem : MonoBehaviour, IResource
    {
        [SerializeField] private float maxResource = 100f;
        public float Current { get; private set; }
        public float Max => maxResource;

        private void Awake()
        {
            Current = maxResource;
        }

        /// <summary>
        /// Try to consume resource. Returns false if not enough.
        /// </summary>
        public bool TryConsume(float amount)
        {
            if (Current < amount) return false;
            Current -= amount;
            return true;
        }

        /// <summary>
        /// Regenerate resource (clamped to Max).
        /// </summary>
        public void Regenerate(float amount)
        {
            Current = Mathf.Min(Current + amount, maxResource);
        }

        /// <summary>
        /// Restore to full.
        /// </summary>
        public void SetToMax()
        {
            Current = maxResource;
        }

        /// <summary>
        /// Adjust max resource. Optionally refill or preserve percentage.
        /// </summary>
        public void SetMax(float newMax, bool refill = true)
        {
            float percent = maxResource > 0 ? Current / maxResource : 1f;
            maxResource = Mathf.Max(1f, newMax);

            Current = refill ? maxResource : maxResource * percent;
            Current = Mathf.Clamp(Current, 0, maxResource);
        }

        /// <summary>
        /// Set resource directly (clamped).
        /// </summary>
        public void Set(float value)
        {
            Current = Mathf.Clamp(value, 0, maxResource);
        }
    }
}
