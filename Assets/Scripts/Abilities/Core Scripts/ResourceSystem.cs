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
        [Header("Resource Configuration")]
        [SerializeField] private ResourceType resourceType = ResourceType.Mana;
        [SerializeField] private int maxResource = 100;
        [SerializeField] private int currentResource = 100;
        
        [Header("Regeneration")]
        [SerializeField] private float regenRate = 5f; // Per second
        [SerializeField] private float regenDelay = 2f; // Delay after spending
        
        private float lastSpendTime;

        /// <summary>
        /// Fired whenever Current or Max changes.
        /// Used by UI bars.
        /// </summary>
        public event Action<int, int> OnResourceChanged;
        public event Action OnResourceDepleted;

        // IResource implementation
        public int CurrentResource => currentResource;
        public int MaxResource => maxResource;

        private void Start()
        {
            currentResource = maxResource;
            OnResourceChanged?.Invoke(currentResource, maxResource);
        }

        private void Update()
        {
            // Regenerate resource
            if (currentResource < maxResource && Time.time >= lastSpendTime + regenDelay)
            {
                float regenAmount = regenRate * Time.deltaTime;
                AddResource((int)regenAmount);
            }
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
            lastSpendTime = Time.time;
            OnResourceChanged?.Invoke(currentResource, maxResource);
            return true;
        }

        /// <summary>
        /// Add resource (clamped to Max). Used for resource generation.
        /// </summary>
        public void AddResource(int amount)
        {
            if (amount <= 0) return;

            currentResource = Mathf.Min(currentResource + amount, maxResource);
            OnResourceChanged?.Invoke(currentResource, maxResource);
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
            OnResourceChanged?.Invoke(currentResource, maxResource);
        }

        // ------------- Additional utility methods -------------

        /// <summary>
        /// Restore to full.
        /// </summary>
        public void SetToMax()
        {
            currentResource = maxResource;
            OnResourceChanged?.Invoke(currentResource, maxResource);
        }

        /// <summary>
        /// Set resource directly (clamped).
        /// </summary>
        public void SetResource(int value)
        {
            currentResource = Mathf.Clamp(value, 0, maxResource);
            OnResourceChanged?.Invoke(currentResource, maxResource);
        }

        /// <summary>
        /// Get normalized resource (0-1 range).
        /// </summary>
        public float GetResourceNormalized()
        {
            if (maxResource <= 0) return 0f;
            return Mathf.Clamp01((float)currentResource / maxResource);
        }

        public void IncreaseMaxResource(int amount)
        {
            maxResource += amount;
            currentResource = Mathf.Min(currentResource + amount, maxResource);
            OnResourceChanged?.Invoke(currentResource, maxResource);
            Debug.Log($"[ResourceSystem] Max {resourceType} increased by {amount}. New max: {maxResource}");
        }

        /// <summary>
        /// Restore to full.
        /// </summary>
        public void RestoreToFull()
        {
            currentResource = maxResource;
            OnResourceChanged?.Invoke(currentResource, maxResource);
        }
    }
}
