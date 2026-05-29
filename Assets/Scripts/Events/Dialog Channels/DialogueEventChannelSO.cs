using UnityEngine;
using System;

public struct DialogueData
{
    public string[] Lines;
    public string SpeakerName;
    public Sprite SpeakerPortrait;
    public string ResultFlag;
    public DialogueChoice[] Choices;
    public Func<string, DialogueData> DialogueResolver;
}


[Serializable]
public struct DialogueChoice
{
    public string displayText;
    [StoryFlag] public string resultFlag;
    [DialogueKey] public string nextDialogueKey;
}

[CreateAssetMenu(fileName = "DialogueEventChannelSO", menuName = "Scriptable Objects/Dialogue/DialogueEventChannelSO")]
public class DialogueEventChannelSO : EventChannelSO<DialogueData> { }