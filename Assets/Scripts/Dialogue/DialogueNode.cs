using System;
using System.Collections.Generic;
using UnityEngine;

namespace Havengard.Dialogue
{
    [Serializable]
    public class DialogueNode
    {
        [TextArea(2, 5)] public string text;

        [Tooltip("Optional. Overrides the NPC's default portrait (expressions).")]
        public Sprite portraitOverride;

        [Tooltip("Empty = a plain line that advances. Otherwise the player must respond.")]
        public List<DialogueChoice> choices = new List<DialogueChoice>();

        [Tooltip("Next node when there are no choices. -1 = end. Default (0 + sequential) is handled by the manager if left as -2.")]
        public int nextNodeIndex = -2;

        [Header("Hooks")]
        [Tooltip("Raised to the listener when this node is shown (e.g. \"open_shop\", \"start_cutscene\").")]
        public string eventId;

        [Tooltip("If true, the listener is asked to create a quest/expedition entry.")]
        public bool createsQuest;
        public string questId;

        public bool HasChoices
        {
            get { return choices != null && choices.Count > 0; }
        }
    }
}