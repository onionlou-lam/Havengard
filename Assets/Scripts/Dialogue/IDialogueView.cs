using System;
using System.Collections.Generic;
using UnityEngine;

namespace Havengard.Dialogue
{
    /// <summary>
    /// Presentation contract. The manager never touches Unity UI directly.
    /// </summary>
    public interface IDialogueView
    {
        /// <summary>Raised when the player advances after a line is fully shown.</summary>
        event Action AdvanceRequested;

        /// <summary>Raised with the index of the chosen response.</summary>
        event Action<int> ChoiceSelected;

        void Open();
        void ShowLine(string speaker, Sprite portrait, string text);
        void ShowChoices(IList<DialogueChoice> choices);
        void Close();
    }
}