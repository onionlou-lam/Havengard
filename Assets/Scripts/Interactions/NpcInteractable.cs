using cherrydev;
using Havengard.Dialogue;
using UnityEngine;
using Havengard.Quests;
namespace Havengard.Interactions
{
    /// <summary>
    /// Base NPC. Talking = run a Yarn dialogue node. Subclasses add behaviour by overriding
    /// RegisterDialogueFunctions (script -> NPC) and OnDialogueEnded (NPC reacts to final state).
    /// </summary>
    public class NPCInteractable : MonoBehaviour, IInteractable, IDialogueListener
    {
        [Header("Dialogue")]
        [Tooltip("Title of the Yarn node to run, e.g. 'Vendor'.")]
        [SerializeField] protected string dialogueNode;
        [Tooltip("Off for ambient/background chatter that must not freeze the player.")]
        [SerializeField] private bool blocksPlayer = true;

        [Header("Interaction")]
        [SerializeField] private string prompt = "Talk";
        [SerializeField] private string key = "E";
        [SerializeField] private Transform tooltipAnchor;

        [Header("Quest")]
        [Tooltip("Added to the expedition list when the Yarn script runs <<start_quest>>.")]
        [SerializeField] protected QuestDefinition quest;
        [Tooltip("Optional. Node used instead once the quest is already on the expedition list.")]
        [SerializeField] protected string questActiveNode;

        /// <summary>Picks the follow-up node once the quest has been given, so it isn't offered twice.</summary>
        protected virtual string GetDialogueNode()
        {
            bool alreadyGiven = quest != null && ExpeditionLog.Instance != null && ExpeditionLog.Instance.Contains(quest);
            return alreadyGiven && !string.IsNullOrEmpty(questActiveNode) ? questActiveNode : dialogueNode;
        }

        public virtual string GetInteractionPrompt() { return prompt; }
        public virtual string GetInteractionKey() { return key; }
        public virtual bool CanInteract() { return !string.IsNullOrEmpty(GetDialogueNode()) && !DialogueManager.IsActive; }
        public Transform GetTooltipTransform() { return tooltipAnchor != null ? tooltipAnchor : transform; }

        public virtual void Interact()
        {
            if (DialogueManager.Instance != null)
                DialogueManager.Instance.Begin(GetDialogueNode(), this, blocksPlayer);
        }

        public virtual void OnDialogueStarted() { }

        public virtual void RegisterDialogueFunctions(IDialogueFunctionBinder binder)
        {
            binder.Bind("start_quest", 
                );
        }

        public virtual void OnDialogueEnded(IDialogueVariables variables) { }

        protected virtual void OnQuestRequested()
        {
            if (quest == null)
            {
                Debug.LogWarning("[NPCInteractable] <<start_quest>> ran but no QuestDefinition is assigned on " + name, this);
                return;
            }

            if (ExpeditionLog.Instance == null)
            {
                Debug.LogWarning("[NPCInteractable] No ExpeditionLog in the scene.", this);
                return;
            }

            if (!ExpeditionLog.Instance.TryAdd(quest))
                Debug.Log("[NPCInteractable] Quest already on the expedition list: " + quest.questId, this);
        }
    }
}