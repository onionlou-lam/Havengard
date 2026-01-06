using UnityEngine;

namespace Havengard.UI
{
    [DisallowMultipleComponent]
    public class DamageNumberLibraryRuntime : MonoBehaviour
    {
        public static DamageNumberLibraryRuntime Instance { get; private set; }

        [SerializeField] private DamageNumberLibrary library;
        public DamageNumberLibrary Library => library;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }
    }
}
