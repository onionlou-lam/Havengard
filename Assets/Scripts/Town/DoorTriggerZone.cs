using UnityEngine;
using UnityEngine.SceneManagement;
using Havengard.Interactions;

namespace Havengard.Town
{
    /// <summary>
    /// Trigger zone for building doors. Can work as entrance or exit.
    /// When the player enters, loads the specified scene.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class DoorTriggerZone : MonoBehaviour, IInteractable
    {
        [Header("Scene Settings")]
        [SerializeField]
        [Tooltip("Name of the scene to load when entering this door")]
        private string targetSceneName;

        [SerializeField]
        [Tooltip("Door type - Auto enters on trigger, or requires interaction")]
        private DoorType doorType = DoorType.AutoEnter;

        [Header("Exit Settings")]
        [SerializeField]
        [Tooltip("Is this an exit door? (e.g., exiting shop back to town)")]
        private bool isExitDoor = false;

        [SerializeField]
        [Tooltip("Custom prompt for exit (e.g., 'Exit Shop')")]
        private string exitPromptText = "Exit";

        [Header("Visual Feedback")]
        [SerializeField]
        [Tooltip("Optional: Highlight sprite when player is near")]
        private SpriteRenderer highlightSprite;

        [SerializeField]
        [Tooltip("Color to use for highlight")]
        private Color highlightColor = new Color(1f, 1f, 1f, 0.5f);

        [Header("Audio")]
        [SerializeField]
        [Tooltip("Optional: Sound to play when entering door")]
        private AudioClip doorOpenSound;

        public enum DoorType
        {
            AutoEnter,      // Automatically enters on trigger
            Interaction     // Requires E press
        }

        private BoxCollider2D triggerCollider;
        private bool playerInRange = false;
        private Color originalHighlightColor;

        private void Awake()
        {
            triggerCollider = GetComponent<BoxCollider2D>();
            triggerCollider.isTrigger = true;

            if (highlightSprite != null)
            {
                originalHighlightColor = highlightSprite.color;
                highlightSprite.enabled = false;
            }

            // Set layer for interaction system
            if (doorType == DoorType.Interaction)
            {
                gameObject.layer = LayerMask.NameToLayer("Interactable");
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = true;

                if (doorType == DoorType.AutoEnter)
                {
                    LoadTargetScene();
                }
                else
                {
                    ShowHighlight();
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = false;
                HideHighlight();
            }
        }

        private void LoadTargetScene()
        {
            if (string.IsNullOrEmpty(targetSceneName))
            {
                Debug.LogWarning($"[DoorTriggerZone] No target scene specified for door: {gameObject.name}");
                return;
            }

            // Play sound effect
            if (doorOpenSound != null)
            {
                AudioSource.PlayClipAtPoint(doorOpenSound, transform.position);
            }

            Debug.Log($"[DoorTriggerZone] Loading scene: {targetSceneName}");
            if (SceneFadeTransition.Instance != null)
            {
                SceneFadeTransition.Instance.LoadSceneWithFade(targetSceneName);
            }
            else
            {
                SceneManager.LoadScene(targetSceneName);
            }
        }

        private void ShowHighlight()
        {
            if (highlightSprite != null)
            {
                highlightSprite.enabled = true;
                highlightSprite.color = highlightColor;
            }
        }

        private void HideHighlight()
        {
            if (highlightSprite != null)
            {
                highlightSprite.enabled = false;
            }
        }

        // IInteractable implementation (for interaction-based doors)
        public string GetInteractionPrompt()
        {
            return isExitDoor ? exitPromptText : $"Enter";
        }

        public string GetInteractionKey()
        {
            return "E";
        }

        public void Interact()
        {
            LoadTargetScene();
        }

        public bool CanInteract()
        {
            return playerInRange && doorType == DoorType.Interaction;
        }

        public Transform GetTooltipTransform()
        {
            return transform;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            BoxCollider2D col = GetComponent<BoxCollider2D>();
            if (col != null)
            {
                Gizmos.color = doorType == DoorType.AutoEnter ? Color.green : Color.yellow;
                Gizmos.DrawWireCube(transform.position + (Vector3)col.offset, col.size);

                // Draw arrow indicating this is a door
                UnityEditor.Handles.Label(transform.position, doorType == DoorType.AutoEnter ? "AUTO" : "INTERACT");
            }
        }
#endif
    }
}