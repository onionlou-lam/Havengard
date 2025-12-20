using UnityEngine;

namespace Havengard.Waves
{
    public enum WaveStartCondition
    {
        TimerAfterPreviousStart,
        TimerAfterPreviousComplete
    }

    public enum WaveCompleteCondition
    {
        AllSpawnedAndAllDead,
        AllSpawnedOnly // (useful for endless-style later)
    }

    [CreateAssetMenu(menuName = "Havengard/Waves/Wave Definition")]
    public class WaveDefinition : ScriptableObject
    {
        public string waveName = "Wave";

        [Header("Progression")]
        public WaveStartCondition startCondition = WaveStartCondition.TimerAfterPreviousComplete;

        [Tooltip("If startCondition is timer-based, delay in seconds.")]
        public float startDelay = 5f;

        public WaveCompleteCondition completeCondition = WaveCompleteCondition.AllSpawnedAndAllDead;

        [Header("Spawn Groups")]
        public WaveSpawnGroup[] groups;

        [Header("Rewards")]
        public int rewardGold = 0;
        public int rewardExp = 0;
        public int rewardCelestium = 0;
    }
}
