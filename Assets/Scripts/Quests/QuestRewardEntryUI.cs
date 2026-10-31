using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Havengard.Quests
{
    /// <summary>One reward row in the popup. Clickable only when the player has to choose.</summary>
    public class QuestRewardEntryUI : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private Button button;

        public void Setup(Sprite sprite, string text, UnityAction onClick)
        {
            if (icon != null)
            {
                icon.sprite = sprite;
                icon.enabled = sprite != null;
            }

            if (label != null) label.text = text;

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                if (onClick != null) button.onClick.AddListener(onClick);
                button.enabled = onClick != null;
            }
        }

        public void Lock()
        {
            if (button != null) button.enabled = false;
        }
    }
}