using Havengard.Dialogue;
using UnityEngine;
using Havengard.Quests;
namespace Havengard.Interactions
{
    /// <summary>
    /// Base NPC. Talking = run a Yarn dialogue node. Subclasses add behaviour by overriding
    /// RegisterDialogueFunctions (script -> NPC) and OnDialogueEnded (NPC reacts to final state).
    /// </summary>
    public class NPCInteractable : MonoBehaviour, IInteractable, IDialogueListener, IQuestDialogueSource
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

        [Header("Quest Reward")]
        [Tooltip("Popup used by <<claim_reward>>. Found in the scene if left empty.")]
        [SerializeField] private QuestRewardPanel rewardPanel;

        private bool claimRewardAfterDialogue;

        // IInteractable
        public virtual string GetInteractionPrompt() { return prompt; }
        public virtual string GetInteractionKey() { return key; }
        public virtual bool CanInteract() { return !string.IsNullOrEmpty(dialogueNode) && !DialogueManager.IsActive; }
        public Transform GetTooltipTransform() { return tooltipAnchor != null ? tooltipAnchor : transform; }

        public virtual void Interact()
        {
            if (DialogueManager.Instance != null)
                DialogueManager.Instance.Begin(dialogueNode, this, blocksPlayer);
        }

        // IDialogueListener
        public virtual void OnDialogueStarted() { }

        public virtual void RegisterDialogueFunctions(IDialogueFunctionBinder binder)
        {
            binder.Bind("start_quest", OnQuestRequested);
            binder.Bind("claim_reward", () => claimRewardAfterDialogue = true);
        }

        public virtual void OnDialogueEnded(IDialogueVariables variables)
        {
            if (!claimRewardAfterDialogue) return;
            claimRewardAfterDialogue = false;
            OpenRewardPanel();
        }

        protected virtual void OpenRewardPanel()
        {
            if (quest == null)
            {
                Debug.LogWarning("[NPCInteractable] <<claim_reward>> ran but no QuestDefinition is assigned on " + name, this);
                return;
            }

            if (rewardPanel == null) rewardPanel = FindFirstObjectByType<QuestRewardPanel>();
            if (rewardPanel == null)
            {
                Debug.LogWarning("[NPCInteractable] No QuestRewardPanel in the scene.", this);
                return;
            }

            rewardPanel.Show(quest);
        }

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

        // IQuestDialogueSource
        public string QuestId { get { return quest != null ? quest.questId : null; } }
    }
}