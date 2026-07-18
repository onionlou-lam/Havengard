using UnityEngine;

namespace Havengard.Town
{
    /// <summary>
    /// Makes a door clickable. When clicked, the player will navigate to the associated trigger zone.
    /// Includes visual feedback on hover.
    /// </summary>
    [RequireComponent(typeof(DoorVisualFeedback))]
    public class ClickableDoor : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        [Tooltip("The door trigger zone the player should walk to")]
        private DoorTriggerZone associatedTriggerZone;

        [Header("Cursor")]
        [SerializeField]
        [Tooltip("Optional: Custom cursor texture when hovering over door")]
        private Texture2D doorCursor;

        [SerializeField]
        private Vector2 cursorHotspot = Vector2.zero;

        [Header("Click Feedback")]
        [SerializeField]
        private AudioClip clickSound;

        [SerializeField]
        [Range(0f, 1f)]
        private float clickVolume = 0.5f;

        private DoorVisualFeedback visualFeedback;
        private bool isHovering = false;
        private AudioSource audioSource;

        private void Awake()
        {
            visualFeedback = GetComponent<DoorVisualFeedback>();

            // Auto-find trigger zone if not assigned
            if (associatedTriggerZone == null)
            {
                associatedTriggerZone = GetComponentInChildren<DoorTriggerZone>();
            }

            // Setup audio
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0.5f;
            audioSource.volume = clickVolume;
        }

        private void OnMouseEnter()
        {
            isHovering = true;

            // Show visual feedback
            if (visualFeedback != null)
            {
                visualFeedback.ShowHoverEffect();
            }

            // Change cursor
            if (doorCursor != null)
            {
                Cursor.SetCursor(doorCursor, cursorHotspot, CursorMode.Auto);
            }
        }

        private void OnMouseExit()
        {
            isHovering = false;

            // Hide visual feedback
            if (visualFeedback != null)
            {
                visualFeedback.HideHoverEffect();
            }

            // Restore default cursor
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        private void OnMouseDown()
        {
            // Play click sound
            if (clickSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(clickSound);
            }

            if (associatedTriggerZone != null)
            {
                // Get the center position of the trigger zone for navigation
                Vector3 targetPosition = associatedTriggerZone.transform.position;
                
                // Find the player and command them to move
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    // Get the player's NavMeshAgent and set destination directly
                    UnityEngine.AI.NavMeshAgent agent = player.GetComponent<UnityEngine.AI.NavMeshAgent>();
                    if (agent != null)
                    {
                        agent.SetDestination(targetPosition);
                        Debug.Log($"[ClickableDoor] Player commanded to door: {gameObject.name} at position {targetPosition}");
                    }
                    else
                    {
                        Debug.LogWarning("[ClickableDoor] Player does not have NavMeshAgent component!");
                    }
                }
                else
                {
                    Debug.LogWarning("[ClickableDoor] Player GameObject not found! Make sure player has 'Player' tag.");
                }
            }
            else
            {
                Debug.LogWarning($"[ClickableDoor] No trigger zone associated with door: {gameObject.name}");
            }
        }

        // Editor helper - draw line to trigger zone
        private void OnDrawGizmos()
        {
            if (associatedTriggerZone != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, associatedTriggerZone.transform.position);
                
                // Draw arrow
                Vector3 direction = (associatedTriggerZone.transform.position - transform.position).normalized;
                Vector3 right = Vector3.Cross(Vector3.forward, direction) * 0.2f;
                Vector3 arrowTip = associatedTriggerZone.transform.position;
                Gizmos.DrawLine(arrowTip, arrowTip - direction * 0.3f + right);
                Gizmos.DrawLine(arrowTip, arrowTip - direction * 0.3f - right);
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Draw interaction range
            Gizmos.color = new Color(0f, 1f, 1f, 0.2f);
            Gizmos.DrawSphere(transform.position, 0.5f);
        }
    }
}