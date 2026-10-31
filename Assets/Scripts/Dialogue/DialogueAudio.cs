using UnityEngine;

namespace Havengard.Dialogue
{
    /// <summary>Small UI SFX player kept separate from the view (SRP).</summary>
    [RequireComponent(typeof(AudioSource))]
    public class DialogueAudio : MonoBehaviour
    {
        public static DialogueAudio Instance { get; private set; }

        [Header("Clips")]
        [SerializeField] private AudioClip openClip;
        [SerializeField] private AudioClip closeClip;
        [SerializeField] private AudioClip typeBlipClip;
        [SerializeField] private AudioClip advanceClip;
        [SerializeField] private AudioClip skipClip;
        [SerializeField] private AudioClip choiceShowClip;
        [SerializeField] private AudioClip choiceHoverClip;
        [SerializeField] private AudioClip choiceSelectClip;
        [SerializeField] private AudioClip choiceDeclineClip;
        [SerializeField] private AudioClip choiceAcceptClip;

        [Header("Settings")]
        [Range(0f, 1f)] [SerializeField] private float volume = 0.8f;
        [SerializeField] private Vector2 blipPitchRange = new Vector2(0.95f, 1.05f);
        [Tooltip("Play a blip every N characters.")]
        [SerializeField] private int blipEveryNChars = 2;

        private AudioSource source;
        private int charCount;
        private int openedFrame = -1;
        private int lastChoiceFrame = -1;

        private void Awake()
        {
            Instance = this;
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void NotifyDialogueStarted()
        {
            openedFrame = Time.frameCount;
            Play(openClip);
        }

        public void NotifyDialogueFinished()
        {
            Play(closeClip);
        }

        public void NotifyLineStarted()
        {
            charCount = 0;

            // The first line shares a frame with the open sound, and a line right after a choice
            // shares one with the choice sound, so don't also play "advance" for either.
            bool afterOpen = Time.frameCount == openedFrame;
            bool afterChoice = Time.frameCount - lastChoiceFrame <= 2;
            if (!afterOpen && !afterChoice) Play(advanceClip);
        }

        public void NotifyCharRevealed()
        {
            if (typeBlipClip == null) return;
            if (charCount++ % Mathf.Max(1, blipEveryNChars) != 0) return;

            source.pitch = Random.Range(blipPitchRange.x, blipPitchRange.y);
            source.PlayOneShot(typeBlipClip, volume);
            source.pitch = 1f;
        }

        public void NotifyTextSkipped() { Play(skipClip); }
        public void NotifyChoicesShown() { Play(choiceShowClip); }

        public void PlayChoiceHover() { Play(choiceHoverClip); }
        
        public void PlayChoiceSelect() { Play(choiceSelectClip); }

        public void PlayChoiceAccept() { Play(choiceAcceptClip != null ? choiceAcceptClip : choiceSelectClip); }

        public void PlayChoiceDecline() { Play(choiceDeclineClip != null ? choiceDeclineClip : choiceSelectClip); }

        private void Play(AudioClip clip)
        {
            if (clip != null) source.PlayOneShot(clip, volume);
        }
    }
}