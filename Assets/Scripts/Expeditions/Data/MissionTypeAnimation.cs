using System;
using UnityEngine;

namespace Havengard.Expeditions
{
    /// <summary>
    /// Defines a looping frame-based animation associated with a specific MissionType.
    /// Used to show a small "follower doing an activity" visual on top of dungeon icons.
    /// </summary>
    [Serializable]
    public class MissionTypeAnimation
    {
        [Tooltip("The mission type this animation is used for (e.g. Raiding = attacking).")]
        public MissionType missionType;

        [Tooltip("Sprite frames played in sequence to form the animation.")]
        public Sprite[] frames;

        [Tooltip("Frames per second for this animation.")]
        [Min(0.01f)]
        public float frameRate = 8f;

        [Tooltip("If true, animation loops. If false, it plays once and holds on the last frame.")]
        public bool loop = true;
    }
}