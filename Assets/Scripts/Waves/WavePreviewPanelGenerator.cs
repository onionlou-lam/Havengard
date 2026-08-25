#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

namespace Havengard.Waves.UI.Editor
{
    /// <summary>
    /// Editor utility to auto-generate Wave Preview Panel UI hierarchy
    /// Usage: Right-click in Hierarchy → Wave System → Create Wave Preview Panel
    /// </summary>
    public static class WavePreviewPanelGenerator
    {
        [MenuItem("GameObject/Wave System/Create Wave Preview Panel (Condensed)", false, 10)]
        public static void CreateWavePreviewPanel()
        {
            // Find or create Canvas
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();

                var scaler = canvasObj.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
            }

            // Create main panel (CONDENSED SIZE)
            GameObject panelObj = CreatePanel("WavePreviewPanel", canvas.transform);
            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.02f, 0.35f);
            panelRect.anchorMax = new Vector2(0.28f, 0.65f); // Smaller, more condensed
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Add WavePreviewPanel component
            WavePreviewPanel previewPanel = panelObj.AddComponent<WavePreviewPanel>();

            // Background
            Image bgImage = panelObj.GetComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

            // Add CanvasGroup for fading
            panelObj.AddComponent<CanvasGroup>();

            // Vertical layout for main panel
            VerticalLayoutGroup mainLayout = panelObj.AddComponent<VerticalLayoutGroup>();
            mainLayout.padding = new RectOffset(15, 15, 15, 15);
            mainLayout.spacing = 10;
            mainLayout.childControlWidth = true;
            mainLayout.childControlHeight = false;

            // === HEADER (CONDENSED) ===
            TextMeshProUGUI waveNumberText = CreateText("WaveNumberText", panelObj.transform, "Wave 1: The First Strike", 24, TextAlignmentOptions.Center);
            waveNumberText.fontStyle = FontStyles.Bold;
            waveNumberText.color = new Color(1f, 0.8f, 0.2f);

            TextMeshProUGUI totalEnemiesText = CreateText("TotalEnemiesText", panelObj.transform, "15 Enemies", 18, TextAlignmentOptions.Center);
            totalEnemiesText.color = new Color(0.9f, 0.5f, 0.5f);

            // === ENEMY LIST (COMPACT) ===
            GameObject scrollViewObj = CreateScrollView("EnemyScrollView", panelObj.transform);
            RectTransform scrollRect = scrollViewObj.GetComponent<RectTransform>();
            scrollRect.sizeDelta = new Vector2(0, 150); // Shorter height

            GameObject contentObj = scrollViewObj.transform.Find("Viewport/Content").gameObject;
            VerticalLayoutGroup contentLayout = contentObj.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 3;
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = false;

            // === REWARDS (SINGLE LINE) ===
            TextMeshProUGUI rewardsText = CreateText("RewardsText", panelObj.transform, "💰50  ⭐25  💎10", 18, TextAlignmentOptions.Center);
            rewardsText.color = new Color(0.2f, 1f, 0.5f);

            // === TIMER ===
            GameObject timerObj = CreateUIObject("TimerObject", panelObj.transform);
            TextMeshProUGUI timerText = CreateText("TimerText", timerObj.transform, "Auto-start in: 10s", 16, TextAlignmentOptions.Center);
            timerText.color = Color.white;

            // === START BUTTON ===
            GameObject buttonObj = new GameObject("StartWaveButton");
            buttonObj.transform.SetParent(panelObj.transform, false);
            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(0, 40);

            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.7f, 0.2f, 1f);

            Button startButton = buttonObj.AddComponent<Button>();

            GameObject buttonTextObj = new GameObject("Text");
            buttonTextObj.transform.SetParent(buttonObj.transform, false);
            TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "Start Wave";
            buttonText.fontSize = 20;
            buttonText.fontStyle = FontStyles.Bold;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;
            RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.sizeDelta = Vector2.zero;

            // === CREATE ENEMY ITEM PREFAB (COMPACT) ===
            GameObject enemyItemPrefab = CreateCompactEnemyItemPrefab();

            // === WIRE UP REFERENCES ===
            SetPrivateField(previewPanel, "panelRoot", panelObj);
            SetPrivateField(previewPanel, "waveNumberText", waveNumberText);
            SetPrivateField(previewPanel, "totalEnemiesText", totalEnemiesText);
            SetPrivateField(previewPanel, "enemyListContainer", contentObj.transform);
            SetPrivateField(previewPanel, "enemyPreviewItemPrefab", enemyItemPrefab);
            SetPrivateField(previewPanel, "timerText", timerText);
            SetPrivateField(previewPanel, "timerObject", timerObj);
            SetPrivateField(previewPanel, "startWaveButton", startButton);
            SetPrivateField(previewPanel, "startButtonText", buttonText);
            SetPrivateField(previewPanel, "rewardsText", rewardsText);

            EditorUtility.SetDirty(previewPanel);
            Selection.activeGameObject = panelObj;

            Debug.Log("[WavePreviewPanelGenerator] Condensed Wave Preview Panel created!");
            Debug.Log("Enemy Item Prefab created at: Assets/Prefabs/UI/WavePreviewEnemyItem.prefab");
        }

        // ========== HELPER METHODS ==========

        private static GameObject CreatePanel(string name, Transform parent)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            Image image = obj.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            return obj;
        }

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1);

            return obj;
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, string text, int fontSize, TextAlignmentOptions alignment)
        {
            GameObject obj = CreateUIObject(name, parent);

            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = Color.white;

            return tmp;
        }

        private static GameObject CreateScrollView(string name, Transform parent)
        {
            GameObject scrollObj = CreateUIObject(name, parent);

            Image scrollImage = scrollObj.AddComponent<Image>();
            scrollImage.color = new Color(0.05f, 0.05f, 0.05f, 0.8f);

            ScrollRect scrollRect = scrollObj.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            // Viewport
            GameObject viewportObj = CreateUIObject("Viewport", scrollObj.transform);
            RectTransform viewportRect = viewportObj.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;

            Mask mask = viewportObj.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            viewportObj.AddComponent<Image>();

            // Content
            GameObject contentObj = CreateUIObject("Content", viewportObj.transform);
            RectTransform contentRect = contentObj.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = Vector2.one;
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 0);

            ContentSizeFitter fitter = contentObj.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;

            return scrollObj;
        }

        private static GameObject CreateCompactEnemyItemPrefab()
        {
            // Create prefab folder if it doesn't exist
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/UI"))
                AssetDatabase.CreateFolder("Assets/Prefabs", "UI");

            GameObject itemObj = new GameObject("WavePreviewEnemyItem");

            RectTransform rect = itemObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 35); // Smaller height

            Image bgImage = itemObj.AddComponent<Image>();
            bgImage.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

            HorizontalLayoutGroup layout = itemObj.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 3, 3);
            layout.spacing = 8;
            layout.childControlWidth = false;
            layout.childControlHeight = true;
            layout.childAlignment = TextAnchor.MiddleLeft;

            // Icon (smaller)
            GameObject iconObj = new GameObject("IconImage");
            iconObj.transform.SetParent(itemObj.transform, false);
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(30, 30);
            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.color = Color.white;

            // Name (smaller font) - USING TMP
            GameObject nameObj = new GameObject("NameText");
            nameObj.transform.SetParent(itemObj.transform, false);
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = "Enemy";
            nameText.fontSize = 14;
            nameText.color = Color.white;
            LayoutElement nameLayout = nameObj.AddComponent<LayoutElement>();
            nameLayout.flexibleWidth = 1;

            // Count - USING TMP
            GameObject countObj = new GameObject("CountText");
            countObj.transform.SetParent(itemObj.transform, false);
            TextMeshProUGUI countText = countObj.AddComponent<TextMeshProUGUI>();
            countText.text = "x5";
            countText.fontSize = 16;
            countText.fontStyle = FontStyles.Bold;
            countText.color = new Color(1f, 0.8f, 0.2f);
            countText.alignment = TextAlignmentOptions.Right;
            RectTransform countRect = countObj.GetComponent<RectTransform>();
            countRect.sizeDelta = new Vector2(50, 0);

            // Add component
            WavePreviewEnemyItem itemComponent = itemObj.AddComponent<WavePreviewEnemyItem>();

            SetPrivateField(itemComponent, "iconImage", iconImage);
            SetPrivateField(itemComponent, "nameText", nameText);
            SetPrivateField(itemComponent, "countText", countText);
            SetPrivateField(itemComponent, "backgroundImage", bgImage);

            // Save as prefab
            string prefabPath = "Assets/Prefabs/UI/WavePreviewEnemyItem.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(itemObj, prefabPath);

            Object.DestroyImmediate(itemObj);

            return prefab;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            target.GetType()
                .GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(target, value);
        }
    }
}
#endif