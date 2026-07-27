using UnityEngine;
using Havengard.Interactions;

namespace Havengard.Town
{
    /// <summary>
    /// Represents a building door that can be entered via interaction.
    /// Combines trigger-based and proximity-based interaction.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class BuildingDoor : MonoBehaviour, IInteractable
    {
        [Header("Building Info")]
        [SerializeField]
        [Tooltip("Name of the building (e.g., 'Inn', 'Blacksmith')")]
        private string buildingName = "Building";

        [SerializeField]
        [Tooltip("Scene to load when entering")]
        private string targetSceneName;

        [Header("Interaction Settings")]
        [SerializeField]
        [Tooltip("Can this door be used right now?")]
        private bool isAccessible = true;

        [SerializeField]
        [Tooltip("Custom interaction text override")]
        private string customPrompt = "";

        [Header("Visual Feedback")]
        [SerializeField]
        [Tooltip("Optional visual feedback component")]
        private DoorVisualFeedback visualFeedback;

        [Header("Audio")]
        [SerializeField]
        private AudioClip doorOpenSound;

        [SerializeField]
        private AudioClip lockedSound;

        [Header("Tooltip Position")]
        [SerializeField]
        [Tooltip("Offset for the interaction tooltip")]
        private Vector3 tooltipOffset = new Vector3(0, 1f, 0);

        private BoxCollider2D doorCollider;
        private bool playerInRange = false;

        private void Awake()
        {
            doorCollider = GetComponent<BoxCollider2D>();
            doorCollider.isTrigger = true;

            // Auto-find visual feedback
            if (visualFeedback == null)
            {
                visualFeedback = GetComponent<DoorVisualFeedback>();
            }

            // Set layer to Interactable
            gameObject.layer = LayerMask.NameToLayer("Interactable");
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = true;
                if (visualFeedback != null)
                {
                    visualFeedback.ShowHoverEffect();
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = false;
                if (visualFeedback != null)
                {
                    visualFeedback.HideHoverEffect();
                }
            }
        }

        // IInteractable implementation
        public string GetInteractionPrompt()
        {
            if (!string.IsNullOrEmpty(customPrompt))
                return customPrompt;

            return isAccessible ? $"Enter {buildingName}" : $"{buildingName} (Locked)";
        }

        public string GetInteractionKey()
        {
            // You can make this dynamic based on input system
            return "E";
        }

        public void Interact()
        {
            if (!isAccessible)
            {
                PlaySound(lockedSound);
                Debug.Log($"[BuildingDoor] {buildingName} is locked!");
                return;
            }

            if (string.IsNullOrEmpty(targetSceneName))
            {
                Debug.LogWarning($"[BuildingDoor] No target scene set for {buildingName}!");
                return;
            }

            PlaySound(doorOpenSound);
            LoadScene();
        }

        public bool CanInteract()
        {
            return playerInRange;
        }

        public Transform GetTooltipTransform()
        {
            return transform;
        }

        private void LoadScene()
        {
            Debug.Log($"[BuildingDoor] Entering {buildingName} - Loading scene: {targetSceneName}");

            // Use fade transition if available
            if (SceneFadeTransition.Instance != null)
            {
                SceneFadeTransition.Instance.LoadSceneWithFade(targetSceneName);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
            }
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, transform.position);
            }
        }

        public void SetAccessible(bool accessible)
        {
            isAccessible = accessible;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // Draw door indicator
            Gizmos.color = isAccessible ? Color.green : Color.red;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);

            // Draw tooltip position
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position + tooltipOffset, 0.2f);
        }

        private void OnDrawGizmosSelected()
        {
            // Draw interaction range
            BoxCollider2D col = GetComponent<BoxCollider2D>();
            if (col != null)
            {
                Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
                Gizmos.DrawCube(transform.position + (Vector3)col.offset, col.size);
            }
        }
#endif
    }
}