using UnityEngine;

namespace Havengard.Dialogue
{
    /// <summary>Optional. An NPC implements this to tell Yarn which quest it owns.</summary>
    public interface IQuestDialogueSource
    {
        string QuestId { get; }
    }

    /// <summary>Who is speaking right now. Set by DialogueManager for the length of a conversation.</summary>
    public static class DialogueContext
    {
        public static string CurrentQuestId { get; set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState()
        {
            CurrentQuestId = null;
        }
    }
}