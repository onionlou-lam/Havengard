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
    }
}