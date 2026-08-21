#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace Havengard.DebugTools.Editor
{
    public class DebugMenuSetup : EditorWindow
    {
        [MenuItem("Havengard/Create Debug Menu")]
        public static void CreateDebugMenu()
        {
            // Create canvas
            GameObject canvasObj = new GameObject("HavengardDebugMenu");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();

            // Create main panel
            GameObject panelObj = new GameObject("MenuPanel");
            panelObj.transform.SetParent(canvasObj.transform, false);
            
            RectTransform panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.05f, 0.1f);
            panelRect.anchorMax = new Vector2(0.95f, 0.9f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

            // Create scroll view
            GameObject scrollView = new GameObject("ScrollView");
            scrollView.transform.SetParent(panelObj.transform, false);
            
            RectTransform scrollRect = scrollView.AddComponent<RectTransform>();
            scrollRect.anchorMin = Vector2.zero;
            scrollRect.anchorMax = Vector2.one;
            scrollRect.offsetMin = new Vector2(10, 10);
            scrollRect.offsetMax = new Vector2(-10, -10);
            
            ScrollRect scroll = scrollView.AddComponent<ScrollRect>();
            scrollView.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 1f);

            // Create content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(scrollView.transform, false);
            
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            
            VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.childControlHeight = false;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.spacing = 10;
            vlg.padding = new RectOffset(10, 10, 10, 10);
            
            ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.content = contentRect;
            scroll.viewport = scrollRect;

            // Add HavengardDebugMenu component
            HavengardDebugMenu debugMenu = canvasObj.AddComponent<HavengardDebugMenu>();
            
            // Set references
            var serializedObject = new SerializedObject(debugMenu);
            serializedObject.FindProperty("debugCanvas").objectReferenceValue = canvas;
            serializedObject.FindProperty("menuPanel").objectReferenceValue = panelObj;
            serializedObject.ApplyModifiedProperties();

            // Add sections
            CreateSection(content.transform, "PLAYER", new string[]
            {
                "Add Gold", "Add Celestium", "Add XP", "Add Skill Points", "Level Up", "Reset Skills"
            });

            CreateSection(content.transform, "COMBAT", new string[]
            {
                "Spawn Enemy Unit", "Spawn Boss Unit", "Kill All", "Start Wave", "Skip Wave"
            });

            CreateSection(content.transform, "ABILITIES", new string[]
            {
                "Reset Cooldowns", "Infinite Mana", "Test Ability", "Toggle Damage Numbers"
            });

            CreateSection(content.transform, "WORLD", new string[]
            {
                "Change Area/Scene", "Game Speed", "Toggle NavMesh", "Show AI Paths"
            });

            CreateSection(content.transform, "RESET TEST STATE", new string[]
            {
                "Reset Player", "Reset Skills", "Reset Inventory", "Reset Everything"
            });

            // Hide by default
            canvasObj.SetActive(false);

            // Select the created object
            Selection.activeGameObject = canvasObj;
            
            Debug.Log("[DebugMenuSetup] Havengard Debug Menu created! Press F1 in play mode to toggle.");
        }

        private static void CreateSection(Transform parent, string title, string[] buttonLabels)
        {
            // Section header
            GameObject header = new GameObject($"Header_{title}");
            header.transform.SetParent(parent, false);
            
            RectTransform headerRect = header.AddComponent<RectTransform>();
            headerRect.sizeDelta = new Vector2(0, 40);
            
            Image headerImage = header.AddComponent<Image>();
            headerImage.color = new Color(0.2f, 0.3f, 0.4f, 1f);
            
            GameObject headerText = new GameObject("Text");
            headerText.transform.SetParent(header.transform, false);
            
            Text text = headerText.AddComponent<Text>();
            text.text = $"[ {title} ]";
            text.font = UnityEngine.Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 18;
            text.fontStyle = FontStyle.Bold;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            
            RectTransform textRect = headerText.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            // Buttons
            foreach (string label in buttonLabels)
            {
                CreateButton(parent, label);
            }

            // Spacer
            GameObject spacer = new GameObject("Spacer");
            spacer.transform.SetParent(parent, false);
            RectTransform spacerRect = spacer.AddComponent<RectTransform>();
            spacerRect.sizeDelta = new Vector2(0, 20);
        }

        private static void CreateButton(Transform parent, string label)
        {
            GameObject button = new GameObject($"Btn_{label.Replace(" ", "")}");
            button.transform.SetParent(parent, false);
            
            RectTransform buttonRect = button.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(0, 40);
            
            Image buttonImage = button.AddComponent<Image>();
            buttonImage.color = new Color(0.25f, 0.25f, 0.25f, 1f);
            
            Button buttonComponent = button.AddComponent<Button>();
            ColorBlock colors = buttonComponent.colors;
            colors.normalColor = new Color(0.25f, 0.25f, 0.25f, 1f);
            colors.highlightedColor = new Color(0.35f, 0.35f, 0.35f, 1f);
            colors.pressedColor = new Color(0.15f, 0.35f, 0.55f, 1f);
            buttonComponent.colors = colors;
            
            GameObject buttonText = new GameObject("Text");
            buttonText.transform.SetParent(button.transform, false);
            
            Text text = buttonText.AddComponent<Text>();
            text.text = label;
            text.font = UnityEngine.Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 16;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            
            RectTransform textRect = buttonText.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 0);
            textRect.offsetMax = new Vector2(-10, 0);
        }
    }
}
#endif