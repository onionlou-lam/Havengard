using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Havengard.UI
{
    /// <summary>
    /// Generic N-way tab switcher: pairs toggle buttons with content panels,
    /// showing exactly one panel at a time.
    /// </summary>
    public class TabToggleUI : MonoBehaviour
    {
        [System.Serializable]
        public struct Tab
        {
            public Button tabButton;
            [Tooltip("Optional - background Image to tint. Leave empty to skip tinting.")]
            public Image background;
            [Tooltip("Optional - label whose colour is set explicitly so it never disappears.")]
            public TextMeshProUGUI label;
            public GameObject contentPanel;
        }

        [SerializeField] private Tab[] tabs;
        [SerializeField] private int defaultTabIndex = 0;

        [Header("Background Colors")]
        [SerializeField] private Color activeColor = Color.white;
        [SerializeField] private Color inactiveColor = new Color(0.6f, 0.6f, 0.6f, 1f);

        [Header("Label Colors")]
        [SerializeField] private Color activeLabelColor = Color.black;
        [SerializeField] private Color inactiveLabelColor = new Color(0.25f, 0.25f, 0.25f, 1f);

        private void Start()
        {
            for (int i = 0; i < tabs.Length; i++)
            {
                int index = i; // capture for closure

                if (tabs[i].tabButton != null)
                    tabs[i].tabButton.onClick.AddListener(() => SelectTab(index));

                ValidateTab(i);
            }

            SelectTab(defaultTabIndex);
        }

        private void ValidateTab(int i)
        {
            var panel = tabs[i].contentPanel;

            if (panel == null)
            {
                Debug.LogWarning($"[TabToggleUI] Tab {i} has no Content Panel assigned.", this);
                return;
            }

            if (!panel.scene.IsValid())
                Debug.LogWarning($"[TabToggleUI] Tab {i} Content Panel is a prefab asset, not a scene object.", this);

            if (tabs[i].tabButton != null && tabs[i].tabButton.transform.IsChildOf(panel.transform))
                Debug.LogWarning($"[TabToggleUI] Tab {i} button is inside its own Content Panel - hiding it would hide the button.", this);
        }

        public void SelectTab(int index)
        {
            if (tabs == null || tabs.Length == 0)
            {
                Debug.LogWarning("[TabToggleUI] Tabs array is empty - nothing to switch.", this);
                return;
            }

            for (int i = 0; i < tabs.Length; i++)
            {
                bool isActive = i == index;

                if (tabs[i].contentPanel != null)
                {
                    tabs[i].contentPanel.SetActive(isActive);
                    Debug.Log($"[TabToggleUI] Tab {i} -> '{tabs[i].contentPanel.name}' set {(isActive ? "ON" : "OFF")} (activeInHierarchy={tabs[i].contentPanel.activeInHierarchy})", tabs[i].contentPanel);
                }

                if (tabs[i].background != null)
                    tabs[i].background.color = isActive ? activeColor : inactiveColor;

                if (tabs[i].label != null)
                    tabs[i].label.color = isActive ? activeLabelColor : inactiveLabelColor;
            }
        }
    }
}