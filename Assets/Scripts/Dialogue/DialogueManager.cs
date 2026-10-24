using System;
using System.Collections;
using System.Collections.Generic;
using Havengard.Core;
using UnityEngine;
using Yarn.Unity;

namespace Havengard.Dialogue
{
    /// <summary>Adapter between Havengard NPCs and the Yarn Spinner DialogueRunner.</summary>
    public class DialogueManager : MonoBehaviour
    {
        [SerializeField] private DialogueRunner dialogueRunner;

        public static DialogueManager Instance { get; private set; }
        public static bool IsActive { get { return Instance != null && Instance.isRunning; } }
        public static event Action<bool> ActiveChanged;

        private readonly List<string> boundCommands = new List<string>();
        private IDialogueListener currentListener;
        private bool isRunning;
        private bool playerLocked;

        private void Awake()
        {
            Instance = this;
            if (dialogueRunner == null)
            {
                Debug.LogError("DialogueManager: DialogueRunner is not assigned.", this);
                return;
            }

            dialogueRunner.onDialogueComplete.AddListener(HandleDialogueComplete);
        }

        private void OnDestroy()
        {
            if (dialogueRunner != null)
                dialogueRunner.onDialogueComplete.RemoveListener(HandleDialogueComplete);

            UnbindCommands();

            if (playerLocked) { playerLocked = false; PlayerInputLock.Release(); }
            if (Instance == this) Instance = null;
        }

        /// <param name="nodeName">Yarn node title, e.g. "Vendor".</param>
        /// <param name="blocksPlayer">False for ambient chatter that must not freeze the player.</param>
        public bool Begin(string nodeName, IDialogueListener listener, bool blocksPlayer = true)
        {
            if (isRunning) { Debug.LogWarning("DialogueManager: already running.", this); return false; }
            if (dialogueRunner == null) { Debug.LogError("DialogueManager: DialogueRunner is not assigned.", this); return false; }
            if (string.IsNullOrEmpty(nodeName)) { Debug.LogWarning("DialogueManager: node name is empty.", this); return false; }

            isRunning = true;
            StartCoroutine(BeginNextFrame(nodeName, listener, blocksPlayer));
            return true;
        }

        private IEnumerator BeginNextFrame(string nodeName, IDialogueListener listener, bool blocksPlayer)
        {
            // The interact key is usually also a "next line" key. Waiting a frame stops
            // the press that opened the dialogue from instantly skipping the first line.
            yield return null;

            Debug.Log("[DialogueManager] Starting node '" + nodeName + "'", this);

            if (blocksPlayer)
            {
                PlayerInputLock.Acquire();
                playerLocked = true;
            }

            if (ActiveChanged != null) ActiveChanged(true);

            currentListener = listener;

            if (listener != null)
            {
                listener.OnDialogueStarted();
                listener.RegisterDialogueFunctions(new FunctionBinder(this));
            }

            dialogueRunner.StartDialogue(nodeName);
            Debug.Log("[DialogueManager] StartDialogue called. Runner running = " + dialogueRunner.IsDialogueRunning, this);
        }

        private void HandleDialogueComplete()
        {
            Debug.Log("[DialogueManager] Dialogue complete.", this);

            if (!isRunning) return;

            isRunning = false;
            IDialogueListener listener = currentListener;
            currentListener = null;

            UnbindCommands();

            if (listener != null)
                listener.OnDialogueEnded(new YarnDialogueVariables(dialogueRunner.VariableStorage));

            if (ActiveChanged != null) ActiveChanged(false);

            if (playerLocked)
            {
                playerLocked = false;
                PlayerInputLock.Release();
            }
        }

        private void BindCommand(string commandName, Action action)
        {
            dialogueRunner.AddCommandHandler(commandName, action);
            boundCommands.Add(commandName);
        }

        private void UnbindCommands()
        {
            if (dialogueRunner != null)
            {
                foreach (string commandName in boundCommands)
                    dialogueRunner.RemoveCommandHandler(commandName);
            }

            boundCommands.Clear();
        }

        private sealed class FunctionBinder : IDialogueFunctionBinder
        {
            private readonly DialogueManager manager;
            public FunctionBinder(DialogueManager manager) { this.manager = manager; }
            public void Bind(string functionName, Action action) { manager.BindCommand(functionName, action); }
        }
    }
}