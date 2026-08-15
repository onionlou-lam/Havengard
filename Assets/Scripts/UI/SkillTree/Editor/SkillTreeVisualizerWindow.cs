using Havengard.Abilities;
using Havengard.Core.Progression;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SkillTreeVisualizerWindow : EditorWindow
{
    private static PlayerClass targetClass;
    private Vector2 scrollPosition;
    private float zoom = 1f;
    private Vector2 panOffset = Vector2.zero;

    private const float NODE_SIZE = 80f;
    private const float GRID_SIZE = 120f;

    public static void ShowWindow(PlayerClass playerClass)
    {
        targetClass = playerClass;
        var window = GetWindow<SkillTreeVisualizerWindow>("Skill Tree Visualizer");
        window.minSize = new Vector2(600, 400);
        window.Show();
    }

    private void OnGUI()
    {
        if (targetClass == null)
        {
            EditorGUILayout.HelpBox("No PlayerClass selected. Open from PlayerClass inspector.", MessageType.Info);
            return;
        }

        DrawToolbar();
        DrawSkillTree();
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        GUILayout.Label($"Class: {targetClass.className}", EditorStyles.boldLabel);

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Reset View", EditorStyles.toolbarButton, GUILayout.Width(80)))
        {
            zoom = 1f;
            panOffset = Vector2.zero;
        }

        GUILayout.Label("Zoom:", EditorStyles.miniLabel);
        zoom = GUILayout.HorizontalSlider(zoom, 0.5f, 2f, GUILayout.Width(100));

        EditorGUILayout.EndHorizontal();
    }

    private void DrawSkillTree()
    {
        if (targetClass.classAbilities == null || targetClass.classAbilities.Length == 0)
        {
            EditorGUILayout.HelpBox("No abilities in this class.", MessageType.Info);
            return;
        }

        Rect viewRect = new Rect(0, 20, position.width, position.height - 20);

        // Calculate bounds
        Vector2 min = Vector2.one * float.MaxValue;
        Vector2 max = Vector2.one * float.MinValue;

        foreach (var ability in targetClass.classAbilities)
        {
            Vector2 pos = ability.treePosition * GRID_SIZE;
            min = Vector2.Min(min, pos);
            max = Vector2.Max(max, pos);
        }

        Vector2 contentSize = (max - min) * zoom + Vector2.one * 200;

        scrollPosition = GUI.BeginScrollView(viewRect, scrollPosition, new Rect(0, 0, contentSize.x, contentSize.y));

        // Draw connections first (behind nodes)
        DrawConnections();

        // Draw nodes
        for (int i = 0; i < targetClass.classAbilities.Length; i++)
        {
            DrawAbilityNode(i, targetClass.classAbilities[i]);
        }

        GUI.EndScrollView();
    }

    private void DrawConnections()
    {
        for (int i = 0; i < targetClass.classAbilities.Length; i++)
        {
            var ability = targetClass.classAbilities[i];
            Vector2 fromPos = GetNodeScreenPosition(ability.treePosition);

            foreach (int prereqIndex in ability.prerequisiteIndices)
            {
                if (prereqIndex >= 0 && prereqIndex < targetClass.classAbilities.Length)
                {
                    var prereqAbility = targetClass.classAbilities[prereqIndex];
                    Vector2 toPos = GetNodeScreenPosition(prereqAbility.treePosition);

                    Handles.color = new Color(0.5f, 0.7f, 1f, 0.5f);
                    Handles.DrawLine(fromPos, toPos);

                    // Draw arrow
                    Vector2 direction = (fromPos - toPos).normalized;
                    Vector2 arrowPos = fromPos - direction * (NODE_SIZE * zoom / 2);
                    DrawArrow(arrowPos, direction, 10f * zoom);
                }
            }
        }
    }

    private void DrawAbilityNode(int index, ClassAbility classAbility)
    {
        Vector2 screenPos = GetNodeScreenPosition(classAbility.treePosition);
        Rect nodeRect = new Rect(screenPos.x - NODE_SIZE * zoom / 2, screenPos.y - NODE_SIZE * zoom / 2,
            NODE_SIZE * zoom, NODE_SIZE * zoom);

        // Background
        Color nodeColor = classAbility.ability != null ?
            new Color(0.2f, 0.4f, 0.8f) : new Color(0.5f, 0.2f, 0.2f);

        if (classAbility.HasPrerequisites())
            nodeColor = Color.Lerp(nodeColor, Color.green, 0.3f);

        EditorGUI.DrawRect(nodeRect, nodeColor);
        GUI.Box(nodeRect, "", EditorStyles.helpBox);

        // Icon
        if (classAbility.ability != null && classAbility.ability.icon != null)
        {
            Texture2D icon = AssetPreview.GetAssetPreview(classAbility.ability.icon.texture);
            if (icon != null)
            {
                Rect iconRect = new Rect(nodeRect.x + 5, nodeRect.y + 5, nodeRect.width - 10, nodeRect.height - 30);
                GUI.DrawTexture(iconRect, icon);
            }
        }

        // Label
        GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel);
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.normal.textColor = Color.white;
        labelStyle.fontSize = Mathf.RoundToInt(10 * zoom);

        Rect labelRect = new Rect(nodeRect.x, nodeRect.yMax - 25 * zoom, nodeRect.width, 20 * zoom);
        GUI.Label(labelRect, $"[{index}] {(classAbility.ability != null ? classAbility.ability.abilityName : "Empty")}", labelStyle);

        // Level requirement
        Rect levelRect = new Rect(nodeRect.x, nodeRect.y + 2, 25 * zoom, 15 * zoom);
        GUIStyle levelStyle = new GUIStyle(EditorStyles.miniLabel);
        levelStyle.normal.textColor = Color.yellow;
        levelStyle.fontSize = Mathf.RoundToInt(10 * zoom);
        GUI.Label(levelRect, $"L{classAbility.requiredLevel}", levelStyle);

        // Click to select
        Event e = Event.current;
        if (e.type == EventType.MouseDown && nodeRect.Contains(e.mousePosition))
        {
            Selection.activeObject = classAbility.ability;
            e.Use();
        }
    }

    private Vector2 GetNodeScreenPosition(Vector2 gridPosition)
    {
        return gridPosition * GRID_SIZE * zoom + panOffset + Vector2.one * 100;
    }

    private void DrawArrow(Vector2 position, Vector2 direction, float size)
    {
        Vector2 right = new Vector2(-direction.y, direction.x);
        Vector2 p1 = position - direction * size + right * size * 0.5f;
        Vector2 p2 = position - direction * size - right * size * 0.5f;

        Handles.DrawLine(position, p1);
        Handles.DrawLine(position, p2);
    }
}