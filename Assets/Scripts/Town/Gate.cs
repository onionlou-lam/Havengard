using System.Collections;
using UnityEngine;
using Havengard.Interactions;
using Havengard.Core.HealthSystem;

namespace Havengard.Town
{
    /// <summary>
    /// Represents a gate that can be opened by player interaction.
    /// Opens temporarily, allowing units to pass through, then closes automatically.
    /// Can take damage and be destroyed.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(Health))]
    public class Gate : MonoBehaviour, IInteractable
    {
        [Header("Gate Settings")]
        [SerializeField]
        [Tooltip("Name of the gate")]
        private string gateName = "Gate";

        [SerializeField]
        [Tooltip("How long the gate stays open (in seconds)")]
        private float openDuration = 5f;

        [SerializeField]
        [Tooltip("Can this gate be used right now?")]
        private bool isAccessible = true;

        [Header("Animation")]
        [SerializeField]
        [Tooltip("Animator component for gate animations")]
        private Animator gateAnimator;

        [SerializeField]
        [Tooltip("Animation trigger parameter name for opening")]
        private string openTrigger = "Open";

        [SerializeField]
        [Tooltip("Animation trigger parameter name for closing")]
        private string closeTrigger = "Close";

        [Header("Collision")]
        [SerializeField]
        [Tooltip("Collider that blocks units when gate is closed")]
        private BoxCollider2D gateBlocker;

        [Header("Visual Damage States")]
        [SerializeField]
        [Tooltip("SpriteRenderer to change based on damage")]
        private SpriteRenderer gateSpriteRenderer;

        [SerializeField]
        [Tooltip("Gate sprite at 100%-76% health")]
        private Sprite gateSprite100;

        [SerializeField]
        [Tooltip("Gate sprite at 75%-51% health")]
        private Sprite gateSprite75;

        [SerializeField]
        [Tooltip("Gate sprite at 50%-26% health")]
        private Sprite gateSprite50;

        [SerializeField]
        [Tooltip("Gate sprite at 25%-1% health")]
        private Sprite gateSprite25;

        [SerializeField]
        [Tooltip("Gate sprite at 0% health (destroyed)")]
        private Sprite gateSprite0;

        [Header("Visual Feedback")]
        [SerializeField]
        [Tooltip("Optional visual feedback component")]
        private DoorVisualFeedback visualFeedback;

        [SerializeField]
        [Tooltip("Enable visual feedback effects")]
        private bool useVisualFeedback = false;

        [Header("Audio")]
        [SerializeField]
        private AudioClip gateOpenSound;

        [SerializeField]
        private AudioClip gateCloseSound;

        [SerializeField]
        private AudioClip lockedSound;

        [SerializeField]
        private AudioClip gateDestroyedSound;

        [Header("Tooltip Position")]
        [SerializeField]
        [Tooltip("Offset for the interaction tooltip")]
        private Vector3 tooltipOffset = new Vector3(0, 1f, 0);

        private BoxCollider2D triggerCollider;
        private bool playerInRange = false;
        private bool isOpen = false;
        private bool isDestroyed = false;
        private Coroutine closeCoroutine;
        private Health health;
        private HealthSystem healthSystem;

        private void Awake()
        {
            triggerCollider = GetComponent<BoxCollider2D>();
            if (triggerCollider == null)
            {
                Debug.LogError("[Gate] No BoxCollider2D found on Gate!");
                return;
            }

            triggerCollider.isTrigger = true;

            // Get health component
            health = GetComponent<Health>();

            // Auto-find sprite renderer if not set
            if (gateSpriteRenderer == null)
            {
                gateSpriteRenderer = GetComponent<SpriteRenderer>();
            }

            // Auto-find animator if not set
            if (gateAnimator == null)
            {
                gateAnimator = GetComponent<Animator>();
            }

            // Ensure blocker is not a trigger
            if (gateBlocker != null)
            {
                gateBlocker.isTrigger = false;
            }

            // Set layer to Interactable
            gameObject.layer = LayerMask.NameToLayer("Interactable");

            // Set tag for easy finding
            if (!CompareTag("Gate"))
            {
                gameObject.tag = "Gate";
            }
        }

        private void OnEnable()
        {
            if (health != null)
            {
                healthSystem = health.GetHealthSystem();
                if (healthSystem != null)
                {
                    healthSystem.OnHealthChanged += OnHealthChanged;
                    healthSystem.OnDeath += OnGateDestroyed;
                }
            }

            UpdateVisualDamageState();
        }

        private void OnDisable()
        {
            if (healthSystem != null)
            {
                healthSystem.OnHealthChanged -= OnHealthChanged;
                healthSystem.OnDeath -= OnGateDestroyed;
            }
        }

        private void OnHealthChanged(int current, int max)
        {
            UpdateVisualDamageState();
        }

        private void OnGateDestroyed()
        {
            isDestroyed = true;
            isAccessible = false;

            // Update to destroyed sprite
            UpdateVisualDamageState();

            // Play destroyed sound
            PlaySound(gateDestroyedSound);

            // Disable gate blocker - units can pass through
            if (gateBlocker != null)
            {
                gateBlocker.enabled = false;
            }

            // Disable interaction
            if (triggerCollider != null)
            {
                triggerCollider.enabled = false;
            }

            Debug.Log($"[Gate] {gateName} has been destroyed!");
        }

        private void UpdateVisualDamageState()
        {
            if (gateSpriteRenderer == null || healthSystem == null) return;

            float healthPercent = healthSystem.GetHealthNormalized();
            Sprite newSprite = GetSpriteForHealth(healthPercent);

            if (newSprite != null)
            {
                gateSpriteRenderer.sprite = newSprite;
            }
        }

        private Sprite GetSpriteForHealth(float healthPercent)
        {
            if (healthPercent <= 0f)
                return gateSprite0;
            else if (healthPercent <= 0.25f)
                return gateSprite25;
            else if (healthPercent <= 0.5f)
                return gateSprite50;
            else if (healthPercent <= 0.75f)
                return gateSprite75;
            else
                return gateSprite100;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = true;

                if (useVisualFeedback && visualFeedback != null && visualFeedback.enabled)
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

                if (useVisualFeedback && visualFeedback != null && visualFeedback.enabled)
                {
                    visualFeedback.HideHoverEffect();
                }
            }
        }

        // IInteractable implementation
        public string GetInteractionPrompt()
        {
            if (isDestroyed)
                return $"{gateName} (Destroyed)";

            if (isOpen)
                return $"{gateName} (Open)";

            return isAccessible ? $"Open {gateName}" : $"{gateName} (Locked)";
        }

        public string GetInteractionKey()
        {
            return "E";
        }

        public void Interact()
        {
            if (isDestroyed || !isAccessible || isOpen)
                return;

            OpenGate();
        }

        public bool CanInteract()
        {
            return playerInRange && !isOpen && !isDestroyed && isAccessible;
        }

        public Transform GetTooltipTransform()
        {
            return transform;
        }

        private void OpenGate()
        {
            isOpen = true;

            // Play animation
            if (gateAnimator != null)
            {
                gateAnimator.SetTrigger(openTrigger);
            }

            // Disable blocker collider to allow units to pass
            if (gateBlocker != null)
            {
                gateBlocker.enabled = false;
            }

            PlaySound(gateOpenSound);
            Debug.Log($"[Gate] {gateName} opened!");

            // Start close timer
            if (closeCoroutine != null)
            {
                StopCoroutine(closeCoroutine);
            }
            closeCoroutine = StartCoroutine(CloseGateAfterDelay());
        }

        private IEnumerator CloseGateAfterDelay()
        {
            yield return new WaitForSeconds(openDuration);
            CloseGate();
        }

        private void CloseGate()
        {
            if (isDestroyed) return;

            isOpen = false;

            // Play close animation
            if (gateAnimator != null)
            {
                gateAnimator.SetTrigger(closeTrigger);
            }

            // Re-enable blocker collider
            if (gateBlocker != null)
            {
                gateBlocker.enabled = true;
            }

            PlaySound(gateCloseSound);
            Debug.Log($"[Gate] {gateName} closed!");
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

        // Manual control methods (for quest/scripted events)
        public void ForceOpen()
        {
            if (!isOpen && !isDestroyed)
            {
                OpenGate();
            }
        }

        public void ForceClose()
        {
            if (isOpen && closeCoroutine != null && !isDestroyed)
            {
                StopCoroutine(closeCoroutine);
                CloseGate();
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // Draw gate indicator
            Gizmos.color = isDestroyed ? Color.black : (isOpen ? Color.green : (isAccessible ? Color.yellow : Color.red));
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

            // Draw blocker collider
            if (gateBlocker != null)
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
                Gizmos.DrawCube(transform.position + (Vector3)gateBlocker.offset, gateBlocker.size);
            }
        }
#endif
    }
}