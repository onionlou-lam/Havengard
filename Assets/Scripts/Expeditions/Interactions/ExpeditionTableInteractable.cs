using UnityEngine;
using Havengard.Interactions;
using Havengard.Expeditions.UI;

namespace Havengard.Expeditions
{
    /// <summary>
    /// Interactable object that opens the Expedition Map
    /// Place this on the Expedition Table game object
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ExpeditionTableInteractable : MonoBehaviour, IInteractable
    {
        [Header("Interaction Settings")]
        [SerializeField] private string promptText = "Open Expedition Map";
        [SerializeField] private string keyHint = "E";
        [SerializeField] private Transform tooltipTransform;

        [Header("References")]
        [SerializeField] private ExpeditionMapController mapController;

        private void Start()
        {
            // Auto-find map controller if not assigned
            if (mapController == null)
            {
                mapController = FindFirstObjectByType<ExpeditionMapController>();
            }

            // Auto-assign tooltip transform if not set
            if (tooltipTransform == null)
            {
                tooltipTransform = transform;
            }
        }

        public string GetInteractionPrompt()
        {
            return promptText;
        }

        public string GetInteractionKey()
        {
            return keyHint;
        }

        public void Interact()
        {
            if (CanInteract())
            {
                Debug.Log("[ExpeditionTable] Opening Expedition Map");
                if (mapController != null)
                {
                    mapController.OpenMap();
                }
                else
                {
                    Debug.LogWarning("[ExpeditionTable] ExpeditionMapController not found!");
                }
            }
        }

        public bool CanInteract()
        {
            // Add conditions here if needed (e.g., during wave combat, etc.)
            return mapController != null;
        }

        public Transform GetTooltipTransform()
        {
            return tooltipTransform;
        }
    }
}