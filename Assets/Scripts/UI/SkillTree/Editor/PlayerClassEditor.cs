using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Havengard.Core.Progression;

[CustomEditor(typeof(PlayerClass))]
public class PlayerClassEditor : Editor
{
    private PlayerClass playerClass;
    private SerializedProperty classAbilitiesProp;
    private Vector2 scrollPosition;
    private bool[] abilityFoldouts;
    
    private GUIStyle headerStyle;
    private GUIStyle boxStyle;
    private GUIStyle warningStyle;
    private bool stylesInitialized = false;

    private void OnEnable()
    {
        playerClass = (PlayerClass)target;
        classAbilitiesProp = serializedObject.FindProperty("classAbilities");
        
        if (playerClass.classAbilities != null)
        {
            abilityFoldouts = new bool[playerClass.classAbilities.Length];
        }
    }

    private void InitializeStyles()
    {
        if (stylesInitialized) return;

        headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.fontSize = 14;
        headerStyle.normal.textColor = new Color(0.3f, 0.7f, 1f);

        boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.padding = new RectOffset(10, 10, 10, 10);

        warningStyle = new GUIStyle(EditorStyles.helpBox);
        warningStyle.normal.textColor = Color.yellow;

        stylesInitialized = true;
    }

    public override void OnInspectorGUI()
    {
        InitializeStyles();
        serializedObject.Update();

        DrawClassInfo();
        EditorGUILayout.Space(10);
        DrawBaseStats();
        EditorGUILayout.Space(10);
        DrawProgression();
        EditorGUILayout.Space(10);
        DrawSpecializations();
        EditorGUILayout.Space(10);
        DrawAbilitiesSection();
        EditorGUILayout.Space(10);
        DrawUtilityButtons();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawClassInfo()
    {
        EditorGUILayout.LabelField("Class Information", headerStyle);
        EditorGUILayout.BeginVertical(boxStyle);
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("className"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("specializationName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("classIcon"));
        
        EditorGUILayout.EndVertical();
    }

    private void DrawBaseStats()
    {
        EditorGUILayout.LabelField("Base Stats", headerStyle);
        EditorGUILayout.BeginVertical(boxStyle);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("baseHP"), new GUIContent("HP"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("hpGrowth"), new GUIContent("Per Level"), GUILayout.Width(100));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("baseAttack"), new GUIContent("Attack"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("attackGrowth"), new GUIContent("Per Level"), GUILayout.Width(100));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("baseDefense"), new GUIContent("Defense"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("defenseGrowth"), new GUIContent("Per Level"), GUILayout.Width(100));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("baseResource"), new GUIContent("Resource"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("resourceGrowth"), new GUIContent("Per Level"), GUILayout.Width(100));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("baseAttackSpeed"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("baseMoveSpeed"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("baseCritChance"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("baseCritMultiplier"));
        
        EditorGUILayout.EndVertical();
    }

    private void DrawProgression()
    {
        EditorGUILayout.LabelField("Progression", headerStyle);
        EditorGUILayout.BeginVertical(boxStyle);
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("baseRollCooldown"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("expToLevel"), true);
        
        EditorGUILayout.EndVertical();
    }

    private void DrawSpecializations()
    {
        EditorGUILayout.LabelField("Specializations", headerStyle);
        EditorGUILayout.BeginVertical(boxStyle);
        
        var specializationsProp = serializedObject.FindProperty("specializations");
        
        if (specializationsProp.arraySize != 3)
        {
            EditorGUILayout.HelpBox("Must have exactly 3 specialization slots", MessageType.Info);
            if (GUILayout.Button("Set to 3 Slots"))
            {
                specializationsProp.arraySize = 3;
            }
        }

        for (int i = 0; i < Mathf.Min(3, specializationsProp.arraySize); i++)
        {
            EditorGUILayout.PropertyField(specializationsProp.GetArrayElementAtIndex(i), 
                new GUIContent($"Spec {i + 1}: Tab {i + 1}"));
        }
        
        EditorGUILayout.EndVertical();
    }

    private void DrawAbilitiesSection()
    {
        EditorGUILayout.LabelField($"Class Abilities ({classAbilitiesProp.arraySize})", headerStyle);
        
        EditorGUILayout.BeginVertical(boxStyle);
        
        // Add/Remove buttons
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("+ Add Ability", GUILayout.Height(25)))
        {
            classAbilitiesProp.arraySize++;
            System.Array.Resize(ref abilityFoldouts, classAbilitiesProp.arraySize);
            serializedObject.ApplyModifiedProperties();
        }
        
        GUI.enabled = classAbilitiesProp.arraySize > 0;
        if (GUILayout.Button("- Remove Last", GUILayout.Height(25)))
        {
            classAbilitiesProp.arraySize--;
            System.Array.Resize(ref abilityFoldouts, classAbilitiesProp.arraySize);
            serializedObject.ApplyModifiedProperties();
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        // Scroll view for abilities
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(400));
        
        for (int i = 0; i < classAbilitiesProp.arraySize; i++)
        {
            DrawAbilityElement(i);
        }
        
        EditorGUILayout.EndScrollView();
        
        EditorGUILayout.EndVertical();
    }

    private void DrawAbilityElement(int index)
    {
        var abilityProp = classAbilitiesProp.GetArrayElementAtIndex(index);
        var abilityRef = abilityProp.FindPropertyRelative("ability");
        var ability = abilityRef.objectReferenceValue as AbilityBase;
        
        // Color code based on ability state
        Color bgColor = GUI.backgroundColor;
        if (ability == null)
            GUI.backgroundColor = new Color(1f, 0.5f, 0.5f, 0.3f); // Red tint for missing
        else if (abilityProp.FindPropertyRelative("prerequisiteIndices").arraySize > 0)
            GUI.backgroundColor = new Color(0.5f, 1f, 0.5f, 0.3f); // Green tint for has prereqs
        else
            GUI.backgroundColor = new Color(0.5f, 0.5f, 1f, 0.3f); // Blue tint for starting ability

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = bgColor;

        // Header with foldout
        EditorGUILayout.BeginHorizontal();
        
        abilityFoldouts[index] = EditorGUILayout.Foldout(abilityFoldouts[index], 
            $"[{index}] {(ability != null ? ability.abilityName : "Empty Slot")}", true);
        
        // Quick info
        if (ability != null)
        {
            GUILayout.Label($"Lvl {abilityProp.FindPropertyRelative("requiredLevel").intValue}", 
                GUILayout.Width(50));
            
            if (ability.icon != null)
            {
                Texture2D icon = AssetPreview.GetAssetPreview(ability.icon.texture);
                if (icon != null)
                    GUILayout.Label(icon, GUILayout.Width(20), GUILayout.Height(20));
            }
        }

        // Move up/down buttons
        GUI.enabled = index > 0;
        if (GUILayout.Button("?", GUILayout.Width(25)))
        {
            classAbilitiesProp.MoveArrayElement(index, index - 1);
            serializedObject.ApplyModifiedProperties();
        }
        GUI.enabled = index < classAbilitiesProp.arraySize - 1;
        if (GUILayout.Button("?", GUILayout.Width(25)))
        {
            classAbilitiesProp.MoveArrayElement(index, index + 1);
            serializedObject.ApplyModifiedProperties();
        }
        GUI.enabled = true;

        // Delete button
        GUI.backgroundColor = new Color(1f, 0.5f, 0.5f);
        if (GUILayout.Button("×", GUILayout.Width(25)))
        {
            classAbilitiesProp.DeleteArrayElementAtIndex(index);
            System.Array.Resize(ref abilityFoldouts, classAbilitiesProp.arraySize);
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            return;
        }
        GUI.backgroundColor = bgColor;

        EditorGUILayout.EndHorizontal();

        // Expanded content
        if (abilityFoldouts[index])
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(abilityRef, new GUIContent("Ability"));
            EditorGUILayout.PropertyField(abilityProp.FindPropertyRelative("requiredLevel"));
            EditorGUILayout.PropertyField(abilityProp.FindPropertyRelative("skillPointCost"));
            
            // Tree position
            EditorGUILayout.PropertyField(abilityProp.FindPropertyRelative("treePosition"), 
                new GUIContent("Grid Position (X=Column, Y=Row)"));
            
            // Prerequisites with dropdown
            DrawPrerequisites(index, abilityProp);
            
            // Sub-skills
            EditorGUILayout.PropertyField(abilityProp.FindPropertyRelative("subSkills"), 
                new GUIContent("Sub-Skills"), true);
            
            EditorGUILayout.PropertyField(abilityProp.FindPropertyRelative("customDescription"), 
                new GUIContent("Custom Description"), true);
            
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(2);
    }

    private void DrawPrerequisites(int currentIndex, SerializedProperty abilityProp)
    {
        var prereqProp = abilityProp.FindPropertyRelative("prerequisiteIndices");
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Prerequisites");
        
        if (GUILayout.Button("+", GUILayout.Width(25)))
        {
            prereqProp.arraySize++;
        }
        
        if (prereqProp.arraySize > 0 && GUILayout.Button("-", GUILayout.Width(25)))
        {
            prereqProp.arraySize--;
        }
        
        EditorGUILayout.EndHorizontal();

        EditorGUI.indentLevel++;
        
        for (int i = 0; i < prereqProp.arraySize; i++)
        {
            EditorGUILayout.BeginHorizontal();
            
            var prereqIndexProp = prereqProp.GetArrayElementAtIndex(i);
            int currentPrereqIndex = prereqIndexProp.intValue;
            
            // Build dropdown options
            List<string> options = new List<string> { "None" };
            List<int> values = new List<int> { -1 };
            
            for (int j = 0; j < classAbilitiesProp.arraySize; j++)
            {
                if (j == currentIndex) continue; // Can't prereq self
                
                var otherAbility = classAbilitiesProp.GetArrayElementAtIndex(j)
                    .FindPropertyRelative("ability").objectReferenceValue as AbilityBase;
                
                string label = $"[{j}] {(otherAbility != null ? otherAbility.abilityName : "Empty")}";
                options.Add(label);
                values.Add(j);
            }
            
            int selectedIndex = values.IndexOf(currentPrereqIndex);
            if (selectedIndex < 0) selectedIndex = 0;
            
            selectedIndex = EditorGUILayout.Popup($"Prereq {i}", selectedIndex, options.ToArray());
            prereqIndexProp.intValue = values[selectedIndex];
            
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUI.indentLevel--;
    }

    private void DrawUtilityButtons()
    {
        EditorGUILayout.LabelField("Utilities", headerStyle);
        EditorGUILayout.BeginVertical(boxStyle);
        
        if (GUILayout.Button("Open Skill Tree Visualizer", GUILayout.Height(30)))
        {
            SkillTreeVisualizerWindow.ShowWindow(playerClass);
        }
        
        if (GUILayout.Button("Validate Skill Tree", GUILayout.Height(25)))
        {
            ValidateSkillTree();
        }
        
        if (GUILayout.Button("Auto-Assign Grid Positions", GUILayout.Height(25)))
        {
            AutoAssignGridPositions();
        }
        
        EditorGUILayout.EndVertical();
    }

    private void ValidateSkillTree()
    {
        if (playerClass.classAbilities == null || playerClass.classAbilities.Length == 0)
        {
            EditorUtility.DisplayDialog("Validation", "No abilities to validate.", "OK");
            return;
        }

        List<string> errors = new List<string>();
        List<string> warnings = new List<string>();

        for (int i = 0; i < playerClass.classAbilities.Length; i++)
        {
            var classAbility = playerClass.classAbilities[i];
            
            if (classAbility.ability == null)
            {
                errors.Add($"[{i}] Missing ability reference");
            }

            foreach (int prereq in classAbility.prerequisiteIndices)
            {
                if (prereq < 0 || prereq >= playerClass.classAbilities.Length)
                {
                    errors.Add($"[{i}] Invalid prerequisite index: {prereq}");
                }
                else if (prereq == i)
                {
                    errors.Add($"[{i}] Cannot have self as prerequisite");
                }
            }

            // Check for circular dependencies
            if (HasCircularDependency(i, new HashSet<int>()))
            {
                errors.Add($"[{i}] Circular dependency detected");
            }
        }

        string message = "";
        if (errors.Count > 0)
        {
            message += "ERRORS:\n" + string.Join("\n", errors) + "\n\n";
        }
        if (warnings.Count > 0)
        {
            message += "WARNINGS:\n" + string.Join("\n", warnings);
        }
        if (errors.Count == 0 && warnings.Count == 0)
        {
            message = "? Skill tree is valid!";
        }

        EditorUtility.DisplayDialog("Skill Tree Validation", message, "OK");
    }

    private bool HasCircularDependency(int index, HashSet<int> visited)
    {
        if (visited.Contains(index)) return true;
        visited.Add(index);

        var classAbility = playerClass.classAbilities[index];
        foreach (int prereq in classAbility.prerequisiteIndices)
        {
            if (prereq >= 0 && prereq < playerClass.classAbilities.Length)
            {
                if (HasCircularDependency(prereq, new HashSet<int>(visited)))
                    return true;
            }
        }

        return false;
    }

    private void AutoAssignGridPositions()
    {
        if (EditorUtility.DisplayDialog("Auto-Assign Grid Positions",
            "This will automatically arrange abilities in a grid based on their order and prerequisites. Continue?",
            "Yes", "Cancel"))
        {
            Undo.RecordObject(playerClass, "Auto-Assign Grid Positions");
            
            // Simple grid layout: 3 columns, arranged by order
            for (int i = 0; i < playerClass.classAbilities.Length; i++)
            {
                int column = i % 3;
                int row = i / 3;
                playerClass.classAbilities[i].treePosition = new Vector2(column, row);
            }
            
            EditorUtility.SetDirty(playerClass);
            serializedObject.Update();
        }
    }
}