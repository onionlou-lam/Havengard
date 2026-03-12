using UnityEngine;
using Havengard.Core.HealthSystem;
using System.Collections;

namespace Havengard.UI
{
    /// <summary>
    /// Flashes the player's sprite renderer red when taking damage.
    /// Attach this to the player GameObject or a child with the SpriteRenderer.
    /// </summary>
    public class PlayerSpriteFlash : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Health health;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Flash Settings")]
        [SerializeField] private Color damageFlashColor = new Color(1f, 0.2f, 0.2f, 1f);
        [SerializeField] private float flashDuration = 0.2f;
        [SerializeField] private int flashCount = 2;

        // FIX: Fully qualify HealthSystem with its namespace if needed, or import the correct type.
        private Havengard.Core.HealthSystem.HealthSystem healthSystem;
        private Color originalColor;
        private Coroutine currentFlashCoroutine;

        private void Awake()
        {
            // Auto-find components if not assigned
            if (health == null)
                health = GetComponentInParent<Health>();

            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer != null)
                originalColor = spriteRenderer.color;
        }

        private void OnEnable()
        {
            TryHook();
        }

        private void OnDisable()
        {
            Unhook();
        }

        private void TryHook()
        {
            if (health == null || healthSystem != null) return;

            healthSystem = health.GetHealthSystem();
            if (healthSystem == null) return;

            healthSystem.OnDamaged += OnDamagedHandler;
        }

        private void Unhook()
        {
            if (healthSystem == null) return;

            healthSystem.OnDamaged -= OnDamagedHandler;
            healthSystem = null;
        }

        private void OnDamagedHandler(int amount)
        {
            PlayDamageFlash();
        }

        private void PlayDamageFlash()
        {
            if (spriteRenderer == null) return;

            if (currentFlashCoroutine != null)
                StopCoroutine(currentFlashCoroutine);

            currentFlashCoroutine = StartCoroutine(FlashCoroutine());
        }

        private IEnumerator FlashCoroutine()
        {
            float flashInterval = flashDuration / (flashCount * 2);

            for (int i = 0; i < flashCount; i++)
            {
                // Flash to damage color
                spriteRenderer.color = damageFlashColor;
                yield return new WaitForSeconds(flashInterval);

                // Return to original
                spriteRenderer.color = originalColor;
                yield return new WaitForSeconds(flashInterval);
            }

            // Ensure we end on original color
            spriteRenderer.color = originalColor;
            currentFlashCoroutine = null;
        }

        private void OnDestroy()
        {
            // Restore original color on cleanup
            if (spriteRenderer != null)
                spriteRenderer.color = originalColor;
        }
    }
}