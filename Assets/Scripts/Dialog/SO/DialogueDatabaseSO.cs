using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public struct DialogueEntry
{
    public string key;
    [TextArea(3, 10)] public string[] conversationLines;
    [StoryFlag] public string resultFlag;
    public DialogueChoice[] choices;
}

[System.Serializable]
public struct DialogueCondition
{
    public FlagCondition flagCondition;

    [FormerlySerializedAs("requiredFlag")]
    [StoryFlag] public string legacyRequiredFlag;
    [DialogueKey] public string dialogueKey;

    public bool IsMet(ICollection<string> completedFlags)
    {
        if (flagCondition.HasAnyFlag())
        {
            return flagCondition.IsMet(completedFlags);
        }

        return !string.IsNullOrEmpty(legacyRequiredFlag) &&
               completedFlags != null &&
               completedFlags.Contains(legacyRequiredFlag);
    }
}


[CreateAssetMenu(fileName = "DialogueDatabaseSO", menuName = "Scriptable Objects/Dialogue/DialogueDatabaseSO")]
public class DialogueDatabaseSO : ScriptableObject
{
    public string languageName = "English";
    [SerializeField] private string interactPromptFormat = "Talk to {0}";
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

    public string GetInteractPrompt(string npcName)
    {
        if (string.IsNullOrWhiteSpace(interactPromptFormat))
            return npcName;

        try
        {
            return string.Format(interactPromptFormat, npcName);
        }
        catch (FormatException exception)
        {
            Debug.LogWarning($"[DialogueDB] Invalid interact prompt format in {name}: {exception.Message}");
            return npcName;
        }
    }
}