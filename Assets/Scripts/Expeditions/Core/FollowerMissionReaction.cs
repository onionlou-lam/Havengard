using UnityEngine;

namespace Havengard.Expeditions.UI
{
    /// <summary>
    /// Drives a follower's celebration or sad reaction animation based on expedition outcome.
    /// Attach this to follower prefabs/portraits (e.g. alongside HeroInstance or on a
    /// dedicated portrait GameObject in the reward popup / follower list).
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class FollowerMissionReaction : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator animator;

        [Header("Animator Trigger Names")]
        [Tooltip("Triggered when the follower's expedition succeeded.")]
        [SerializeField] private string celebrateTrigger = "Celebrate";

        [Tooltip("Triggered when the follower's expedition failed.")]
        [SerializeField] private string sadTrigger = "Sad";

        [Tooltip("Triggered to reset the follower back to idle after the reaction plays out.")]
        [SerializeField] private string idleTrigger = "Idle";

        // Animator parameter hashes (must match Animator parameter names exactly)
        private static readonly int AnimCelebrate = Animator.StringToHash("Celebrate");
        private static readonly int AnimSad = Animator.StringToHash("Sad");
        private static readonly int AnimIdle = Animator.StringToHash("Idle");

        private void Awake()
        {
            if (animator == null)
                animator = GetComponent<Animator>();
        }

        /// <summary>
        /// Plays the appropriate reaction animation for a completed expedition result.
        /// </summary>
        public void PlayReaction(bool success)
        {
            if (animator == null) return;

            if (success)
                animator.SetTrigger(AnimCelebrate);
            else
                animator.SetTrigger(AnimSad);
        }

        /// <summary>
        /// Resets the follower back to its idle state (e.g. after the reward popup closes).
        /// </summary>
        public void ResetToIdle()
        {
            if (animator == null) return;

            animator.SetTrigger(AnimIdle);
        }
    }
}