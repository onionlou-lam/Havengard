using UnityEngine;
using Havengard.UI;

namespace Havengard.Items
{
    /// <summary>
    /// Simple keybind handler for opening item cache
    /// </summary>
    public class ItemCacheKeybind : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private KeyCode openKey = KeyCode.I;
        [SerializeField] private ItemCacheUI cacheUI;

        private void Update()
        {
            if (Input.GetKeyDown(openKey))
            {
                if (cacheUI != null)
                {
                    cacheUI.Toggle();
                }
            }
        }
    }
}