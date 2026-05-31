using UnityEngine;

namespace Havengard.Waves
{
    public enum WaveStartCondition
    {
        TimerAfterPreviousStart,
        TimerAfterPreviousComplete,
        ManualStartOnly,              // NEW: Only button, no timer
        ManualOrTimerAfterComplete    // NEW: Timer + button option
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

        /// <summary>
        /// Returns true if this wave should use a timer (either auto-start or optional)
        /// </summary>
        public bool UsesTimer()
        {
            return startCondition == WaveStartCondition.TimerAfterPreviousStart ||
                   startCondition == WaveStartCondition.TimerAfterPreviousComplete ||
                   startCondition == WaveStartCondition.ManualOrTimerAfterComplete;
        }

        /// <summary>
        /// Returns true if this wave can be manually started with a button
        /// </summary>
        public bool AllowsManualStart()
        {
            return startCondition == WaveStartCondition.ManualStartOnly ||
                   startCondition == WaveStartCondition.ManualOrTimerAfterComplete;
        }
    }
}
