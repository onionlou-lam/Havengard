using System.Collections.Generic;
using Havengard.Core.Heroes;
using UnityEngine;

namespace Havengard.Expeditions
{
    /// <summary>
    /// Runtime representation of an active expedition
    /// </summary>
    [System.Serializable]
    public class ExpeditionInstance
    {
        public string expeditionId;
        public ExpeditionData expeditionData;
        public List<HeroInstance> assignedFollowers;
        public MissionType missionType;
        public int startWave;
        public int durationInDays;
        public int daysElapsed;
        public ExpeditionStatus status;
        public bool isMainStoryMission;

        /// <summary>
        /// The result of this expedition once completed. Null while still active.
        /// Kept here (rather than only passed via event) so a completed-but-unviewed
        /// result can still be shown to the player later (e.g. next time they open the map).
        /// </summary>
        public ExpeditionResult pendingResult;

        /// <summary>
        /// True once the player has viewed the ExpeditionResultPanel for this completed expedition.
        /// Used to drive the "unclaimed reward" glow/indicator on the dungeon icon.
        /// </summary>
        public bool resultViewed;

        public ExpeditionInstance(ExpeditionData data, List<HeroInstance> followers, int currentWave)
        {
            expeditionId = data.id;
            expeditionData = data;
            assignedFollowers = new List<HeroInstance>(followers);
            missionType = data.missionType;
            startWave = currentWave;
            durationInDays = data.durationInDays;
            daysElapsed = 0;
            status = ExpeditionStatus.Active;
            isMainStoryMission = data.isMainStoryMission;
        }

        /// <summary>
        /// Advances the expedition by one day (one completed wave)
        /// </summary>
        public void AdvanceDay()
        {
            if (status != ExpeditionStatus.Active) return;

            daysElapsed++;

            if (daysElapsed >= durationInDays)
            {
                status = ExpeditionStatus.Completed;
            }
        }

        /// <summary>
        /// Checks if the expedition is complete
        /// </summary>
        public bool IsComplete()
        {
            return daysElapsed >= durationInDays || status == ExpeditionStatus.Completed;
        }

        /// <summary>
        /// Gets the progress as a 0-1 value
        /// </summary>
        public float GetProgress()
        {
            if (durationInDays <= 0) return 1f;
            return Mathf.Clamp01((float)daysElapsed / durationInDays);
        }

        /// <summary>
        /// True when this expedition has completed and the player has not yet
        /// viewed the reward/result popup for it.
        /// </summary>
        public bool HasUnviewedResult()
        {
            return status == ExpeditionStatus.Completed && pendingResult != null && !resultViewed;
        }
    }
}