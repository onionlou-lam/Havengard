using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Havengard.Expeditions.UI
{
    /// <summary>
    /// Plays a celebrate/sad reaction on a follower portrait.
    /// If an Animator with a controller is assigned, it fires triggers on it.
    /// Otherwise it falls back to swapping sprite frames on the Image.
    /// Don't use both on the same object - an Animator overrides script-set sprites.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class FollowerMissionReaction : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image portraitImage;

        [Header("Animator Mode (preferred)")]
        [Tooltip("Optional. If assigned and it has a controller, triggers are used and the frame lists below are ignored.")]
        [SerializeField] private Animator animator;
        [SerializeField] private string celebrateTrigger = "Celebrate";
        [SerializeField] private string sadTrigger = "Sad";
        [SerializeField] private string idleTrigger = "Idle";

        [Header("Frame Mode (fallback)")]
        [SerializeField] private Sprite[] celebrateFrames;
        [SerializeField] private float celebrateFrameRate = 8f;
        [SerializeField] private bool celebrateLoop = true;
        [SerializeField] private Sprite[] sadFrames;
        [SerializeField] private float sadFrameRate = 6f;
        [SerializeField] private bool sadLoop = true;
        [SerializeField] private Sprite idleSprite;

        [Header("Celebration VFX")]
        [SerializeField] private ParticleSystem confettiBurst;

        [Header("Aseprite / SpriteRenderer clips")]
        [Tooltip("Optional. If the Animator clips animate a SpriteRenderer, its sprite is copied to the UI Image every frame.")]
        [SerializeField] private SpriteRenderer spriteRendererSource;

        [Header("Follower Info (under the sprite)")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI levelText;

        private int celebrateHash;
        private int sadHash;
        private int idleHash;

        private Sprite[] currentFrames;
        private float currentFrameRate;
        private bool currentLoop;
        private int currentFrameIndex;
        private float frameTimer;
        private bool isPlaying;

        private bool UseAnimator => animator != null && animator.runtimeAnimatorController != null;

        private void Awake()
        {
            if (portraitImage == null)
                portraitImage = GetComponent<Image>();

            if (animator == null)
                animator = GetComponentInChildren<Animator>(true);

            if (spriteRendererSource == null)
                spriteRendererSource = GetComponent<SpriteRenderer>();

            if (spriteRendererSource != null)
                spriteRendererSource.enabled = false; // never draw it; the Image shows the sprite

            celebrateHash = Animator.StringToHash(celebrateTrigger);
            sadHash = Animator.StringToHash(sadTrigger);
            idleHash = Animator.StringToHash(idleTrigger);

            if (!UseAnimator && idleSprite != null)
                portraitImage.sprite = idleSprite;
        }

        private void LateUpdate()
        {
            // Animator writes to the SpriteRenderer; mirror it onto the Image
            if (UseAnimator && spriteRendererSource != null && spriteRendererSource.sprite != null)
                portraitImage.sprite = spriteRendererSource.sprite;
        }

        private void Update()
        {
            if (UseAnimator || !isPlaying || currentFrames == null || currentFrames.Length == 0)
                return;

            frameTimer += Time.unscaledDeltaTime;
            float frameDuration = 1f / Mathf.Max(0.01f, currentFrameRate);

            if (frameTimer >= frameDuration)
            {
                frameTimer -= frameDuration;
                AdvanceFrame();
            }
        }

        private void AdvanceFrame()
        {
            currentFrameIndex++;

            if (currentFrameIndex >= currentFrames.Length)
            {
                if (currentLoop)
                {
                    currentFrameIndex = 0;
                }
                else
                {
                    currentFrameIndex = currentFrames.Length - 1;
                    isPlaying = false;
                }
            }

            portraitImage.sprite = currentFrames[currentFrameIndex];
        }

        public void PlayReaction(bool success)
        {
            if (UseAnimator)
            {
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
                animator.ResetTrigger(idleHash);
                animator.SetTrigger(success ? celebrateHash : sadHash);
            }
            else
            {
                currentFrames = success ? celebrateFrames : sadFrames;
                currentFrameRate = success ? celebrateFrameRate : sadFrameRate;
                currentLoop = success ? celebrateLoop : sadLoop;
                currentFrameIndex = 0;
                frameTimer = 0f;

                if (currentFrames != null && currentFrames.Length > 0)
                {
                    isPlaying = true;
                    portraitImage.sprite = currentFrames[0];
                }
                else
                {
                    isPlaying = false;
                    Debug.LogWarning($"[FollowerMissionReaction] No Animator controller and no {(success ? "celebrate" : "sad")} frames on '{name}'.", this);
                }
            }

            if (success && confettiBurst != null)
                confettiBurst.Play();
        }

        public void ResetToIdle()
        {
            if (UseAnimator)
            {
                animator.SetTrigger(idleHash);
            }
            else
            {
                isPlaying = false;
                currentFrames = null;
                if (idleSprite != null)
                    portraitImage.sprite = idleSprite;
            }

            if (confettiBurst != null)
                confettiBurst.Stop();
        }

        public void SetFollowerInfo(string followerName, int level)
        {
            if (nameText != null)
                nameText.text = followerName;

            if (levelText != null)
                levelText.text = $"Lv. {level}";
        }

        /// <summary>
        /// Sets the per-follower idle sprite shown before/after the reaction.
        /// </summary>
        public void Initialize(Sprite followerIdleSprite)
        {
            if (followerIdleSprite != null)
                idleSprite = followerIdleSprite;

            if (portraitImage != null && idleSprite != null)
                portraitImage.sprite = idleSprite;
        }
    }
}