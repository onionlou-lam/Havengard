using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Havengard.Abilities;
using System.Collections;

namespace Havengard.UI
{
    /// <summary>
    /// Handles the visual behaviour of a main skill node with VFX/SFX feedback.
    /// </summary>
    public class SkillTreeNodeUI : MonoBehaviour,
        IPointerClickHandler,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        [Header("UI")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image borderImage;
        [SerializeField] private Image glowBorder;
        [SerializeField] private GameObject lockedOverlay;

        [Header("Visual Effects - Prefab References")]
        [SerializeField] private ParticleSystem clickParticlePrefab;
        [SerializeField] private ParticleSystem hoverParticlePrefab;
        [SerializeField] private ParticleSystem unlockParticlePrefab;
        [SerializeField] private ParticleSystem pulseParticlePrefab;

        [Header("Audio Effects")]
        [SerializeField] private AudioClip clickSound;
        [SerializeField] private AudioClip hoverSound;
        [SerializeField] private AudioClip unlockSound;
        [SerializeField] private AudioClip lockedSound;
        [Range(0f, 1f)]
        [SerializeField] private float sfxVolume = 0.7f;

        [Header("Colours")]
        [SerializeField] private Color unlockedColor = Color.white;
        [SerializeField] private Color lockedColor = Color.gray;
        [SerializeField] private Color availableColor = new Color(1f, 1f, 0.5f);

        [Header("Border")]
        [SerializeField] private Color normalBorderColor = Color.white;
        [SerializeField] private Color hoverBorderColor = new Color(1f, 0.9f, 0.4f);

        [Header("Glow")]
        [SerializeField] private Color glowColour = new Color(1f, 0.95f, 0.35f);
        [SerializeField] private float glowPulseSpeed = 4f;
        [SerializeField] private float glowMinAlpha = 0.2f;
        [SerializeField] private float glowMaxAlpha = 0.9f;

        [Header("Animation")]
        [SerializeField] private bool enableScaleAnimation = true;
        [SerializeField] private float scaleAmount = 1.1f;
        [SerializeField] private float scaleDuration = 0.15f;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = false;

        private Coroutine pulseRoutine;
        private Coroutine scaleRoutine;

        private int abilityIndex;
        private ClassAbility classAbility;
        private SkillTreeUI parentUI;
        private SkillTreeParticleManager particleManager;

        private bool isSelected;
        private bool isUnlocked;
        private bool canUnlock;
        private AudioSource audioSource;

        private ParticleSystem activePulseParticle; // Track continuous pulse

        public int AbilityIndex => abilityIndex;
        public RectTransform RectTransform => GetComponent<RectTransform>();

        //-----------------------------------------------------

        private void Awake()
        {
            // Setup glow border
            if (glowBorder != null)
            {
                glowBorder.enabled = true;
                Color c = glowColour;
                c.a = 0f;
                glowBorder.color = c;
            }

            if (borderImage != null)
            {
                borderImage.color = normalBorderColor;
            }

            // Setup audio
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = sfxVolume;
        }

        //-----------------------------------------------------

        public void Initialize(int index, ClassAbility ability, SkillTreeUI parent, SkillTreeParticleManager manager)
        {
            abilityIndex = index;
            classAbility = ability;
            parentUI = parent;
            particleManager = manager;

            if (iconImage != null && ability.ability != null && ability.ability.icon != null)
            {
                iconImage.sprite = ability.ability.icon;
            }
        }

        //-----------------------------------------------------

        public void RefreshState(bool[] unlockedAbilities, int availableSkillPoints, int playerLevel)
        {
            isUnlocked = unlockedAbilities[abilityIndex];

            if (isUnlocked)
            {
                SetVisualState(unlockedColor, false);
                canUnlock = false;
                StopPulseParticle();
                return;
            }

            bool meetsLevel = playerLevel >= classAbility.requiredLevel;
            bool hasPoints = availableSkillPoints >= classAbility.skillPointCost;
            bool prereqsMet = classAbility.ArePrerequisitesMet(unlockedAbilities);

            canUnlock = meetsLevel && hasPoints && prereqsMet;

            if (canUnlock)
            {
                SetVisualState(availableColor, true);

                if (activePulseParticle == null && pulseParticlePrefab != null && particleManager != null)
                {
                    activePulseParticle = particleManager.PlayContinuousParticle(pulseParticlePrefab, RectTransform);
                }
            }
            else
            {
                SetVisualState(lockedColor, true);
                StopPulseParticle();
            }
        }

        //-----------------------------------------------------

        private void StopPulseParticle()
        {
            if (activePulseParticle != null && particleManager != null)
            {
                particleManager.StopContinuousParticle(activePulseParticle, pulseParticlePrefab);
                activePulseParticle = null;
            }
        }

        //-----------------------------------------------------

        private void SetVisualState(Color tint, bool showLocked)
        {
            if (iconImage != null)
                iconImage.color = tint;

            if (backgroundImage != null)
                backgroundImage.color = tint;

            if (lockedOverlay != null)
                lockedOverlay.SetActive(showLocked);
        }

        //-----------------------------------------------------

        public void SetSelected(bool selected)
        {
            isSelected = selected;

            if (borderImage != null)
            {
                borderImage.color = selected ? hoverBorderColor : normalBorderColor;
            }

            if (glowBorder == null)
                return;

            if (pulseRoutine != null)
            {
                StopCoroutine(pulseRoutine);
            }

            if (selected)
            {
                pulseRoutine = StartCoroutine(PulseGlow());
            }
            else
            {
                Color c = glowBorder.color;
                c.a = 0;
                glowBorder.color = c;
            }
        }

        //-----------------------------------------------------

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isSelected && borderImage != null)
            {
                borderImage.color = hoverBorderColor;
            }

            if (hoverParticlePrefab != null && particleManager != null)
            {
                particleManager.PlayParticleAtUI(hoverParticlePrefab, RectTransform);
            }

            PlaySound(hoverSound);

            if (enableScaleAnimation)
            {
                if (scaleRoutine != null)
                    StopCoroutine(scaleRoutine);
                scaleRoutine = StartCoroutine(ScaleAnimation(scaleAmount));
            }
        }

        //-----------------------------------------------------

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isSelected && borderImage != null)
            {
                borderImage.color = normalBorderColor;
            }

            if (enableScaleAnimation)
            {
                if (scaleRoutine != null)
                    StopCoroutine(scaleRoutine);
                scaleRoutine = StartCoroutine(ScaleAnimation(1f));
            }
        }

        //-----------------------------------------------------

        public void OnPointerClick(PointerEventData eventData)
        {
            if (clickParticlePrefab != null && particleManager != null)
            {
                particleManager.PlayParticleAtUI(clickParticlePrefab, RectTransform);
            }

            if (isUnlocked)
                PlaySound(clickSound);
            else if (canUnlock)
                PlaySound(clickSound);
            else
                PlaySound(lockedSound);

            parentUI?.OnNodeClicked(abilityIndex, this);
        }

        //-----------------------------------------------------

        public void PlayUnlockEffects()
        {
            if (unlockParticlePrefab != null && particleManager != null)
            {
                particleManager.PlayParticleAtUI(unlockParticlePrefab, RectTransform);
            }

            PlaySound(unlockSound);

            if (enableScaleAnimation)
            {
                if (scaleRoutine != null)
                    StopCoroutine(scaleRoutine);
                scaleRoutine = StartCoroutine(UnlockScaleAnimation());
            }
        }

        //-----------------------------------------------------

        private void PlaySound(AudioClip clip)
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip, sfxVolume);
            }
        }

        //-----------------------------------------------------

        private IEnumerator PulseGlow()
        {
            while (isSelected)
            {
                float t = Mathf.PingPong(Time.unscaledTime * glowPulseSpeed, 1f);
                float alpha = Mathf.Lerp(glowMinAlpha, glowMaxAlpha, t);

                Color c = glowColour;
                c.a = alpha;
                glowBorder.color = c;

                yield return null;
            }
        }

        //-----------------------------------------------------

        private IEnumerator ScaleAnimation(float targetScale)
        {
            Vector3 startScale = transform.localScale;
            Vector3 endScale = Vector3.one * targetScale;
            float elapsed = 0f;

            while (elapsed < scaleDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / scaleDuration;
                transform.localScale = Vector3.Lerp(startScale, endScale, t);
                yield return null;
            }

            transform.localScale = endScale;
        }

        //-----------------------------------------------------

        private IEnumerator UnlockScaleAnimation()
        {
            Vector3 originalScale = transform.localScale;
            Vector3 bigScale = originalScale * 1.3f;

            float elapsed = 0f;
            float punchDuration = 0.2f;

            // Scale up
            while (elapsed < punchDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / punchDuration;
                transform.localScale = Vector3.Lerp(originalScale, bigScale, t);
                yield return null;
            }

            // Scale back down
            elapsed = 0f;
            while (elapsed < punchDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / punchDuration;
                transform.localScale = Vector3.Lerp(bigScale, originalScale, t);
                yield return null;
            }

            transform.localScale = originalScale;
        }
    }
}