using System.Collections;
using cherrydev;
using UnityEngine;

namespace Havengard.Dialogue
{
    /// <summary>
    /// Shows the answer buttons as soon as the NPC's sentence has finished,
    /// instead of waiting for an extra "next" press.
    /// </summary>
    public class DialogueAnswerAutoReveal : MonoBehaviour
    {
        [SerializeField] private DialogBehaviour dialogBehaviour;

        private bool dialogActive;
        private bool answerShown;

        private void OnEnable()
        {
            dialogBehaviour.OnDialogStarted.AddListener(HandleStarted);
            dialogBehaviour.OnDialogFinished.AddListener(HandleFinished);
            dialogBehaviour.SentenceNodeActivated += HandleSentenceActivated;
            dialogBehaviour.AnswerNodeActivated += HandleAnswerActivated;
            dialogBehaviour.SentenceEnded += HandleSentenceEnded;
        }

        private void OnDisable()
        {
            dialogBehaviour.OnDialogStarted.RemoveListener(HandleStarted);
            dialogBehaviour.OnDialogFinished.RemoveListener(HandleFinished);
            dialogBehaviour.SentenceNodeActivated -= HandleSentenceActivated;
            dialogBehaviour.AnswerNodeActivated -= HandleAnswerActivated;
            dialogBehaviour.SentenceEnded -= HandleSentenceEnded;
        }

        private void HandleStarted() { dialogActive = true; }
        private void HandleFinished() { dialogActive = false; }
        private void HandleSentenceActivated() { answerShown = false; }
        private void HandleAnswerActivated() { answerShown = true; }

        private void HandleSentenceEnded()
        {
            SentenceNode sentence = dialogBehaviour.CurrentSentenceNode;
            AnswerNode answer = sentence != null ? sentence.ChildNode as AnswerNode : null;

            if (answer != null)
                StartCoroutine(RevealNextFrame(sentence, answer));
        }

        private IEnumerator RevealNextFrame(SentenceNode sentence, AnswerNode answer)
        {
            // Deferred a frame so we don't stop the package's own typing coroutine from inside its event.
            yield return null;

            // Bail out if the dialogue ended, moved on, or the player already advanced to the answers.
            if (!dialogActive || answerShown || dialogBehaviour.CurrentSentenceNode != sentence)
                yield break;

            dialogBehaviour.SetCurrentNodeAndHandleDialogGraph(answer);
        }
    }
}