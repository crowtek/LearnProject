using System;
using System.Collections.Generic;
using UnityEngine;

public enum NodeType { StoryFlag, Dialogue, Cutscene }

[Serializable]
public class StoryNodeData
{
    public string nodeName;
    public NodeType nodeType;

    // DialogueEntry.key, used by dialogue-choice nextDialogueKey links.
    public string dialogueKey;

    // String links used by the story assets.
    public string requiredFlag;
    public string resultFlag;

    // Richer runtime-style conditions that can be visualized/evaluated by the editor.
    public List<string> requiredFlags = new List<string>();
    public List<string> blockedFlags = new List<string>();

    // Index inside the source database list. Used to write struct/list entries back safely.
    public int sourceIndex = -1;

    public string descriptionText;

    // Branching options copied from DialogueChoice[].
    public List<DialogueOptionData> dialogueChoices;

    // Back-references to original source data.
    public DialogueEntry originalDialogue;
    public CutsceneDialogueEntry originalCutscene;

    // Parent ScriptableObject database assets used for saving/undo.
    public ScriptableObject originalDialogueAssetReference;
    public ScriptableObject originalCutsceneAssetReference;
    public ScriptableObject originalStoryFlagAssetReference;

    public string SourceAssetGuid;

    public IEnumerable<string> GetAllRequiredFlags()
    {
        if (!string.IsNullOrEmpty(requiredFlag))
        {
            yield return requiredFlag;
        }

        if (requiredFlags == null)
        {
            yield break;
        }

        foreach (string flag in requiredFlags)
        {
            if (!string.IsNullOrEmpty(flag))
            {
                yield return flag;
            }
        }
    }

    public IEnumerable<string> GetAllBlockedFlags()
    {
        if (blockedFlags == null)
        {
            yield break;
        }

        foreach (string flag in blockedFlags)
        {
            if (!string.IsNullOrEmpty(flag))
            {
                yield return flag;
            }
        }
    }
}

[Serializable]
public class DialogueOptionData
{
    public string optionText;
    public string resultFlag;
    public string nextDialogueKey;
}
