using UnityEngine;

namespace Havengard.Dialogue
{
    /// <summary>Small UI SFX player kept separate from the view (SRP).</summary>
    [RequireComponent(typeof(AudioSource))]
    public class DialogueAudio : MonoBehaviour
    {
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
        [Range(0f, 1f)][SerializeField] private float volume = 0.8f;
        [Tooltip("Random pitch range for the typewriter blip so it doesn't sound robotic.")]
        [SerializeField] private Vector2 blipPitchRange = new Vector2(0.95f, 1.05f);
        [Tooltip("Play a blip every N visible characters.")]
        [SerializeField] private int blipEveryNChars = 2;

        private AudioSource source;

        public int BlipEveryNChars { get { return Mathf.Max(1, blipEveryNChars); } }

        private void Awake() { source = GetComponent<AudioSource>(); source.playOnAwake = false; }

        public void PlayOpen() { Play(openClip); }
        public void PlayClose() { Play(closeClip); }
        public void PlayAdvance() { Play(advanceClip); }
        public void PlaySkip() { Play(skipClip); }
        public void PlayChoiceShow() { Play(choiceShowClip); }
        public void PlayChoiceHover() { Play(choiceHoverClip); }
        public void PlayChoiceSelect() { Play(choiceSelectClip); }

        public void PlayBlip()
        {
            if (typeBlipClip == null) return;
            source.pitch = Random.Range(blipPitchRange.x, blipPitchRange.y);
            source.PlayOneShot(typeBlipClip, volume);
            source.pitch = 1f;
        }

        private void Play(AudioClip clip)
        {
            if (clip != null) source.PlayOneShot(clip, volume);
        }
    }
}