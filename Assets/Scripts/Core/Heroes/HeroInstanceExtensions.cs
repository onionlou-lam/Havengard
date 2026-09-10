using Havengard.Core.Heroes;

namespace Havengard.Expeditions
{
    /// <summary>
    /// Extension methods for HeroInstance to support expedition system
    /// Reuses existing quest tracking methods
    /// </summary>
    public static class HeroInstanceExtensions
    {
        // Note: HeroInstance already has these methods from the quest system:
        // - StartQuest(int days)
        // - ProgressQuestDay()
        // - CompleteQuest()
        // - IsOnQuest property

        // We simply reuse them for expeditions without modification
        // The expedition system will call these existing methods
    }
}