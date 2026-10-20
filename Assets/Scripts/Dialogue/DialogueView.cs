using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Havengard.Dialogue
{
    public class DialogueView : MonoBehaviour, IDialogueView
    {
        [Header("UI")]
        [SerializeField] private GameObject root;
        [SerializeField] private Image portraitImage;
        [SerializeField] private TMPro.TextMeshProUGUI speakerText;
        [SerializeField] private TMPro.TextMeshProUGUI bodyText;
        [SerializeField] private GameObject continueIndicator;

        [Header("Choices")]
        [SerializeField] private RectTransform choiceContainer;
        [SerializeField] private Button choiceButtonPrefab;

        [Header("Typewriter")]
        [SerializeField] private float charsPerSecond = 40f;
        [SerializeField] private KeyCode advanceKey = KeyCode.E;
        [SerializeField] private KeyCode alternateAdvanceKey = KeyCode.Space;

        [Header("Audio")]
        [SerializeField] private DialogueAudio audioPlayer;

        public event Action AdvanceRequested;
        public event Action<int> ChoiceSelected;

        private Coroutine typing;
        private string fullText;
        private bool lineComplete;
        private bool choicesShowing;
        private readonly List<GameObject> spawnedChoices = new List<GameObject>();
        private int openedFrame = -1;

        private void Awake() { if (root != null) root.SetActive(false); }

        public void Open()
        {
            openedFrame = Time.frameCount;
            root.SetActive(true);
            ClearChoices();
            if (audioPlayer != null) audioPlayer.PlayOpen();
        }

        public void Close()
        {
            StopTyping();
            ClearChoices();
            root.SetActive(false);
            if (audioPlayer != null) audioPlayer.PlayClose();
        }

        public void ShowLine(string speaker, Sprite portrait, string text)
        {
            ClearChoices();
            speakerText.text = speaker;
            portraitImage.sprite = portrait;
            portraitImage.enabled = portrait != null;

            fullText = text;
            StopTyping();
            typing = StartCoroutine(TypeRoutine());
        }

        public void ShowChoices(IList<DialogueChoice> choices)
        {
            ClearChoices();
            choicesShowing = true;
            if (continueIndicator != null) continueIndicator.SetActive(false);
            if (audioPlayer != null) audioPlayer.PlayChoiceShow();

            for (int i = 0; i < choices.Count; i++)
            {
                int captured = i;
                Button button = Instantiate(choiceButtonPrefab, choiceContainer);
                button.GetComponentInChildren<Text>().text = choices[i].text;
                button.onClick.AddListener(() => SelectChoice(captured));

                if (audioPlayer != null)
                {
                    var trigger = button.gameObject.AddComponent<EventTrigger>();
                    var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
                    entry.callback.AddListener(_ => audioPlayer.PlayChoiceHover());
                    trigger.triggers.Add(entry);
                }
                spawnedChoices.Add(button.gameObject);
            }
        }

        private void Update()
        {
            if (!root.activeSelf || choicesShowing || Time.frameCount == openedFrame) return;

            if (Input.GetKeyDown(advanceKey) || Input.GetKeyDown(alternateAdvanceKey) || Input.GetMouseButtonDown(0))
            {
                if (!lineComplete)
                {
                    CompleteLineInstantly();
                    if (audioPlayer != null) audioPlayer.PlaySkip();
                }
                else
                {
                    if (audioPlayer != null) audioPlayer.PlayAdvance();
                    Action handler = AdvanceRequested;
                    if (handler != null) handler();
                }
            }
        }

        private IEnumerator TypeRoutine()
        {
            lineComplete = false;
            if (continueIndicator != null) continueIndicator.SetActive(false);
            bodyText.text = string.Empty;

            float delay = 1f / Mathf.Max(1f, charsPerSecond);
            int blipEvery = audioPlayer != null ? audioPlayer.BlipEveryNChars : 2;

            for (int i = 0; i < fullText.Length; i++)
            {
                bodyText.text = fullText.Substring(0, i + 1);
                if (audioPlayer != null && i % blipEvery == 0 && !char.IsWhiteSpace(fullText[i]))
                    audioPlayer.PlayBlip();
                yield return new WaitForSeconds(delay);
            }
            FinishLine();
        }

        private void CompleteLineInstantly()
        {
            StopTyping();
            bodyText.text = fullText;
            FinishLine();
        }

        private void FinishLine()
        {
            typing = null;
            lineComplete = true;
            if (continueIndicator != null) continueIndicator.SetActive(true);
        }

        private void StopTyping()
        {
            if (typing != null) { StopCoroutine(typing); typing = null; }
        }

        private void SelectChoice(int index)
        {
            if (audioPlayer != null) audioPlayer.PlayChoiceSelect();
            ClearChoices();
            Action<int> handler = ChoiceSelected;
            if (handler != null) handler(index);
        }

        private void ClearChoices()
        {
            choicesShowing = false;
            foreach (GameObject go in spawnedChoices) Destroy(go);
            spawnedChoices.Clear();
        }
    }
}