using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct DialogueEntry
{
    public string key;
    [TextArea(3, 10)] public string text;
}


[CreateAssetMenu(fileName = "DialogueDatabaseSO", menuName = "Scriptable Objects/Dialogue/DialogueDatabaseSO")]
public class DialogueDatabaseSO : ScriptableObject
{
    public string languageName = "English";
    public List<DialogueEntry> dialogueEntries = new List<DialogueEntry>();

    public string GetText(string key)
    {
        var entry = dialogueEntries.Find(e => e.key == key);
        return entry.text ?? $"[Missing text for key: {key}]";
    }

}
