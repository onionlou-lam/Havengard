using UnityEngine;

namespace Havengard.Character
{
    public class ResourceSystem : MonoBehaviour
    {
        [SerializeField] private float maxResource = 100f;
        public float Current { get; private set; }

        private void Awake()
        {
            Current = maxResource;
        }

        public void Consume(float amount)
        {
            Current = Mathf.Max(Current - amount, 0);
        }

        public void Regenerate(float amount)
        {
            Current = Mathf.Min(Current + amount, maxResource);
        }
    }
}
