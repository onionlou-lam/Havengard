using System;
using System.Collections;
using cherrydev;
using Havengard.Core;
using UnityEngine;

namespace Havengard.Dialogue
{
    /// <summary>Adapter between Havengard NPCs and the cherrydev DialogBehaviour.</summary>
    public class DialogueManager : MonoBehaviour
    {
        [SerializeField] private DialogBehaviour dialogBehaviour;

        public static DialogueManager Instance { get; private set; }
        public static bool IsActive { get { return Instance != null && Instance.isRunning; } }

        private bool isRunning;
        private bool playerLocked;

        private void Awake()
        {
            Instance = this;
            if (dialogBehaviour == null)
                Debug.LogError("DialogueManager: DialogBehaviour is not assigned.", this);
        }

        private void OnDestroy()
        {
            if (playerLocked) { playerLocked = false; PlayerInputLock.Release(); }
            if (Instance == this) Instance = null;
        }

        /// <param name="blocksPlayer">False for ambient chatter that must not freeze the player.</param>
        public bool Begin(DialogNodeGraph graph, IDialogueListener listener, bool blocksPlayer = true)
        {
            if (isRunning || dialogBehaviour == null) return false;
            if (graph == null || graph.NodesList == null || graph.NodesList.Count == 0) return false;

            isRunning = true;
            StartCoroutine(BeginNextFrame(graph, listener, blocksPlayer));
            return true;
        }

        private IEnumerator BeginNextFrame(DialogNodeGraph graph, IDialogueListener listener, bool blocksPlayer)
        {
            // The interact key is usually also a "next sentence" key. Waiting a frame stops
            // the press that opened the dialogue from instantly skipping the first line.
            yield return null;

            if (blocksPlayer)
            {
                PlayerInputLock.Acquire();
                playerLocked = true;
            }

            if (listener != null)
            {
                listener.OnDialogueStarted();
                listener.RegisterDialogueFunctions(new FunctionBinder(dialogBehaviour));
            }

            dialogBehaviour.StartDialog(graph, null, variables => Finish(listener, variables));
        }

        private void Finish(IDialogueListener listener, DialogVariablesHandler variables)
        {
            isRunning = false;

            // Listener first: if it opens a shop, that shop takes its own lock before ours is released.
            if (listener != null)
                listener.OnDialogueEnded(new CherryDialogueVariables(variables));

            if (playerLocked)
            {
                playerLocked = false;
                PlayerInputLock.Release();
            }
        }

        private sealed class FunctionBinder : IDialogueFunctionBinder
        {
            private readonly DialogBehaviour behaviour;
            public FunctionBinder(DialogBehaviour behaviour) { this.behaviour = behaviour; }
            public void Bind(string functionName, Action action) { behaviour.BindExternalFunction(functionName, action); }
        }
    }
}