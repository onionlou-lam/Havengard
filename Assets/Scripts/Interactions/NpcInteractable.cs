using Havengard.Dialogue;
using UnityEngine;

namespace Havengard.Interactions
{
    /// <summary>
    /// Base NPC. Talking = start dialogue. Subclasses override the hooks
    /// (e.g. VendorNpc opens a shop on "open_shop", GuildNpc opens follower allocation).
    /// </summary>
    public class NpcInteractable : MonoBehaviour, IInteractable, IDialogueListener
    {
        [SerializeField] protected DialogueData dialogue;
        [SerializeField] private string prompt = "Talk";
        [SerializeField] private string key = "E";
        [SerializeField] private Transform tooltipAnchor;

        public virtual string GetInteractionPrompt() { return prompt; }
        public virtual string GetInteractionKey() { return key; }
        public virtual bool CanInteract() { return dialogue != null && !DialogueManager.IsActive; }
        public Transform GetTooltipTransform() { return tooltipAnchor != null ? tooltipAnchor : transform; }

        public virtual void Interact()
        {
            if (DialogueManager.Instance != null)
                DialogueManager.Instance.Begin(dialogue, this);
        }

        public virtual void OnDialogueStarted() { }
        public virtual void OnDialogueEvent(string eventId) { }
        public virtual void OnQuestRequested(string questId) { }
        public virtual void OnDialogueEnded() { }
    }
}