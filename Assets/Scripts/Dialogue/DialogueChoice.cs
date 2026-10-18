using System;

namespace Havengard.Dialogue
{
    [Serializable]
    public class DialogueChoice
    {
        public string text = "Yes";

        /// <summary>Index of the next node. -1 ends the conversation.</summary>
        public int nextNodeIndex = -1;

        /// <summary>Optional id raised to the listener when picked (e.g. "accept_quest", "open_shop").</summary>
        public string eventId;
    }
}