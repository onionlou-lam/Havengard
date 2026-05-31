using UnityEngine;
using System;

namespace Havengard.Abilities
{
    public class ResourceSystem : MonoBehaviour
    {
        [Header("Resource Configuration")]
        [SerializeField] private int maxResource = 100;
        [SerializeField] private int currentResource = 100;

        [Header("Regeneration")]
        [SerializeField] private float regenRate = 5f;
        [SerializeField] private float regenDelay = 2f;

        private float lastSpendTime;

        public event Action<int, int> OnResourceChanged;
        public event Action OnResourceDepleted;

        // Public properties
        public int Current => currentResource;
        public int Max => maxResource;
        public int CurrentResource => currentResource;
        public int MaxResource => maxResource;

        private void Start()
        {
            currentResource = maxResource;
            OnResourceChanged?.Invoke(currentResource, maxResource);
        }

        private void Update()
        {
            if (currentResource < maxResource && Time.time >= lastSpendTime + regenDelay)
            {
                float regenAmount = regenRate * Time.deltaTime;
                AddResource((int)regenAmount);
            }
        }

        public bool HasResource(int amount)
        {
            return currentResource >= amount;
        }

        public bool TryConsume(int amount)
        {
            return SpendResource(amount);
        }

        public bool SpendResource(int amount)
        {
            if (!HasResource(amount))
            {
                OnResourceDepleted?.Invoke();
                return false;
            }

            currentResource -= amount;
            lastSpendTime = Time.time;
            OnResourceChanged?.Invoke(currentResource, maxResource);
            return true;
        }

        public void AddResource(int amount)
        {
            currentResource = Mathf.Min(currentResource + amount, maxResource);
            OnResourceChanged?.Invoke(currentResource, maxResource);
        }

        public void IncreaseMaxResource(int amount)
        {
            maxResource += amount;
            currentResource += amount;
            OnResourceChanged?.Invoke(currentResource, maxResource);
        }

        public void SetMaxResource(int newMax)
        {
            maxResource = Mathf.Max(1, newMax);
            currentResource = Mathf.Min(currentResource, maxResource);
            OnResourceChanged?.Invoke(currentResource, maxResource);
        }

        public void SetToMax()
        {
            currentResource = maxResource;
            OnResourceChanged?.Invoke(currentResource, maxResource);
        }

        public void RestoreToFull()
        {
            SetToMax();
        }

        public float GetResourceNormalized()
        {
            return maxResource > 0 ? (float)currentResource / maxResource : 0f;
        }
        
        // ADD THIS METHOD FOR SAVE SYSTEM
        /// <summary>
        /// Set current resource directly (for loading saves)
        /// </summary>
        public void SetCurrentResource(int amount)
        {
            currentResource = Mathf.Clamp(amount, 0, maxResource);
            OnResourceChanged?.Invoke(currentResource, maxResource);
            Debug.Log($"[ResourceSystem] Resource set to: {currentResource}/{maxResource}");
        }
    }
}