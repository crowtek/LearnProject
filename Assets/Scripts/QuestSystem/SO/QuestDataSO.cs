using UnityEngine;

[CreateAssetMenu(fileName = "QuestDataSO", menuName = "Scriptable Objects/Quest/QuestDataSO")]
public class QuestDataSO : ScriptableObject
{
    public string questName;
    [TextArea] public string description;

    // What story flag is needed to start the Quest
    [StoryFlag] public string requiredStoryFlag;

    // What story flag is set after compleding the Quest
    [StoryFlag] public string completionFlag;
}
