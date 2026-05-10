using UnityEngine;

[CreateAssetMenu(fileName = "QuestDataSO", menuName = "Scriptable Objects/Story/QuestDataSO")]
public class QuestDataSO : ScriptableObject
{
    public string questName;
    [TextArea] public string description;

    public string startFlag;      
    public string completionFlag;
}
