namespace Havengard.Dialogue
{
    /// <summary>
    /// Implemented by NPCs so they can react to dialogue without the manager knowing about shops, quests, etc.
    /// </summary>
    public interface IDialogueListener
    {
        void OnDialogueStarted();
        void OnDialogueEvent(string eventId);
        void OnQuestRequested(string questId);
        void OnDialogueEnded();
    }
}