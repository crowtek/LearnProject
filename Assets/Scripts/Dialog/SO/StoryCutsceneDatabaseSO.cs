using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct CutsceneDialogueEntry
{
    [Tooltip("Story flag needed for this dialog")]
    [StoryFlag] public string triggerStoryFlag;

    public string speakerName;
    public Sprite speakerPortrait;

    [Tooltip("Wich Dialog should be shown")]
    [DialogueKey] public string dialogueKey;
}

[CreateAssetMenu(fileName = "StoryCutsceneDatabaseSO", menuName = "Scriptable Objects/Dialogue/StoryCutsceneDatabaseSO")]
public class StoryCutsceneDatabaseSO : ScriptableObject
{
    [SerializeField] private List<CutsceneDialogueEntry> cutsceneDialogues = new List<CutsceneDialogueEntry>();

    private Dictionary<string, CutsceneDialogueEntry> _cache;

    public void InitializeCache()
    {
        _cache = cutsceneDialogues.ToDictionary(e => e.triggerStoryFlag, e => e);
    }

    public bool TryGetCutscene(string storyFlag, out CutsceneDialogueEntry entry)
    {
        if (_cache == null)
        {
            InitializeCache();
        }

        return _cache.TryGetValue(storyFlag, out entry);
    }
}