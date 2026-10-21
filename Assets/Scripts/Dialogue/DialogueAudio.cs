using cherrydev;
using UnityEngine;

namespace Havengard.Dialogue
{
    /// <summary>Small UI SFX player kept separate from the view (SRP).</summary>
    [RequireComponent(typeof(AudioSource))]
    public class DialogueAudio : MonoBehaviour
    {
        public static DialogueAudio Instance { get; private set; }

        [SerializeField] private DialogBehaviour dialogBehaviour;

        [Header("Clips")]
        [SerializeField] private AudioClip openClip;
        [SerializeField] private AudioClip closeClip;
        [SerializeField] private AudioClip typeBlipClip;
        [SerializeField] private AudioClip advanceClip;
        [SerializeField] private AudioClip skipClip;
        [SerializeField] private AudioClip choiceShowClip;
        [SerializeField] private AudioClip choiceHoverClip;
        [SerializeField] private AudioClip choiceSelectClip;

        [Header("Settings")]
        [Range(0f, 1f)] [SerializeField] private float volume = 0.8f;
        [SerializeField] private Vector2 blipPitchRange = new Vector2(0.95f, 1.05f);
        [Tooltip("Play a blip every N characters.")]
        [SerializeField] private int blipEveryNChars = 2;

        private AudioSource source;
        private int charCount;
        private int openedFrame = -1;

        private void Awake()
        {
            Instance = this;
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
        }

        private void OnEnable()
        {
            if (dialogBehaviour == null) return;
            dialogBehaviour.OnDialogStarted.AddListener(HandleStarted);
            dialogBehaviour.OnDialogFinished.AddListener(HandleFinished);
            dialogBehaviour.SentenceNodeActivated += HandleSentenceActivated;
            dialogBehaviour.SentenceStarted += HandleSentenceStarted;
            dialogBehaviour.DialogTextCharWrote += HandleCharWrote;
            dialogBehaviour.DialogTextSkipped += HandleTextSkipped;
            dialogBehaviour.AnswerNodeActivated += HandleAnswersShown;
        }

        private void OnDisable()
        {
            if (Instance == this) Instance = null;
            if (dialogBehaviour == null) return;
            dialogBehaviour.OnDialogStarted.RemoveListener(HandleStarted);
            dialogBehaviour.OnDialogFinished.RemoveListener(HandleFinished);
            dialogBehaviour.SentenceNodeActivated -= HandleSentenceActivated;
            dialogBehaviour.SentenceStarted -= HandleSentenceStarted;
            dialogBehaviour.DialogTextCharWrote -= HandleCharWrote;
            dialogBehaviour.DialogTextSkipped -= HandleTextSkipped;
            dialogBehaviour.AnswerNodeActivated -= HandleAnswersShown;
        }

        private void HandleStarted() { openedFrame = Time.frameCount; Play(openClip); }
        private void HandleFinished() { Play(closeClip); }

        private void HandleSentenceActivated()
        {
            // The first sentence shares a frame with the open sound, so don't also play "advance".
            if (Time.frameCount != openedFrame) Play(advanceClip);
        }

        private void HandleSentenceStarted() { charCount = 0; }

        private void HandleCharWrote()
        {
            if (typeBlipClip == null) return;
            if (charCount++ % Mathf.Max(1, blipEveryNChars) != 0) return;

            source.pitch = Random.Range(blipPitchRange.x, blipPitchRange.y);
            source.PlayOneShot(typeBlipClip, volume);
            source.pitch = 1f;
        }

        private void HandleTextSkipped(string _) { Play(skipClip); }
        private void HandleAnswersShown() { Play(choiceShowClip); }

        public void PlayChoiceHover() { Play(choiceHoverClip); }
        public void PlayChoiceSelect() { Play(choiceSelectClip); }

        private void Play(AudioClip clip)
        {
            if (clip != null) source.PlayOneShot(clip, volume);
        }
    }
}