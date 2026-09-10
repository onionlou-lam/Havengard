using UnityEngine;

namespace Havengard.Expeditions
{
    /// <summary>
    /// ScriptableObject defining a dungeon/expedition location
    /// </summary>
    [CreateAssetMenu(menuName = "Havengard/Expeditions/Expedition Data")]
    public class ExpeditionData : ScriptableObject
    {
        [Header("Identity")]
        public string id;
        public string displayName;
        [TextArea(3, 6)]
        public string description;
        public Sprite mapIcon;
        public Sprite previewImage;

        [Header("Requirements")]
        public int recommendedLevel = 1;
        public bool hasRequiredLevel = false;
        public int requiredLevel = 1;

        [Header("Party Configuration")]
        public int recommendedPartySize = 3;
        public int maximumPartySize = 5;

        [Header("Duration")]
        [Tooltip("Number of completed waves required to complete this expedition")]
        public int durationInDays = 2;

        [Header("Mission Configuration")]
        public MissionType missionType = MissionType.Scouting;
        public bool isMainStoryMission = false;

        [Header("Future Systems")]
        [Tooltip("Base success chance (0-1). Not yet implemented.")]
        [Range(0f, 1f)]
        public float baseSuccessChance = 1f;

        [Header("Availability")]
        public bool isAvailable = true;
        public bool isUnlocked = true;

        // Extension points for future systems
        // public ExpeditionEffect[] dungeonEffects;
        // public ExpeditionReward[] baseRewards;
    }
}