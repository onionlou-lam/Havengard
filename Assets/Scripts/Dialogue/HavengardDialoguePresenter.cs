#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Yarn.Unity;
using Havengard.Core.Heroes;

namespace Havengard.Dialogue
{
    /// <summary>
    /// Yarn Spinner presenter that drives the existing speech bubble UI.
    /// </summary>
    public class HavengardDialoguePresenter : DialoguePresenterBase
    {
        [Serializable]
        public struct Portrait
        {
            public string characterName;
            public Sprite sprite;
        }

        [Header("UI")]
        [SerializeField] private GameObject _root;
        [SerializeField] private SentencePanel _sentencePanel;
        [SerializeField] private AnswerPanel _answerPanel;

        [Header("Portraits")]
        [SerializeField] private List<Portrait> _portraits = new();

        [Header("Typewriter")]
        [SerializeField, Min(1f)] private float _charactersPerSecond = 40f;

        [Header("Optional Effects")]
        [SerializeField] private DialoguePortraitBob _portraitBob;

        private static DialogueAudio? Audio => DialogueAudio.Instance;

        private void Awake()
        {
            SetVisible(false);
        }

        public override YarnTask OnDialogueStartedAsync()
        {
            SetVisible(true);
            Debug.Log("[Presenter] Dialogue started.", this);
            if (Audio != null) Audio.NotifyDialogueStarted();
            return YarnTask.CompletedTask;
        }

        public override YarnTask OnDialogueCompleteAsync()
        {
            SetVisible(false);
            if (Audio != null) Audio.NotifyDialogueFinished();
            return YarnTask.CompletedTask;
        }

        public override async YarnTask RunLineAsync(LocalizedLine line, LineCancellationToken token)
        {
            _answerPanel.gameObject.SetActive(false);
            _sentencePanel.gameObject.SetActive(true);

            string text = line.TextWithoutCharacterName.Text;
            string speaker = line.CharacterName ?? string.Empty;
            string displayName = speaker;
            Sprite? portrait;

            if (string.Equals(speaker.Trim(), PlayerSpeaker, StringComparison.OrdinalIgnoreCase))
            {
                HeroData hero = HeroDialogue.Current;
                if (hero != null && !string.IsNullOrEmpty(hero.heroName)) displayName = hero.heroName;

                // Hero's own portrait first, then a "Player" entry in the Portraits list as a default.
                portrait = hero != null && hero.portrait != null ? hero.portrait : FindPortrait(speaker);
            }
            else
            {
                portrait = FindPortrait(speaker);
            }

            _sentencePanel.Setup(displayName, text, portrait);
            _sentencePanel.ResetDialogText();

            if (Audio != null) Audio.NotifyLineStarted();
            if (_portraitBob != null) _portraitBob.NotifyLineStarted();

            Debug.Log("[Presenter] Running line: " + line.TextWithoutCharacterName.Text, this);

            // Typewriter effect, skipped if the player requests "hurry up" or "next"
            float delay = 1f / _charactersPerSecond;
            bool skipped = false;

            IsTyping = true;

            for (int i = 0; i < text.Length; i++)
            {
                if (token.IsHurryUpRequested || token.IsNextLineRequested)
                {
                    skipped = true;
                    break;
                }

                _sentencePanel.IncreaseMaxVisibleCharacters();
                if (Audio != null) Audio.NotifyCharRevealed();

                try
                {
                    await YarnTask.Delay(TimeSpan.FromSeconds(delay), token.HurryUpToken);
                }
                catch (OperationCanceledException)
                {
                    // Skip was requested mid-delay. Show the full line below.
                    skipped = true;
                    break;
                }
            }

            IsTyping = false;

            _sentencePanel.ShowFullDialogText(text);

            if (skipped && Audio != null)
            {
                Audio.NotifyTextSkipped();
            }

            // Wait for the player to advance
            await YarnTask.WaitUntil(() => token.IsNextLineRequested);
        }

        public override async YarnTask<DialogueOption?> RunOptionsAsync(
            DialogueOption[] dialogueOptions, CancellationToken cancellationToken)
        {
            var available = new List<DialogueOption>();
            foreach (DialogueOption candidate in dialogueOptions)
            {
                if (candidate.IsAvailable) available.Add(candidate);
            }

            _answerPanel.gameObject.SetActive(true);
            _answerPanel.SetUpButtons(available.Count);

            if ( Audio != null) Audio.NotifyChoicesShown();

            DialogueOption? selected = null;

            for (int i = 0; i < available.Count; i++)
            {
                DialogueOption option = available[i];
                ChoiceKind kind = GetChoiceKind(option);

                var button = _answerPanel.GetButtonByIndex(i);

                _answerPanel.GetButtonTextByIndex(i).text = option.Line.TextWithoutCharacterName.Text;
                button.interactable = true;
                button.gameObject.SetActive(true);
                button.onClick.AddListener(() =>
                {
                    if (Audio != null)
                    {
                        switch (kind)
                        {
                            case ChoiceKind.Accept: Audio.PlayChoiceAccept(); break;
                            case ChoiceKind.Decline: Audio.PlayChoiceDecline(); break;
                            default: Audio.PlayChoiceSelect(); break;
                        }
                    }

                    selected = option;
                });
            }

            IsShowingOptions = true;

            await YarnTask.WaitUntil(() => selected != null || cancellationToken.IsCancellationRequested);

            IsShowingOptions = false;
            _answerPanel.DisableAllButtons();
            return selected;
        }

        /// <summary>Same lookup the dialogue uses, so other UIs (e.g. the vendor shop) show the identical portrait.</summary>
        public Sprite? GetPortrait(string characterName)
        {
            return FindPortrait(characterName);
        }

        private Sprite? FindPortrait(string? characterName)
        {
            if (string.IsNullOrWhiteSpace(characterName))
            {
                return null;
            }

            string wanted = characterName.Trim();

            foreach (Portrait portrait in _portraits)
            {
                if (string.Equals(portrait.characterName?.Trim(), wanted, StringComparison.OrdinalIgnoreCase))
                {
                    return portrait.sprite;
                }
            }

            return null;
        }

        private void SetVisible(bool visible)
        {
            if (_root != null)
            {
                _root.SetActive(visible);
            }
        }

        public bool IsTyping { get; private set; }
        public bool IsShowingOptions { get; private set; }

        private const string AcceptTag = "accept";
        private const string DeclineTag = "decline";
        private const string PlayerSpeaker = "Player";

        private enum ChoiceKind { Generic, Accept, Decline }

        private static ChoiceKind GetChoiceKind(DialogueOption option)
        {
            string[]? metadata = option.Line.Metadata;
            if (metadata == null) return ChoiceKind.Generic;

            foreach (string raw in metadata)
            {
                string tag = raw.TrimStart('#').Trim();

                if (string.Equals(tag, DeclineTag, StringComparison.OrdinalIgnoreCase)) return ChoiceKind.Decline;
                if (string.Equals(tag, AcceptTag, StringComparison.OrdinalIgnoreCase)) return ChoiceKind.Accept;
            }

            return ChoiceKind.Generic;
        }

        private static bool IsDecline(DialogueOption option)
        {
            string[]? metadata = option.Line.Metadata;
            return metadata != null && Array.IndexOf(metadata, DeclineTag) >= 0;
        }
    }
}