using cherrydev;
using UnityEngine;

namespace Havengard.Dialogue
{
    /// <summary>Shows a gently pulsing "continue" marker once a line has finished (not before an answer prompt).</summary>
    public class DialogueContinueIndicator : MonoBehaviour
    {
        [SerializeField] private DialogBehaviour dialogBehaviour;
        [SerializeField] private GameObject indicator;

        [Header("Pulse")]
        [SerializeField] private float pulseSpeed = 4f;
        [Range(0f, 0.5f)] [SerializeField] private float pulseAmount = 0.12f;

        private Vector3 baseScale;
        private float pulseStartTime;
        private bool pulsing;

        private void Awake()
        {
            baseScale = indicator.transform.localScale;
        }

        private void OnEnable()
        {
            dialogBehaviour.SentenceStarted += Hide;
            dialogBehaviour.SentenceEnded += Show;
            dialogBehaviour.AnswerNodeActivated += Hide;
            dialogBehaviour.OnDialogFinished.AddListener(Hide);
            Hide();
        }

        private void OnDisable()
        {
            dialogBehaviour.SentenceStarted -= Hide;
            dialogBehaviour.SentenceEnded -= Show;
            dialogBehaviour.AnswerNodeActivated -= Hide;
            dialogBehaviour.OnDialogFinished.RemoveListener(Hide);
        }

        private void Show()
        {
            SentenceNode sentence = dialogBehaviour.CurrentSentenceNode;
            if (sentence != null && sentence.ChildNode is AnswerNode) return;

            indicator.SetActive(true);
            pulseStartTime = Time.unscaledTime;
            pulsing = true;
        }

        private void Hide()
        {
            pulsing = false;
            indicator.transform.localScale = baseScale;
            indicator.SetActive(false);
        }

        private void Update()
        {
            if (!pulsing) return;

            // 0 ? 1 ? 0 smooth wave that starts at the normal scale.
            float wave = 0.5f - 0.5f * Mathf.Cos((Time.unscaledTime - pulseStartTime) * pulseSpeed);
            indicator.transform.localScale = baseScale * (1f + pulseAmount * wave);
        }
    }
}