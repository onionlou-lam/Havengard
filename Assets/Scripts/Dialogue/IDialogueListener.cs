using System;

namespace Havengard.Dialogue
{
    /// <summary>Lets an NPC expose functions that ExternalFunctionNodes in a graph can call.</summary>
    public interface IDialogueFunctionBinder
    {
        void Bind(string functionName, Action action);
    }

    /// <summary>Read-only view of the graph's variables (e.g. a bool set by a ModifyVariableNode).</summary>
    public interface IDialogueVariables
    {
        bool GetBool(string name);
        int GetInt(string name);
        string GetString(string name);
    }

    /// <summary>
    /// Implemented by NPCs so they can react to dialogue without the manager knowing about shops, quests, etc.
    /// </summary>
    public interface IDialogueListener
    {
        void OnDialogueStarted();

        /// <summary>Bind the function names this NPC supports (e.g. "open_shop"). Unbound automatically when the dialogue ends.</summary>
        void RegisterDialogueFunctions(IDialogueFunctionBinder binder);

        void OnDialogueEnded(IDialogueVariables variables);
    }
}