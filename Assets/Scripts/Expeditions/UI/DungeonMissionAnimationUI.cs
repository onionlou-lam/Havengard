using UnityEngine;
using UnityEngine.UI;

namespace Havengard.Expeditions.UI
{
    /// <summary>
    /// Plays a small looping "follower activity" sprite animation on top of a dungeon icon
    /// while an expedition is active there. The animation shown depends on the expedition's
    /// MissionType (e.g. Raiding = attacking, Scouting = binoculars), driven by a
    /// MissionAnimationLibrary asset so new mission types can be added without code changes.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class DungeonMissionAnimationUI : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private MissionAnimationLibrary animationLibrary;

        [Header("References")]
        [SerializeField] private Image animatedImage;

        private MissionTypeAnimation currentAnimation;
        private int currentFrameIndex;
        private float frameTimer;
        private bool isPlaying;

        private void Awake()
        {
            if (animatedImage == null)
                animatedImage = GetComponent<Image>();

            SetVisible(false);
        }

        private void Update()
        {
            if (!isPlaying || currentAnimation == null || currentAnimation.frames.Length == 0)
                return;

            frameTimer += Time.deltaTime;
            float frameDuration = 1f / Mathf.Max(0.01f, currentAnimation.frameRate);

            if (frameTimer >= frameDuration)
            {
                frameTimer -= frameDuration;
                AdvanceFrame();
            }
        }

        private void AdvanceFrame()
        {
            currentFrameIndex++;

            if (currentFrameIndex >= currentAnimation.frames.Length)
            {
                if (currentAnimation.loop)
                {
                    currentFrameIndex = 0;
                }
                else
                {
                    currentFrameIndex = currentAnimation.frames.Length - 1;
                    isPlaying = false;
                }
            }

            animatedImage.sprite = currentAnimation.frames[currentFrameIndex];
        }

        /// <summary>
        /// Starts playing the animation associated with the given mission type.
        /// Call this when an expedition becomes active at this dungeon.
        /// </summary>
        public void PlayForMissionType(MissionType missionType)
        {
            if (animationLibrary == null)
            {
                Debug.LogWarning("[DungeonMissionAnimationUI] No MissionAnimationLibrary assigned.");
                SetVisible(false);
                return;
            }

            if (!animationLibrary.TryGetAnimation(missionType, out var animation))
            {
                Debug.LogWarning($"[DungeonMissionAnimationUI] No animation configured for mission type: {missionType}");
                SetVisible(false);
                return;
            }

            currentAnimation = animation;
            currentFrameIndex = 0;
            frameTimer = 0f;
            isPlaying = true;

            animatedImage.sprite = currentAnimation.frames[0];
            SetVisible(true);
        }

        /// <summary>
        /// Stops and hides the animation (e.g. when the expedition completes or the icon is idle).
        /// </summary>
        public void StopAnimation()
        {
            isPlaying = false;
            currentAnimation = null;
            SetVisible(false);
        }

        private void SetVisible(bool visible)
        {
            if (animatedImage != null)
                animatedImage.enabled = visible;
        }
    }
}