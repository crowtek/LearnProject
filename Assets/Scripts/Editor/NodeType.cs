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

    // Deine echten String-Verknüpfungen aus den SOs
    public string requiredFlag;
    public string resultFlag;

    // Index inside the source database list. Used to write struct/list entries back safely.
    public int sourceIndex = -1;

    public string descriptionText;

    // Optionale Listen für Verzweigungen (Aus DialogueChoice[])
    public List<DialogueOptionData> dialogueChoices;

    // Back-References, falls du das Original-Asset im Editor anklicken willst
    public DialogueEntry originalDialogue;
    public CutsceneDialogueEntry originalCutscene;

    // Das übergeordnete ScriptableObject-Datenbank-Asset, um SetDirty aufzurufen
    public ScriptableObject originalDialogueAssetReference;
    public ScriptableObject originalCutsceneAssetReference;
    public ScriptableObject originalStoryFlagAssetReference;
}

[Serializable]
public class DialogueOptionData
{
    public string optionText;
    public string resultFlag;
    public string nextDialogueKey;
}
