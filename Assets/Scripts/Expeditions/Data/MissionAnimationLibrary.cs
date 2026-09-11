using UnityEngine;

namespace Havengard.Expeditions
{
    /// <summary>
    /// Data-driven library mapping each MissionType to its follower activity animation.
    /// Extendable via the Inspector - simply add a new entry when new MissionTypes are introduced.
    /// </summary>
    [CreateAssetMenu(menuName = "Havengard/Expeditions/Mission Animation Library")]
    public class MissionAnimationLibrary : ScriptableObject
    {
        [Tooltip("List of animations, one per mission type. Extend this list to support new mission types.")]
        [SerializeField] private MissionTypeAnimation[] animations;

        /// <summary>
        /// Attempts to find the animation configured for the given mission type.
        /// </summary>
        public bool TryGetAnimation(MissionType missionType, out MissionTypeAnimation animation)
        {
            if (animations != null)
            {
                foreach (var anim in animations)
                {
                    if (anim != null && anim.missionType == missionType && anim.frames != null && anim.frames.Length > 0)
                    {
                        animation = anim;
                        return true;
                    }
                }
            }

            animation = null;
            return false;
        }
    }
}