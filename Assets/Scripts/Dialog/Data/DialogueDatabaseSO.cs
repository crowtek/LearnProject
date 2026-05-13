using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct DialogueEntry
{
    public string key;
    [TextArea(3, 10)] public string[] conversationLines;
    [StoryFlag] public string resultFlag;
}

[CreateAssetMenu(fileName = "DialogueDatabaseSO", menuName = "Scriptable Objects/Dialogue/DialogueDatabaseSO")]
public class DialogueDatabaseSO : ScriptableObject
{
    public string languageName = "English";
    public List<DialogueEntry> dialogueEntries = new List<DialogueEntry>();

    private Dictionary<string, DialogueEntry> _cache;

    private void OnEnable()
    {
        _cache = dialogueEntries.ToDictionary(e => e.key, e => e);
    }

    public DialogueEntry GetEntry(string key)
    {
        if (_cache != null && _cache.TryGetValue(key, out DialogueEntry entry))
            return entry;

        Debug.LogWarning($"[DialogueDB] Missing key: {key}");
        return default;
    }
}