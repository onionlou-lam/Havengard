using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Havengard.Abilities;
using System.Collections;

namespace Havengard.UI
{
    /// <summary>
    /// Visual representation of a sub-skill node (smaller, cyan-themed)
    /// </summary>
    public class SubSkillNodeUI : MonoBehaviour,
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

        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem clickParticlePrefab;
        [SerializeField] private ParticleSystem hoverParticlePrefab;
        [SerializeField] private ParticleSystem unlockParticlePrefab;
        [SerializeField] private ParticleSystem pulseParticlePrefab;

        [Header("Audio")]
        [SerializeField] private AudioClip clickSound;
        [SerializeField] private AudioClip hoverSound;
        [SerializeField] private AudioClip unlockSound;
        [SerializeField] private AudioClip lockedSound;
        [Range(0f, 1f)]
        [SerializeField] private float sfxVolume = 0.7f;

        [Header("Sub-Skill Colors")]
        [SerializeField] private Color unlockedColor = new Color(0.4f, 0.8f, 1f);
        [SerializeField] private Color lockedColor = new Color(0.3f, 0.3f, 0.4f);
        [SerializeField] private Color availableColor = new Color(0.6f, 1f, 1f);
        [SerializeField] private Color mutuallyExclusiveColor = new Color(0.5f, 0.2f, 0.2f);

        [Header("Border Colors")]
        [SerializeField] private Color normalBorderColor = new Color(0.5f, 0.8f, 1f);
        [SerializeField] private Color hoverBorderColor = new Color(0.8f, 1f, 1f);

        [Header("Scale")]
        [SerializeField] private float nodeScale = 0.7f;

        private int parentAbilityIndex;
        private int subSkillIndex;
        private SubSkillNodeData subSkillData;
        private SkillTreeUI parentUI;
        private SkillTreeParticleManager particleManager;

        private bool isSelected;
        private bool isUnlocked;
        private bool canUnlock;
        private bool isMutuallyExclusive; // Another sub-skill was already chosen

        private AudioSource audioSource;
        private ParticleSystem activePulseParticle;
        private Coroutine pulseRoutine;
        private Coroutine scaleRoutine;

        public RectTransform RectTransform => GetComponent<RectTransform>();
        public int ParentAbilityIndex => parentAbilityIndex;
        public int SubSkillIndex => subSkillIndex;

        private void Awake()
        {
            transform.localScale = Vector3.one * nodeScale;

            if (glowBorder != null)
            {
                Color c = unlockedColor;
                c.a = 0f;
                glowBorder.color = c;
            }

            if (borderImage != null)
                borderImage.color = normalBorderColor;

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = sfxVolume;
        }

        public void Initialize(int parentIndex, int subIndex, SubSkillNodeData data,
                               SkillTreeUI parent, SkillTreeParticleManager manager)
        {
            parentAbilityIndex = parentIndex;
            subSkillIndex = subIndex;
            subSkillData = data;
            parentUI = parent;
            particleManager = manager;

            if (iconImage != null)
            {
                Sprite icon = data.GetIcon();
                if (icon != null)
                {
                    iconImage.sprite = icon;
                    iconImage.enabled = true;
                }
            }
        }

        public void RefreshState(bool parentUnlocked, bool anySubSkillUnlocked, int selectedSubSkillIndex,
                                 int availableSkillPoints, int playerLevel)
        {
            // Check if this specific sub-skill is unlocked
            isUnlocked = selectedSubSkillIndex == subSkillIndex;

            // Check if a different sub-skill was chosen (mutual exclusivity)
            isMutuallyExclusive = anySubSkillUnlocked && !isUnlocked;

            if (isUnlocked)
            {
                SetVisualState(unlockedColor, false);
                canUnlock = false;
                StopPulseParticle();
                return;
            }

            if (isMutuallyExclusive)
            {
                // Another sub-skill was chosen - show as unavailable
                SetVisualState(mutuallyExclusiveColor, true);
                canUnlock = false;
                StopPulseParticle();
                return;
            }

            // Check requirements
            bool meetsLevel = playerLevel >= subSkillData.requiredLevel;
            bool hasPoints = availableSkillPoints >= subSkillData.skillPointCost;
            bool parentIsUnlocked = parentUnlocked;

            canUnlock = meetsLevel && hasPoints && parentIsUnlocked && !anySubSkillUnlocked;

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

        private void StopPulseParticle()
        {
            if (activePulseParticle != null && particleManager != null)
            {
                particleManager.StopContinuousParticle(activePulseParticle, pulseParticlePrefab);
                activePulseParticle = null;
            }
        }

        private void SetVisualState(Color tint, bool showLocked)
        {
            if (iconImage != null)
                iconImage.color = tint;

            if (backgroundImage != null)
                backgroundImage.color = tint;

            if (lockedOverlay != null)
                lockedOverlay.SetActive(showLocked);
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;

            if (borderImage != null)
                borderImage.color = selected ? hoverBorderColor : normalBorderColor;

            if (glowBorder == null)
                return;

            if (pulseRoutine != null)
                StopCoroutine(pulseRoutine);

            if (selected)
                pulseRoutine = StartCoroutine(PulseGlow());
            else
            {
                Color c = glowBorder.color;
                c.a = 0;
                glowBorder.color = c;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isSelected && borderImage != null)
                borderImage.color = hoverBorderColor;

            if (hoverParticlePrefab != null && particleManager != null)
                particleManager.PlayParticleAtUI(hoverParticlePrefab, RectTransform);

            PlaySound(hoverSound);

            if (scaleRoutine != null)
                StopCoroutine(scaleRoutine);
            scaleRoutine = StartCoroutine(ScaleAnimation(nodeScale * 1.1f));
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isSelected && borderImage != null)
                borderImage.color = normalBorderColor;

            if (scaleRoutine != null)
                StopCoroutine(scaleRoutine);
            scaleRoutine = StartCoroutine(ScaleAnimation(nodeScale));
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (clickParticlePrefab != null && particleManager != null)
                particleManager.PlayParticleAtUI(clickParticlePrefab, RectTransform);

            if (isMutuallyExclusive)
            {
                PlaySound(lockedSound);
                Debug.Log("[SubSkillNodeUI] Another sub-skill was already chosen for this ability");
            }
            else if (canUnlock || isUnlocked)
            {
                PlaySound(clickSound);
            }
            else
            {
                PlaySound(lockedSound);
            }

            parentUI?.OnSubSkillNodeClicked(parentAbilityIndex, subSkillIndex, subSkillData, this);
        }

        public void PlayUnlockEffects()
        {
            if (unlockParticlePrefab != null && particleManager != null)
                particleManager.PlayParticleAtUI(unlockParticlePrefab, RectTransform);

            PlaySound(unlockSound);

            if (scaleRoutine != null)
                StopCoroutine(scaleRoutine);
            scaleRoutine = StartCoroutine(UnlockScaleAnimation());
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip != null && audioSource != null)
                audioSource.PlayOneShot(clip, sfxVolume);
        }

        private IEnumerator PulseGlow()
        {
            while (isSelected)
            {
                float t = Mathf.PingPong(Time.unscaledTime * 4f, 1f);
                float alpha = Mathf.Lerp(0.2f, 0.9f, t);

                Color c = unlockedColor;
                c.a = alpha;
                glowBorder.color = c;

                yield return null;
            }
        }

        private IEnumerator ScaleAnimation(float targetScale)
        {
            Vector3 startScale = transform.localScale;
            Vector3 endScale = Vector3.one * targetScale;
            float elapsed = 0f;
            float duration = 0.15f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(startScale, endScale, t);
                yield return null;
            }

            transform.localScale = endScale;
        }

        private IEnumerator UnlockScaleAnimation()
        {
            Vector3 originalScale = Vector3.one * nodeScale;
            Vector3 bigScale = originalScale * 1.3f;
            float duration = 0.2f;

            // Scale up
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(originalScale, bigScale, t);
                yield return null;
            }

            // Scale back down
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(bigScale, originalScale, t);
                yield return null;
            }

            transform.localScale = originalScale;
        }
    }
}