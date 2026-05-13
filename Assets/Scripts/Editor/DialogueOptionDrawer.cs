using System.Collections.Generic;
using System.Linq; // Required for .Select()
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(DialogueKeyAttribute))]
public class DialogueOptionDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        string[] guids = AssetDatabase.FindAssets("t:DialogueDatabaseSO");

        if (guids.Length == 0)
        {
            EditorGUI.LabelField(position, label.text, "No DialogueDatabaseSO found!");
            return;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        var database = AssetDatabase.LoadAssetAtPath<DialogueDatabaseSO>(path);

        if (database == null || database.dialogueEntries == null)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        List<string> options = new List<string> { "No Value" };
        options.AddRange(database.dialogueEntries.Select(e => e.key));

        string[] optionsArray = options.ToArray();

        int currentIndex = 0;
        if (!string.IsNullOrEmpty(property.stringValue))
        {
            currentIndex = System.Array.IndexOf(optionsArray, property.stringValue);
            if (currentIndex == -1) currentIndex = 0;
        }

        // Draw the Popup
        currentIndex = EditorGUI.Popup(position, label.text, currentIndex, optionsArray);
        property.stringValue = (currentIndex == 0) ? string.Empty : optionsArray[currentIndex];
    }
}