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

        if (database == null || database.allFlags == null || database.allFlags.Count == 0)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        string[] flags = database.allFlags.ToArray();

        int currentIndex = System.Array.IndexOf(flags, property.stringValue);
        if (currentIndex == -1) currentIndex = 0;

        currentIndex = EditorGUI.Popup(position, label.text, currentIndex, flags); // Dropdown zeichnen
        property.stringValue = flags[currentIndex];
    }
}
