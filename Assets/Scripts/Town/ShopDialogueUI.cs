using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Havengard.Town
{
    /// <summary>
    /// Simple dialogue UI for shop conversations.
    /// </summary>
    public class ShopDialogueUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField]
        [Tooltip("Text component for the dialogue message")]
        private TextMeshProUGUI dialogueText;

        [SerializeField]
        [Tooltip("Container for dialogue option buttons")]
        private Transform optionsContainer;

        [SerializeField]
        [Tooltip("Prefab for dialogue option buttons")]
        private GameObject optionButtonPrefab;

        [Header("Animation")]
        [SerializeField]
        [Tooltip("Optional animator for dialogue panel")]
        private Animator panelAnimator;

        public void ShowDialogue(string message, DialogueOption[] options)
        {
            // Set dialogue text
            if (dialogueText != null)
            {
                dialogueText.text = message;
            }

            // Clear existing options
            ClearOptions();

            // Create option buttons
            foreach (var option in options)
            {
                CreateOptionButton(option);
            }

            // Play show animation
            if (panelAnimator != null)
            {
                panelAnimator.SetTrigger("Show");
            }
        }

        private void ClearOptions()
        {
            if (optionsContainer == null) return;

            foreach (Transform child in optionsContainer)
            {
                Destroy(child.gameObject);
            }
        }

        private void CreateOptionButton(DialogueOption option)
        {
            if (optionButtonPrefab == null || optionsContainer == null)
            {
                Debug.LogWarning("[ShopDialogueUI] Missing option button prefab or container!");
                return;
            }

            GameObject buttonObj = Instantiate(optionButtonPrefab, optionsContainer);
            Button button = buttonObj.GetComponent<Button>();
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

            if (buttonText != null)
            {
                buttonText.text = option.text;
            }

            if (button != null)
            {
                button.onClick.AddListener(() => option.onSelected?.Invoke());
            }
        }
    }
}