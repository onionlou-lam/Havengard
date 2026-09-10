using System;
using System.Collections.Generic;

namespace Havengard.Expeditions
{
    [Serializable]
    public class ExpeditionSaveData
    {
        public List<ActiveExpeditionData> activeExpeditions;

        [Serializable]
        public struct ActiveExpeditionData
        {
            public string expeditionId;
            public string[] followerNames; // Or use unique IDs
            public int daysElapsed;
            public string status;
        }
    }
}