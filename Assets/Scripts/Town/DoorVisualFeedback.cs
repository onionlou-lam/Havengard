using UnityEngine;
using System.Collections;

namespace Havengard.Town
{
    /// <summary>
    /// Provides visual feedback for doors (glow, particles, outline, etc.)
    /// </summary>
    public class DoorVisualFeedback : MonoBehaviour
    {
        [System.Serializable]
        public enum FeedbackType
        {
            ColorTint,
            Particle,
            OutlineGlow,
            ScalePulse,
            FloatingIcon,
            Combined
        }

        [Header("Feedback Type")]
        [SerializeField] private FeedbackType feedbackType = FeedbackType.Combined;

        [Header("Color Tint Settings")]
        [SerializeField] private SpriteRenderer targetSprite;
        [SerializeField] private Color hoverTintColor = new Color(1.3f, 1.3f, 1.0f, 1f);
        [SerializeField] private float tintSpeed = 5f;

        [Header("Particle Settings")]
        [SerializeField] private GameObject hoverParticlePrefab;
        [SerializeField] private Transform particleSpawnPoint;
        [SerializeField] private Vector3 particleOffset = Vector3.up * 0.5f;

        [Header("Outline/Glow Settings")]
        [SerializeField] private Material glowMaterial;
        [SerializeField] private string outlineColorProperty = "_OutlineColor";
        [SerializeField] private string outlineWidthProperty = "_OutlineWidth";
        [SerializeField] private Color outlineColor = Color.yellow;
        [SerializeField] private float outlineWidth = 0.05f;

        [Header("Scale Pulse Settings")]
        [SerializeField] private Transform pulseTarget;
        [SerializeField] private float pulseScale = 1.1f;
        [SerializeField] private float pulseSpeed = 2f;

        [Header("Floating Icon Settings")]
        [SerializeField] private GameObject floatingIconPrefab;
        [SerializeField] private Transform iconSpawnPoint;
        [SerializeField] private Vector3 iconOffset = Vector3.up * 1f;
        [SerializeField] private float iconBobSpeed = 2f;
        [SerializeField] private float iconBobHeight = 0.2f;

        [Header("Audio")]
        [SerializeField] private AudioClip hoverSound;
        [SerializeField][Range(0f, 1f)] private float hoverVolume = 0.3f;

        // Private state
        private Color originalColor;
        private Material originalMaterial;
        private Material instanceMaterial;
        private GameObject activeParticleInstance;
        private GameObject activeIconInstance;
        private Vector3 originalScale;
        private bool isHovering = false;
        private AudioSource audioSource;
        private float bobTimer = 0f;

        private void Awake()
        {
            // Auto-find components if not assigned
            if (targetSprite == null)
                targetSprite = GetComponent<SpriteRenderer>();

            if (targetSprite != null)
            {
                originalColor = targetSprite.color;
                originalMaterial = targetSprite.sharedMaterial;
            }

            if (pulseTarget == null)
                pulseTarget = transform;

            originalScale = pulseTarget.localScale;

            // Setup audio
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0.5f; // 2D/3D blend
            audioSource.volume = hoverVolume;
        }

        public void ShowHoverEffect()
        {
            if (isHovering) return;
            isHovering = true;

            // Play hover sound
            if (hoverSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(hoverSound);
            }

            // Apply effects based on type
            switch (feedbackType)
            {
                case FeedbackType.ColorTint:
                    StartColorTint();
                    break;
                case FeedbackType.Particle:
                    StartParticleEffect();
                    break;
                case FeedbackType.OutlineGlow:
                    StartOutlineGlow();
                    break;
                case FeedbackType.ScalePulse:
                    StartScalePulse();
                    break;
                case FeedbackType.FloatingIcon:
                    StartFloatingIcon();
                    break;
                case FeedbackType.Combined:
                    StartColorTint();
                    StartParticleEffect();
                    StartFloatingIcon();
                    break;
            }
        }

        public void HideHoverEffect()
        {
            if (!isHovering) return;
            isHovering = false;

            StopAllCoroutines();

            // Clean up effects
            if (targetSprite != null)
                targetSprite.color = originalColor;

            if (instanceMaterial != null && targetSprite != null)
            {
                targetSprite.sharedMaterial = originalMaterial;
                Destroy(instanceMaterial);
                instanceMaterial = null;
            }

            if (activeParticleInstance != null)
                Destroy(activeParticleInstance);

            if (activeIconInstance != null)
                Destroy(activeIconInstance);

            if (pulseTarget != null)
                pulseTarget.localScale = originalScale;
        }

        private void StartColorTint()
        {
            if (targetSprite == null) return;
            StartCoroutine(ColorTintRoutine());
        }

        private void StartParticleEffect()
        {
            if (hoverParticlePrefab == null) return;

            Transform spawnPoint = particleSpawnPoint != null ? particleSpawnPoint : transform;
            Vector3 spawnPos = spawnPoint.position + particleOffset;

            activeParticleInstance = Instantiate(hoverParticlePrefab, spawnPos, Quaternion.identity, transform);
        }

        private void StartOutlineGlow()
        {
            if (targetSprite == null || glowMaterial == null) return;

            // Create material instance
            instanceMaterial = new Material(glowMaterial);
            targetSprite.sharedMaterial = instanceMaterial;

            // Set outline properties
            if (instanceMaterial.HasProperty(outlineColorProperty))
                instanceMaterial.SetColor(outlineColorProperty, outlineColor);

            if (instanceMaterial.HasProperty(outlineWidthProperty))
                instanceMaterial.SetFloat(outlineWidthProperty, outlineWidth);

            StartCoroutine(OutlineGlowPulseRoutine());
        }

        private void StartScalePulse()
        {
            if (pulseTarget == null) return;
            StartCoroutine(ScalePulseRoutine());
        }

        private void StartFloatingIcon()
        {
            if (floatingIconPrefab == null) return;

            Transform spawnPoint = iconSpawnPoint != null ? iconSpawnPoint : transform;
            Vector3 spawnPos = spawnPoint.position + iconOffset;

            activeIconInstance = Instantiate(floatingIconPrefab, spawnPos, Quaternion.identity, transform);
            StartCoroutine(FloatingIconBobRoutine());
        }

        private IEnumerator ColorTintRoutine()
        {
            while (isHovering)
            {
                targetSprite.color = Color.Lerp(targetSprite.color, hoverTintColor, Time.deltaTime * tintSpeed);
                yield return null;
            }

            // Fade back
            while (Vector4.Distance(targetSprite.color, originalColor) > 0.01f)
            {
                targetSprite.color = Color.Lerp(targetSprite.color, originalColor, Time.deltaTime * tintSpeed);
                yield return null;
            }
            targetSprite.color = originalColor;
        }

        private IEnumerator OutlineGlowPulseRoutine()
        {
            if (instanceMaterial == null || !instanceMaterial.HasProperty(outlineWidthProperty))
                yield break;

            float timer = 0f;
            while (isHovering)
            {
                timer += Time.deltaTime * pulseSpeed;
                float pulse = (Mathf.Sin(timer) + 1f) * 0.5f; // 0 to 1
                float width = Mathf.Lerp(outlineWidth * 0.5f, outlineWidth, pulse);
                instanceMaterial.SetFloat(outlineWidthProperty, width);
                yield return null;
            }
        }

        private IEnumerator ScalePulseRoutine()
        {
            float timer = 0f;
            while (isHovering)
            {
                timer += Time.deltaTime * pulseSpeed;
                float pulse = (Mathf.Sin(timer) + 1f) * 0.5f; // 0 to 1
                Vector3 targetScale = Vector3.Lerp(originalScale, originalScale * pulseScale, pulse);
                pulseTarget.localScale = targetScale;
                yield return null;
            }

            // Reset scale
            pulseTarget.localScale = originalScale;
        }

        private IEnumerator FloatingIconBobRoutine()
        {
            if (activeIconInstance == null) yield break;

            Vector3 startPos = activeIconInstance.transform.position;
            bobTimer = 0f;

            while (isHovering && activeIconInstance != null)
            {
                bobTimer += Time.deltaTime * iconBobSpeed;
                float yOffset = Mathf.Sin(bobTimer) * iconBobHeight;
                activeIconInstance.transform.position = startPos + Vector3.up * yOffset;
                yield return null;
            }
        }

        private void OnDestroy()
        {
            if (instanceMaterial != null)
                Destroy(instanceMaterial);

            if (activeParticleInstance != null)
                Destroy(activeParticleInstance);

            if (activeIconInstance != null)
                Destroy(activeIconInstance);
        }
    }
}