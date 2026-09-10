using System.Collections.Generic;
using Havengard.Core.Heroes;

namespace Havengard.Expeditions
{
    /// <summary>
    /// Represents the outcome of a completed expedition
    /// Future expansion point for success/failure and rewards
    /// </summary>
    [System.Serializable]
    public class ExpeditionResult
    {
        public string expeditionId;
        public bool success = true;
        public List<HeroInstance> participatingHeroes;

        // Future reward fields
        public int goldReward;
        public int celestiumReward;
        public int experienceReward;
        // public List<Item> itemRewards;
        // public string specialOutcome;

        public ExpeditionResult(string expId, List<HeroInstance> heroes)
        {
            expeditionId = expId;
            participatingHeroes = heroes;
            success = true; // Default to success for now
        }
    }
}