using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Works with the StoryFlagDatabaseSO
[CustomPropertyDrawer(typeof(StoryFlagAttribute))]
public class StoryFlagDrawer : PropertyDrawer
{
    private StoryFlagDatabaseSO chachedDatabase;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.String)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        return base.GetPropertyHeight(property, label);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.PropertyField(position, property, label, true);
            return;
        }

        if (chachedDatabase == null)
        {
            // Search for Story Flag Database asset
            string[] guids = AssetDatabase.FindAssets("t:StoryFlagDatabaseSO");

            // Check if DB is not empty
            if (guids.Length == 0)
            {
                EditorGUI.LabelField(position, label.text, "No StoryFlagDatabaseSO found!");
                return;
            }

            // Get Data path and loads DB from it
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            chachedDatabase = AssetDatabase.LoadAssetAtPath<StoryFlagDatabaseSO>(path);
        }
       

        if (chachedDatabase == null || chachedDatabase.allFlags == null)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        // Build a list for the Dropdown
        List<string> flagsOptions = new List<string> { "No Story Flag" }; // add a empty first value
        flagsOptions.AddRange(chachedDatabase.allFlags);
        string[] flags = flagsOptions.ToArray();

        int currentIndex = 0;
        if (!string.IsNullOrEmpty(property.stringValue))
        {
            currentIndex = System.Array.IndexOf(flags, property.stringValue);
            if (currentIndex == -1) currentIndex = 0; 
        }

        EditorGUI.BeginChangeCheck();

        // Draw the Dropdown with empty first value
        currentIndex = EditorGUI.Popup(position, label.text, currentIndex, flags);

        if (EditorGUI.EndChangeCheck())
        {
            if (currentIndex == 0)
            {
                property.stringValue = string.Empty;
            }
            else
            {
                property.stringValue = flags[currentIndex];
            }
        }
    }
}
