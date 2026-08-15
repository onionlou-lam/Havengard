#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Havengard.Abilities;
using Havengard.Core.Progression;

namespace Havengard.UI.SkillTree
{
    /// <summary>
    /// Visual grid editor for positioning skill tree nodes with drag-and-drop
    /// </summary>
    public class SkillTreeGridEditor : EditorWindow
    {
        private PlayerClass targetClass;
        private Vector2 scrollPosition;
        private Vector2 detailsScrollPosition;

        // Grid settings - MUST match SkillTreeUI.cs
        private const int GRID_SIZE = 20;      // Keep for max bounds
        private const float CELL_SIZE = 50f;   // Visual scale (half of runtime 100px)
        private const float NODE_SIZE = 42f;   // Scaled down from 100px runtime
        private const float SUB_NODE_SIZE = 25f; // Scaled down from 60px runtime

        // ✅ NEW: Display grid that matches runtime (15x12 visible area)
        private const int DISPLAY_GRID_WIDTH = 15;
        private const int DISPLAY_GRID_HEIGHT = 12;

        // Visual settings
        private Color gridColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        private Color nodeColor = new Color(0.2f, 0.6f, 1f, 1f);
        private Color selectedNodeColor = new Color(1f, 0.8f, 0.2f, 1f);
        private Color subNodeColor = new Color(0.4f, 0.8f, 0.4f, 1f);
        private Color selectedSubNodeColor = new Color(1f, 1f, 0.3f, 1f);
        private Color prerequisiteLineColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);
        private Color subSkillLineColor = new Color(0.4f, 0.8f, 0.4f, 0.6f);

        private int selectedAbilityIndex = -1;
        private int selectedSubSkillIndex = -1;
        private bool isDragging = false;
        private bool isDraggingSubSkill = false;
        private Vector2 dragOffset;

        [MenuItem("Havengard/Skill Tree Grid Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<SkillTreeGridEditor>("Skill Tree Grid");
            window.minSize = new Vector2(1000, 600);
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                GUILayout.Label("Skill Tree Grid Editor", EditorStyles.boldLabel);

                if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    if (targetClass != null)
                    {
                        EditorUtility.SetDirty(targetClass);
                        AssetDatabase.SaveAssets();
                        Debug.Log($"Saved {targetClass.name}");
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            // Class selection
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                PlayerClass newClass = (PlayerClass)EditorGUILayout.ObjectField(
                    "Target Class", targetClass, typeof(PlayerClass), false);

                if (newClass != targetClass)
                {
                    targetClass = newClass;
                    selectedAbilityIndex = -1;
                    selectedSubSkillIndex = -1;
                }
            }
            EditorGUILayout.EndVertical();

            if (targetClass == null)
            {
                EditorGUILayout.HelpBox("Select a PlayerClass to edit its skill tree grid", MessageType.Info);
                return;
            }

            if (targetClass.classAbilities == null || targetClass.classAbilities.Length == 0)
            {
                EditorGUILayout.HelpBox("This class has no abilities defined", MessageType.Warning);
                return;
            }

            EditorGUILayout.Space();

            // Split view: Grid + Details
            EditorGUILayout.BeginHorizontal();
            {
                // Left side: Grid view
                DrawGridView();

                // Right side: Details panel
                DrawDetailsPanel();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawGridView()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.65f));
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
                {
                    Rect gridRect = GUILayoutUtility.GetRect(
                        GRID_SIZE * CELL_SIZE,
                        GRID_SIZE * CELL_SIZE,
                        GUILayout.ExpandWidth(false),
                        GUILayout.ExpandHeight(false)
                    );

                    DrawGrid(gridRect);
                    DrawPrerequisiteLines(gridRect);
                    DrawSubSkillLines(gridRect);
                    DrawNodes(gridRect);
                    DrawSubSkills(gridRect);
                    HandleNodeInteraction(gridRect);
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawGrid(Rect gridRect)
        {
            Handles.BeginGUI();
            Handles.color = gridColor;

            // Draw grid lines
            for (int x = 0; x <= GRID_SIZE; x++)
            {
                float xPos = gridRect.x + x * CELL_SIZE;
                Handles.DrawLine(
                    new Vector3(xPos, gridRect.y),
                    new Vector3(xPos, gridRect.y + gridRect.height)
                );
            }

            for (int y = 0; y <= GRID_SIZE; y++)
            {
                float yPos = gridRect.y + y * CELL_SIZE;
                Handles.DrawLine(
                    new Vector3(gridRect.x, yPos),
                    new Vector3(gridRect.x + gridRect.width, yPos)
                );
            }

            Handles.EndGUI();
        }

        private void DrawPrerequisiteLines(Rect gridRect)
        {
            Handles.BeginGUI();
            Handles.color = prerequisiteLineColor;

            for (int i = 0; i < targetClass.classAbilities.Length; i++)
            {
                ClassAbility ability = targetClass.classAbilities[i];
                if (ability == null || !ability.HasPrerequisites())
                    continue;

                Vector2 fromPos = GetNodeScreenPosition(gridRect, ability.treePosition);

                foreach (int prereqIndex in ability.prerequisiteIndices)
                {
                    if (prereqIndex >= 0 && prereqIndex < targetClass.classAbilities.Length)
                    {
                        ClassAbility prereq = targetClass.classAbilities[prereqIndex];
                        if (prereq != null)
                        {
                            Vector2 toPos = GetNodeScreenPosition(gridRect, prereq.treePosition);
                            Handles.DrawLine(toPos, fromPos);
                        }
                    }
                }
            }

            Handles.EndGUI();
        }

        private void DrawSubSkillLines(Rect gridRect)
        {
            Handles.BeginGUI();
            Handles.color = subSkillLineColor;

            for (int i = 0; i < targetClass.classAbilities.Length; i++)
            {
                ClassAbility ability = targetClass.classAbilities[i];
                if (ability == null || !ability.HasSubSkills())
                    continue;

                Vector2 parentPos = GetNodeScreenPosition(gridRect, ability.treePosition);

                foreach (var subSkill in ability.subSkills)
                {
                    if (subSkill == null || !subSkill.IsValid())
                        continue;

                    Vector2 subPos = GetSubSkillScreenPosition(gridRect, ability.treePosition, subSkill.positionOffset);
                    Handles.DrawLine(parentPos, subPos);
                }
            }

            Handles.EndGUI();
        }

        private void DrawNodes(Rect gridRect)
        {
            for (int i = 0; i < targetClass.classAbilities.Length; i++)
            {
                ClassAbility ability = targetClass.classAbilities[i];
                if (ability == null)
                    continue;

                Vector2 screenPos = GetNodeScreenPosition(gridRect, ability.treePosition);
                Rect nodeRect = new Rect(
                    screenPos.x - NODE_SIZE / 2,
                    screenPos.y - NODE_SIZE / 2,
                    NODE_SIZE,
                    NODE_SIZE
                );

                // Draw node circle
                Color nodeDrawColor = (i == selectedAbilityIndex) ? selectedNodeColor : nodeColor;
                EditorGUI.DrawRect(nodeRect, nodeDrawColor);

                // Draw ability icon or index
                GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel);
                labelStyle.alignment = TextAnchor.MiddleCenter;
                labelStyle.normal.textColor = Color.white;
                labelStyle.fontStyle = FontStyle.Bold;

                GUI.Label(nodeRect, i.ToString(), labelStyle);

                // Draw ability name below node
                if (ability.ability != null)
                {
                    Rect nameRect = new Rect(screenPos.x - 50, screenPos.y + NODE_SIZE / 2 + 2, 100, 15);
                    labelStyle.fontSize = 9;
                    labelStyle.fontStyle = FontStyle.Normal;
                    GUI.Label(nameRect, ability.ability.abilityName, labelStyle);
                }
            }
        }

        private void DrawSubSkills(Rect gridRect)
        {
            for (int i = 0; i < targetClass.classAbilities.Length; i++)
            {
                ClassAbility ability = targetClass.classAbilities[i];
                if (ability == null || !ability.HasSubSkills())
                    continue;

                for (int j = 0; j < ability.subSkills.Length; j++)
                {
                    SubSkillNodeData subSkill = ability.subSkills[j];
                    if (subSkill == null || !subSkill.IsValid())
                        continue;

                    Vector2 screenPos = GetSubSkillScreenPosition(gridRect, ability.treePosition, subSkill.positionOffset);
                    Rect subNodeRect = new Rect(
                        screenPos.x - SUB_NODE_SIZE / 2,
                        screenPos.y - SUB_NODE_SIZE / 2,
                        SUB_NODE_SIZE,
                        SUB_NODE_SIZE
                    );

                    // Draw sub-skill node
                    bool isSelected = (i == selectedAbilityIndex && j == selectedSubSkillIndex);
                    Color subDrawColor = isSelected ? selectedSubNodeColor : subNodeColor;
                    EditorGUI.DrawRect(subNodeRect, subDrawColor);

                    // Draw sub-skill label
                    GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel);
                    labelStyle.alignment = TextAnchor.MiddleCenter;
                    labelStyle.normal.textColor = Color.white;
                    labelStyle.fontStyle = FontStyle.Bold;
                    labelStyle.fontSize = 8;

                    string label = subSkill.GetName();
                    if (label.Length > 3)
                        label = label.Substring(0, 3);
                    
                    GUI.Label(subNodeRect, label, labelStyle);
                }
            }
        }

        private void HandleNodeInteraction(Rect gridRect)
        {
            Event e = Event.current;
            Vector2 mousePos = e.mousePosition;

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                // First check sub-skills (they're smaller and should have priority)
                for (int i = 0; i < targetClass.classAbilities.Length; i++)
                {
                    ClassAbility ability = targetClass.classAbilities[i];
                    if (ability == null || !ability.HasSubSkills())
                        continue;

                    for (int j = 0; j < ability.subSkills.Length; j++)
                    {
                        SubSkillNodeData subSkill = ability.subSkills[j];
                        if (subSkill == null || !subSkill.IsValid())
                            continue;

                        Vector2 screenPos = GetSubSkillScreenPosition(gridRect, ability.treePosition, subSkill.positionOffset);
                        float distance = Vector2.Distance(mousePos, screenPos);

                        if (distance < SUB_NODE_SIZE / 2)
                        {
                            selectedAbilityIndex = i;
                            selectedSubSkillIndex = j;
                            isDraggingSubSkill = true;
                            dragOffset = subSkill.positionOffset - GetGridOffset(gridRect, mousePos, ability.treePosition);
                            e.Use();
                            Repaint();
                            return;
                        }
                    }
                }

                // Then check main nodes
                for (int i = 0; i < targetClass.classAbilities.Length; i++)
                {
                    ClassAbility ability = targetClass.classAbilities[i];
                    if (ability == null)
                        continue;

                    Vector2 screenPos = GetNodeScreenPosition(gridRect, ability.treePosition);
                    float distance = Vector2.Distance(mousePos, screenPos);

                    if (distance < NODE_SIZE / 2)
                    {
                        selectedAbilityIndex = i;
                        selectedSubSkillIndex = -1;
                        isDragging = true;
                        dragOffset = ability.treePosition - GetGridPosition(gridRect, mousePos);
                        e.Use();
                        Repaint();
                        break;
                    }
                }
            }
            else if (e.type == EventType.MouseDrag && selectedAbilityIndex >= 0)
            {
                if (isDraggingSubSkill && selectedSubSkillIndex >= 0)
                {
                    // Drag sub-skill
                    ClassAbility ability = targetClass.classAbilities[selectedAbilityIndex];
                    Vector2 newOffset = GetGridOffset(gridRect, mousePos, ability.treePosition) + dragOffset;
                    
                    // Round to nearest 0.5 for finer control
                    newOffset.x = Mathf.Round(newOffset.x * 2f) / 2f;
                    newOffset.y = Mathf.Round(newOffset.y * 2f) / 2f;

                    ability.subSkills[selectedSubSkillIndex].positionOffset = newOffset;

                    EditorUtility.SetDirty(targetClass);
                    e.Use();
                    Repaint();
                }
                else if (isDragging)
                {
                    // Drag main node
                    Vector2 newGridPos = GetGridPosition(gridRect, mousePos) + dragOffset;

                    // Snap to grid
                    newGridPos.x = Mathf.Round(newGridPos.x);
                    newGridPos.y = Mathf.Round(newGridPos.y);

                    // Clamp to grid bounds
                    newGridPos.x = Mathf.Clamp(newGridPos.x, 0, GRID_SIZE - 1);
                    newGridPos.y = Mathf.Clamp(newGridPos.y, 0, GRID_SIZE - 1);

                    targetClass.classAbilities[selectedAbilityIndex].treePosition = newGridPos;

                    EditorUtility.SetDirty(targetClass);
                    e.Use();
                    Repaint();
                }
            }
            else if (e.type == EventType.MouseUp && e.button == 0)
            {
                isDragging = false;
                isDraggingSubSkill = false;
            }
        }

        private Vector2 GetNodeScreenPosition(Rect gridRect, Vector2 gridPosition)
        {
            return new Vector2(
                gridRect.x + gridPosition.x * CELL_SIZE + CELL_SIZE / 2,
                gridRect.y + gridPosition.y * CELL_SIZE + CELL_SIZE / 2
            );
        }

        private Vector2 GetSubSkillScreenPosition(Rect gridRect, Vector2 parentGridPosition, Vector2 offset)
        {
            Vector2 parentScreenPos = GetNodeScreenPosition(gridRect, parentGridPosition);
            return new Vector2(
                parentScreenPos.x + offset.x * CELL_SIZE,
                parentScreenPos.y + offset.y * CELL_SIZE
            );
        }

        private Vector2 GetGridPosition(Rect gridRect, Vector2 screenPosition)
        {
            return new Vector2(
                (screenPosition.x - gridRect.x) / CELL_SIZE,
                (screenPosition.y - gridRect.y) / CELL_SIZE
            );
        }

        private Vector2 GetGridOffset(Rect gridRect, Vector2 screenPosition, Vector2 parentGridPosition)
        {
            Vector2 parentScreenPos = GetNodeScreenPosition(gridRect, parentGridPosition);
            return new Vector2(
                (screenPosition.x - parentScreenPos.x) / CELL_SIZE,
                (screenPosition.y - parentScreenPos.y) / CELL_SIZE
            );
        }

        private void DrawDetailsPanel()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(position.width * 0.33f));
            {
                detailsScrollPosition = EditorGUILayout.BeginScrollView(detailsScrollPosition);
                {
                    EditorGUILayout.LabelField("Node Details", EditorStyles.boldLabel);

                    if (selectedAbilityIndex < 0 || selectedAbilityIndex >= targetClass.classAbilities.Length)
                    {
                        EditorGUILayout.HelpBox("Click a node to edit its details\nClick a sub-skill to edit its offset", MessageType.Info);
                    }
                    else
                    {
                        ClassAbility selected = targetClass.classAbilities[selectedAbilityIndex];

                        EditorGUILayout.LabelField($"Main Ability Index: {selectedAbilityIndex}", EditorStyles.miniLabel);
                        EditorGUILayout.Space();

                        // Ability reference
                        selected.ability = (AbilityBase)EditorGUILayout.ObjectField(
                            "Ability", selected.ability, typeof(AbilityBase), false);

                        // Grid position (read-only display)
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.Vector2Field("Grid Position", selected.treePosition);
                        EditorGUI.EndDisabledGroup();

                        EditorGUILayout.Space();

                        // Requirements
                        selected.requiredLevel = EditorGUILayout.IntField("Required Level", selected.requiredLevel);
                        selected.skillPointCost = EditorGUILayout.IntField("Skill Point Cost", selected.skillPointCost);

                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("Prerequisites", EditorStyles.boldLabel);

                        // Show prerequisite list
                        if (selected.prerequisiteIndices != null && selected.prerequisiteIndices.Length > 0)
                        {
                            for (int i = 0; i < selected.prerequisiteIndices.Length; i++)
                            {
                                EditorGUILayout.BeginHorizontal();
                                {
                                    selected.prerequisiteIndices[i] = EditorGUILayout.IntField(
                                        $"Prereq {i}", selected.prerequisiteIndices[i]);

                                    if (GUILayout.Button("X", GUILayout.Width(25)))
                                    {
                                        RemovePrerequisite(selected, i);
                                        EditorUtility.SetDirty(targetClass);
                                    }
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                        }
                        else
                        {
                            EditorGUILayout.LabelField("None", EditorStyles.miniLabel);
                        }

                        if (GUILayout.Button("Add Prerequisite"))
                        {
                            AddPrerequisite(selected);
                            EditorUtility.SetDirty(targetClass);
                        }

                        // ✨ SUB-SKILLS SECTION
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("Sub-Skills", EditorStyles.boldLabel);

                        if (selected.subSkills != null && selected.subSkills.Length > 0)
                        {
                            for (int i = 0; i < selected.subSkills.Length; i++)
                            {
                                SubSkillNodeData subSkill = selected.subSkills[i];
                                
                                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                                {
                                    EditorGUILayout.BeginHorizontal();
                                    {
                                        bool isSelected = (i == selectedSubSkillIndex);
                                        string label = isSelected ? $"► Sub-Skill {i}" : $"Sub-Skill {i}";
                                        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

                                        if (GUILayout.Button("X", GUILayout.Width(25)))
                                        {
                                            RemoveSubSkill(selected, i);
                                            if (selectedSubSkillIndex >= selected.subSkills.Length)
                                                selectedSubSkillIndex = -1;
                                            EditorUtility.SetDirty(targetClass);
                                            break;
                                        }
                                    }
                                    EditorGUILayout.EndHorizontal();

                                    if (subSkill != null)
                                    {
                                        subSkill.subSkillModifier = (AbilitySubSkill)EditorGUILayout.ObjectField(
                                            "Modifier", subSkill.subSkillModifier, typeof(AbilitySubSkill), false);

                                        subSkill.positionOffset = EditorGUILayout.Vector2Field("Position Offset", subSkill.positionOffset);
                                        subSkill.requiredLevel = EditorGUILayout.IntField("Required Level", subSkill.requiredLevel);
                                        subSkill.skillPointCost = EditorGUILayout.IntField("Skill Point Cost", subSkill.skillPointCost);

                                        subSkill.customIcon = (Sprite)EditorGUILayout.ObjectField(
                                            "Custom Icon", subSkill.customIcon, typeof(Sprite), false);

                                        EditorGUILayout.LabelField("Custom Description:");
                                        subSkill.customDescription = EditorGUILayout.TextArea(
                                            subSkill.customDescription, GUILayout.Height(40));
                                    }
                                }
                                EditorGUILayout.EndVertical();
                                EditorGUILayout.Space();
                            }
                        }
                        else
                        {
                            EditorGUILayout.LabelField("None", EditorStyles.miniLabel);
                        }

                        if (GUILayout.Button("Add Sub-Skill"))
                        {
                            AddSubSkill(selected);
                            EditorUtility.SetDirty(targetClass);
                        }
                    }
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }

        private void AddPrerequisite(ClassAbility ability)
        {
            int[] newArray = new int[(ability.prerequisiteIndices?.Length ?? 0) + 1];
            if (ability.prerequisiteIndices != null)
            {
                System.Array.Copy(ability.prerequisiteIndices, newArray, ability.prerequisiteIndices.Length);
            }
            ability.prerequisiteIndices = newArray;
        }

        private void RemovePrerequisite(ClassAbility ability, int index)
        {
            if (ability.prerequisiteIndices == null || index < 0 || index >= ability.prerequisiteIndices.Length)
                return;

            int[] newArray = new int[ability.prerequisiteIndices.Length - 1];
            int newIndex = 0;

            for (int i = 0; i < ability.prerequisiteIndices.Length; i++)
            {
                if (i != index)
                {
                    newArray[newIndex++] = ability.prerequisiteIndices[i];
                }
            }

            ability.prerequisiteIndices = newArray;
        }

        private void AddSubSkill(ClassAbility ability)
        {
            SubSkillNodeData[] newArray = new SubSkillNodeData[(ability.subSkills?.Length ?? 0) + 1];
            if (ability.subSkills != null)
            {
                System.Array.Copy(ability.subSkills, newArray, ability.subSkills.Length);
            }
            
            // Create new sub-skill with default offset (to the right)
            newArray[newArray.Length - 1] = new SubSkillNodeData
            {
                positionOffset = new Vector2(2, 0),
                requiredLevel = ability.requiredLevel,
                skillPointCost = 1
            };
            
            ability.subSkills = newArray;
        }

        private void RemoveSubSkill(ClassAbility ability, int index)
        {
            if (ability.subSkills == null || index < 0 || index >= ability.subSkills.Length)
                return;

            SubSkillNodeData[] newArray = new SubSkillNodeData[ability.subSkills.Length - 1];
            int newIndex = 0;

            for (int i = 0; i < ability.subSkills.Length; i++)
            {
                if (i != index)
                {
                    newArray[newIndex++] = ability.subSkills[i];
                }
            }

            ability.subSkills = newArray;
        }
    }
}
#endif