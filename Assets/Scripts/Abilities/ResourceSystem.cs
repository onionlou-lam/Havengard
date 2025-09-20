using UnityEngine;
using Havengard.Abilities;

namespace Havengard.Character
{
    /// <summary>
    /// Resource system (mana, stamina, etc.).
    /// Implements IResource so AbilityUser can consume it.
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

        public bool TryConsume(float amount)
        {
            if (Current < amount) return false;
            Current -= amount;
            return true;
        }

        public void Regenerate(float amount)
        {
            Current = Mathf.Min(Current + amount, maxResource);
        }

        public void SetToMax()
        {
            Current = maxResource;
        }

        public void SetMaxResource(float newMax, bool refill = true)
        {
            float percent = maxResource > 0 ? Current / maxResource : 1f;
            maxResource = Mathf.Max(1f, newMax);
            Current = refill ? maxResource : maxResource * percent;
        }

        public void SetResource(float value)
        {
            Current = Mathf.Clamp(value, 0, maxResource);
        }
    }
}
