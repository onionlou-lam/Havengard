// Assets/Scripts/Character/ResourceSystem.cs
using System;
using UnityEngine;
using Havengard.Abilities;

namespace Havengard.Character
{
    /// <summary>
    /// Resource system (mana, stamina, rage, etc.).
    /// Implements IResource interface with int-based API for consistency.
    /// </summary>
    public class ResourceSystem : MonoBehaviour, IResource
    {
        [SerializeField] private int maxResource = 100;
        [SerializeField] private int currentResource;

        /// <summary>
        /// Fired whenever Current or Max changes.
        /// Used by UI bars.
        /// </summary>
        public event Action OnResourceChanged;

        // IResource implementation
        public int CurrentResource => currentResource;
        public int MaxResource => maxResource;

        private void Awake()
        {
            currentResource = maxResource;
            RaiseChanged();
        }

        // ------------- IResource implementation -------------

        /// <summary>
        /// Try to consume resource. Returns false if not enough.
        /// </summary>
        public bool TryConsume(int amount)
        {
            if (amount <= 0) return true;

            if (currentResource < amount) return false;

            currentResource -= amount;
            RaiseChanged();
            return true;
        }

        /// <summary>
        /// Add resource (clamped to Max). Used for resource generation.
        /// </summary>
        public void AddResource(int amount)
        {
            if (amount <= 0) return;

            currentResource = Mathf.Min(currentResource + amount, maxResource);
            RaiseChanged();
        }

        /// <summary>
        /// Set maximum resource. Optionally refill or preserve percentage.
        /// </summary>
        public void SetMaxResource(int newMax)
        {
            float percent = maxResource > 0 ? (float)currentResource / maxResource : 1f;
            maxResource = Mathf.Max(1, newMax);

            currentResource = Mathf.RoundToInt(maxResource * percent);
            currentResource = Mathf.Clamp(currentResource, 0, maxResource);
            RaiseChanged();
        }

        // ------------- Additional utility methods -------------

        /// <summary>
        /// Restore to full.
        /// </summary>
        public void SetToMax()
        {
            currentResource = maxResource;
            RaiseChanged();
        }

        /// <summary>
        /// Set resource directly (clamped).
        /// </summary>
        public void SetResource(int value)
        {
            currentResource = Mathf.Clamp(value, 0, maxResource);
            RaiseChanged();
        }

        /// <summary>
        /// Get normalized resource (0-1 range).
        /// </summary>
        public float GetResourceNormalized()
        {
            if (maxResource <= 0) return 0f;
            return Mathf.Clamp01((float)currentResource / maxResource);
        }

        // ------------- Helpers -------------

        private void RaiseChanged()
        {
            OnResourceChanged?.Invoke();
        }
    }
}
