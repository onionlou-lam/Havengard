using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Havengard.Dialogue
{
    /// <summary>Holds the answer buttons for a choice. Driven by HavengardDialoguePresenter.</summary>
    public class AnswerPanel : MonoBehaviour
    {
        [SerializeField] private Button _answerButtonPrefab;
        [SerializeField] private Transform _parentTransform;

        private readonly List<Button> _buttons = new List<Button>();
        private readonly List<TextMeshProUGUI> _buttonTexts = new List<TextMeshProUGUI>();

        /// <summary>
        /// Destroys any existing buttons and creates the requested number of new ones.
        /// </summary>
        public void SetUpButtons(int amount)
        {
            DeleteAllExistingButtons();

            if (_answerButtonPrefab == null || _parentTransform == null)
            {
                Debug.LogError("[AnswerPanel] Answer Button Prefab or Parent Transform is not assigned.", this);
                return;
            }

            for (int i = 0; i < amount; i++)
            {
                Button answerButton = Instantiate(_answerButtonPrefab, _parentTransform);

                _buttons.Add(answerButton);
                _buttonTexts.Add(answerButton.GetComponentInChildren<TextMeshProUGUI>());
            }
        }

        public Button GetButtonByIndex(int index) => _buttons[index];

        public TextMeshProUGUI GetButtonTextByIndex(int index) => _buttonTexts[index];

        /// <summary>
        /// Hides all buttons.
        /// </summary>
        public void DisableAllButtons()
        {
            foreach (Button button in _buttons)
            {
                if (button != null) button.gameObject.SetActive(false);
            }
        }

        private void DeleteAllExistingButtons()
        {
            foreach (Button button in _buttons)
            {
                if (button != null) Destroy(button.gameObject);
            }

            _buttons.Clear();
            _buttonTexts.Clear();
        }
    }
}