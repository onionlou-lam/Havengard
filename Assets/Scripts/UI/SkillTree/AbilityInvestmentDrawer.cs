using UnityEngine;
using UnityEditor;
using Havengard.Abilities;
using Havengard.Core.Stats;

[CustomPropertyDrawer(typeof(AbilityInvestment))]
public class AbilityInvestmentDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var bonusesProperty = property.FindPropertyRelative("investmentBonuses");

        EditorGUILayout.LabelField("Ability Investment Configuration", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox($"Configure stat bonuses per investment level (max 20)", MessageType.Info);

        EditorGUILayout.Space(5);

        if (GUILayout.Button("+ Add Investment Bonus", GUILayout.Height(25)))
        {
            bonusesProperty.arraySize++;
        }

        for (int i = 0; i < bonusesProperty.arraySize; i++)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            var element = bonusesProperty.GetArrayElementAtIndex(i);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Bonus {i + 1}", EditorStyles.boldLabel, GUILayout.Width(60));

            if (GUILayout.Button("×", GUILayout.Width(25)))
            {
                bonusesProperty.DeleteArrayElementAtIndex(i);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                break;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(element.FindPropertyRelative("statType"));
            EditorGUILayout.PropertyField(element.FindPropertyRelative("modifierType"));
            EditorGUILayout.PropertyField(element.FindPropertyRelative("value"), new GUIContent("Base Value"));
            EditorGUILayout.PropertyField(element.FindPropertyRelative("valuePerLevel"), new GUIContent("Per Investment Level"));

            // Show preview
            var statType = (StatModifier.StatType)element.FindPropertyRelative("statType").enumValueIndex;
            var modType = (StatModifier.ModifierType)element.FindPropertyRelative("modifierType").enumValueIndex;
            float baseVal = element.FindPropertyRelative("value").floatValue;
            float perLevel = element.FindPropertyRelative("valuePerLevel").floatValue;

            string preview = $"Preview at level 20: {baseVal + (perLevel * 19):F2}";
            if (modType != StatModifier.ModifierType.Flat)
                preview += "%";

            EditorGUILayout.LabelField(preview, EditorStyles.miniLabel);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(3);
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 0; // We're using GUILayout
    }
}