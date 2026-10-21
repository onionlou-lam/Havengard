#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using cherrydev;
using UnityEngine;
using Yarn.Unity;

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
            _sentencePanel.Setup(line.CharacterName ?? string.Empty, text, FindPortrait(line.CharacterName));
            _sentencePanel.ResetDialogText();

            if (Audio != null) Audio.NotifyLineStarted();
            if (_portraitBob != null) _portraitBob.NotifyLineStarted();

            Debug.Log("[Presenter] Running line: " + line.TextWithoutCharacterName.Text, this);

            // Typewriter effect, skipped if the player requests "hurry up" or "next"
            float delay = 1f / _charactersPerSecond;
            bool skipped = false;

            for (int i = 0; i < text.Length; i++)
            {
                if (token.IsHurryUpRequested || token.IsNextLineRequested)
                {
                    skipped = true;
                    break;
                }

                _sentencePanel.IncreaseMaxVisibleCharacters();
                if (Audio != null) Audio.NotifyCharRevealed();

                await YarnTask.Delay(TimeSpan.FromSeconds(delay), token.HurryUpToken);
            }

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
            _answerPanel.gameObject.SetActive(true);
            _answerPanel.SetUpButtons(dialogueOptions.Length);

            if (Audio != null) Audio.NotifyChoicesShown();

            DialogueOption? selected = null;

            for (int i = 0; i < dialogueOptions.Length; i++)
            {
                DialogueOption option = dialogueOptions[i];
                var button = _answerPanel.GetButtonByIndex(i);

                _answerPanel.GetButtonTextByIndex(i).text = option.Line.TextWithoutCharacterName.Text;
                button.interactable = option.IsAvailable;
                button.gameObject.SetActive(true);
                button.onClick.AddListener(() =>
                {
                    if (Audio != null) Audio.PlayChoiceSelect();
                    selected = option;
                });
            }

            await YarnTask.WaitUntil(() => selected != null || cancellationToken.IsCancellationRequested);

            _answerPanel.DisableAllButtons();
            return selected;
        }

        private Sprite? FindPortrait(string? characterName)
        {
            if (string.IsNullOrEmpty(characterName))
            {
                return null;
            }

            foreach (Portrait portrait in _portraits)
            {
                if (string.Equals(portrait.characterName, characterName, StringComparison.OrdinalIgnoreCase))
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
    }
}