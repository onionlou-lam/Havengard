using UnityEngine;
using UnityEngine.SceneManagement;

namespace Havengard.Town
{
    /// <summary>
    /// Trigger zone for building doors. When the player enters, loads the specified scene.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class DoorTriggerZone : MonoBehaviour
    {
        [Header("Scene Settings")]
        [SerializeField]
        [Tooltip("Name of the scene to load when entering this door")]
        private string targetSceneName;

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
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Check if the player entered the zone
            if (other.CompareTag("Player"))
            {
                playerInRange = true;
                LoadTargetScene();
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

        // Editor helper - visualize trigger zone
        private void OnDrawGizmos()
        {
            BoxCollider2D col = GetComponent<BoxCollider2D>();
            if (col != null)
            {
                Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(col.offset, col.size);
            }
        }

        private void OnDrawGizmosSelected()
        {
            BoxCollider2D col = GetComponent<BoxCollider2D>();
            if (col != null)
            {
                Gizmos.color = Color.green;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(col.offset, col.size);
            }
        }
    }
}