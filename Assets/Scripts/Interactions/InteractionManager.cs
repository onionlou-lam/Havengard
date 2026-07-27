using UnityEngine;

namespace Havengard.Interactions
{
    /// <summary>
    /// Manages player interactions with interactable objects.
    /// Handles detection, tooltip display, and interaction input.
    /// </summary>
    public class InteractionManager : MonoBehaviour
    {
        public static InteractionManager Instance { get; private set; }

        [Header("Detection Settings")]
        [SerializeField]
        [Tooltip("Range to detect interactables around the player")]
        private float interactionRange = 2f;

        [SerializeField]
        [Tooltip("Layer mask for interactable objects")]
        private LayerMask interactableLayer = -1;

        [Header("UI References")]
        [SerializeField]
        [Tooltip("The tooltip UI prefab or instance")]
        private InteractionTooltip tooltipUI;

        [Header("Input")]
        [SerializeField]
        [Tooltip("Key to interact (should match PlayerController2D)")]
        private KeyCode interactKey = KeyCode.E;

        private Transform playerTransform;
        private IInteractable currentInteractable;
        private bool isShowingTooltip;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Find player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogWarning("[InteractionManager] Player not found!");
            }
        }

        private void Update()
        {
            if (playerTransform == null) return;

            // Detect nearby interactables
            DetectInteractables();

            // Handle interaction input
            if (currentInteractable != null && Input.GetKeyDown(interactKey))
            {
                if (currentInteractable.CanInteract())
                {
                    currentInteractable.Interact();
                }
            }
        }

        private void DetectInteractables()
        {
            // Find all interactables in range
            Collider2D[] colliders = Physics2D.OverlapCircleAll(
                playerTransform.position, 
                interactionRange, 
                interactableLayer
            );

            IInteractable closestInteractable = null;
            float closestDistance = float.MaxValue;

            // Find the closest valid interactable
            foreach (Collider2D col in colliders)
            {
                IInteractable interactable = col.GetComponent<IInteractable>();
                if (interactable != null && interactable.CanInteract())
                {
                    float distance = Vector2.Distance(playerTransform.position, col.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestInteractable = interactable;
                    }
                }
            }

            // Update current interactable
            if (closestInteractable != currentInteractable)
            {
                // Hide previous tooltip
                if (currentInteractable != null && tooltipUI != null)
                {
                    tooltipUI.Hide();
                    isShowingTooltip = false;
                }

                currentInteractable = closestInteractable;

                // Show new tooltip
                if (currentInteractable != null && tooltipUI != null)
                {
                    tooltipUI.Show(currentInteractable);
                    isShowingTooltip = true;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (playerTransform != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(playerTransform.position, interactionRange);
            }
        }
    }
}