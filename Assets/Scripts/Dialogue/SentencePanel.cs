using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Havengard.Dialogue
{
    /// <summary>Speech bubble text, speaker name and portrait. Driven by HavengardDialoguePresenter.</summary>
    public class SentencePanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _dialogNameText;
        [SerializeField] private TextMeshProUGUI _dialogText;
        [SerializeField] private Image _dialogCharacterImage;

        private string _currentFullText;

        /// <summary>
        /// Sets the dialog text's max visible characters to zero.
        /// </summary>
        public void ResetDialogText()
        {
            _dialogText.maxVisibleCharacters = 0;
            _currentFullText = string.Empty;
        }

        /// <summary>
        /// Shows the whole dialog text at once.
        /// </summary>
        public void ShowFullDialogText(string text)
        {
            _currentFullText = text;
            _dialogText.text = text;
            _dialogText.maxVisibleCharacters = text.Length;
        }

        /// <summary>
        /// Reveals one more character.
        /// </summary>
        public void IncreaseMaxVisibleCharacters() => _dialogText.maxVisibleCharacters++;

        /// <summary>
        /// Assigns the speaker name, portrait sprite and dialog text.
        /// </summary>
        public void Setup(string characterName, string text, Sprite sprite)
        {
            _dialogNameText.text = characterName;
            _dialogText.text = text;
            _currentFullText = text;

            if (_dialogCharacterImage == null)
            {
                Debug.LogError("[SentencePanel] Dialog Character Image is not assigned.", this);
                return;
            }

            if (sprite == null)
            {
                _dialogCharacterImage.color = new Color(_dialogCharacterImage.color.r,
                    _dialogCharacterImage.color.g, _dialogCharacterImage.color.b, 0f);
                return;
            }

            _dialogCharacterImage.sprite = sprite;
            _dialogCharacterImage.enabled = true;
            _dialogCharacterImage.color = new Color(_dialogCharacterImage.color.r,
                _dialogCharacterImage.color.g, _dialogCharacterImage.color.b, 1f);

            Debug.Log("[SentencePanel] Portrait set on '" + _dialogCharacterImage.name +
                "', activeInHierarchy = " + _dialogCharacterImage.gameObject.activeInHierarchy, _dialogCharacterImage);
        }
    }
}