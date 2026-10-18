using UnityEngine;

namespace Havengard.Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        [Tooltip("Must implement IDialogueView (e.g. DialogueView).")]
        [SerializeField] private MonoBehaviour viewBehaviour;

        public static DialogueManager Instance { get; private set; }
        public static bool IsActive { get { return Instance != null && Instance.current != null; } }

        private IDialogueView view;
        private DialogueData current;
        private IDialogueListener listener;
        private int nodeIndex;

        private void Awake()
        {
            Instance = this;
            view = viewBehaviour as IDialogueView;
            if (view == null)
            {
                Debug.LogError("DialogueManager: viewBehaviour does not implement IDialogueView.", this);
                return;
            }
            view.AdvanceRequested += HandleAdvance;
            view.ChoiceSelected += HandleChoice;
        }

        private void OnDestroy()
        {
            if (view != null)
            {
                view.AdvanceRequested -= HandleAdvance;
                view.ChoiceSelected -= HandleChoice;
            }
            if (Instance == this) Instance = null;
        }

        public void Begin(DialogueData data, IDialogueListener dialogueListener)
        {
            if (IsActive || data == null || data.nodes.Count == 0 || view == null) return;

            current = data;
            listener = dialogueListener;
            if (listener != null) listener.OnDialogueStarted();

            view.Open();
            ShowNode(0);
        }

        private void ShowNode(int index)
        {
            if (index < 0 || index >= current.nodes.Count)
            {
                End();
                return;
            }

            nodeIndex = index;
            DialogueNode node = current.nodes[index];
            Sprite portrait = node.portraitOverride != null ? node.portraitOverride : current.portrait;

            view.ShowLine(current.speakerName, portrait, node.text);

            if (listener != null)
            {
                if (!string.IsNullOrEmpty(node.eventId)) listener.OnDialogueEvent(node.eventId);
                if (node.createsQuest) listener.OnQuestRequested(node.questId);
            }
        }

        private void HandleAdvance()
        {
            if (current == null) return;
            DialogueNode node = current.nodes[nodeIndex];

            if (node.HasChoices)
            {
                view.ShowChoices(node.choices);
                return;
            }

            // -2 = default: go to the next node in the list
            ShowNode(node.nextNodeIndex == -2 ? nodeIndex + 1 : node.nextNodeIndex);
        }

        private void HandleChoice(int choiceIndex)
        {
            if (current == null) return;
            DialogueChoice choice = current.nodes[nodeIndex].choices[choiceIndex];

            if (listener != null && !string.IsNullOrEmpty(choice.eventId))
                listener.OnDialogueEvent(choice.eventId);

            ShowNode(choice.nextNodeIndex);
        }

        private void End()
        {
            view.Close();
            IDialogueListener finished = listener;
            current = null;
            listener = null;
            if (finished != null) finished.OnDialogueEnded();
        }
    }
}