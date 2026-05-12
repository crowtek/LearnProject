using UnityEngine;

public struct DialogueData
{
    public string Text;
    public string SpeakerName;
    public Sprite SpeakerPortrait;
}

[CreateAssetMenu(fileName = "DialogueEventChannelSO", menuName = "Scriptable Objects/Dialogue/DialogueEventChannelSO")]
public class DialogueEventChannelSO : EventChannelSO<DialogueData> { }