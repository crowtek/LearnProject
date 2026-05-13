using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(StoryFlagAttribute))]
public class StoryFlagDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        string[] guids = AssetDatabase.FindAssets("t:StoryFlagDatabaseSO"); // Suche nach der Datenbank

        if (guids.Length == 0)
        {
            EditorGUI.LabelField(position, label.text, "No StoryFlagDatabaseSO found!");
            return;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        var database = AssetDatabase.LoadAssetAtPath<StoryFlagDatabaseSO>(path);

        if (database == null || database.allFlags == null)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        List<string> flagsOptions = new List<string> { "No value" };
        flagsOptions.AddRange(database.allFlags);

        string[] flags = flagsOptions.ToArray();

        int currentIndex = 0;
        if (!string.IsNullOrEmpty(property.stringValue))
        {
            currentIndex = System.Array.IndexOf(flags, property.stringValue);
            if (currentIndex == -1) currentIndex = 0; 
        }

        currentIndex = EditorGUI.Popup(position, label.text, currentIndex, flags);
        property.stringValue = (currentIndex == 0) ? string.Empty : flags[currentIndex];
    }
}
