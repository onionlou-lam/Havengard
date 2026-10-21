using cherrydev;
using Havengard.Dialogue;
using UnityEngine;

namespace Havengard.Interactions
{
    /// <summary>
    /// Base NPC. Talking = run a dialogue graph. Subclasses add behaviour by overriding
    /// RegisterDialogueFunctions (graph -> NPC) and OnDialogueEnded (NPC reacts to final state).
    /// </summary>
    public class NpcInteractable : MonoBehaviour, IInteractable, IDialogueListener
    {
        [Header("Dialogue")]
        [SerializeField] protected DialogNodeGraph dialogue;
        [Tooltip("Off for ambient/background chatter that must not freeze the player.")]
        [SerializeField] private bool blocksPlayer = true;

        [Header("Interaction")]
        [SerializeField] private string prompt = "Talk";
        [SerializeField] private string key = "E";
        [SerializeField] private Transform tooltipAnchor;

        [Header("Quest")]
        [Tooltip("Quest/expedition entry created when the graph calls the 'start_quest' function.")]
        [SerializeField] protected string questId;

        public virtual string GetInteractionPrompt() { return prompt; }
        public virtual string GetInteractionKey() { return key; }
        public virtual bool CanInteract() { return dialogue != null && !DialogueManager.IsActive; }
        public Transform GetTooltipTransform() { return tooltipAnchor != null ? tooltipAnchor : transform; }

        public virtual void Interact()
        {
            if (DialogueManager.Instance != null)
                DialogueManager.Instance.Begin(dialogue, this, blocksPlayer);
        }

        public virtual void OnDialogueStarted() { }

        public virtual void RegisterDialogueFunctions(IDialogueFunctionBinder binder)
        {
            binder.Bind("start_quest", () => OnQuestRequested(questId));
        }

        public virtual void OnDialogueEnded(IDialogueVariables variables) { }

        protected virtual void OnQuestRequested(string id)
        {
            // Hook up to QuestSystem when we do quests.
            Debug.Log("[NpcInteractable] Quest requested: " + id, this);
        }
    }
}