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

        /// <summary>
        /// The success chance roll that was used to determine the outcome (0-1).
        /// Useful for debugging/logging.
        /// </summary>
        public float rolledValue;

        /// <summary>
        /// The success chance threshold that was required to succeed (0-1).
        /// </summary>
        public float successChance;

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
            success = true; // Default to success; overwritten by ExpeditionManager's success roll
        }
    }
}