using UnityEngine;

public struct DialogueData
{
    public string[] Lines;
    public string SpeakerName;
    public Sprite SpeakerPortrait;
    public string ResultFlag;
}

[CreateAssetMenu(fileName = "DialogueEventChannelSO", menuName = "Scriptable Objects/Dialogue/DialogueEventChannelSO")]
public class DialogueEventChannelSO : EventChannelSO<DialogueData> { }