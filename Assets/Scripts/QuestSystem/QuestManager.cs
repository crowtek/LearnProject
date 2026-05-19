using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    [Header("DB for Quests")]
    [SerializeField] private List<QuestDataSO> allAvailableQuests;

    [Header("Current Status ")]
    [SerializeField] private List<QuestDataSO> activeQuests = new List<QuestDataSO>();
    [SerializeField] private List<QuestDataSO> completedQuests = new List<QuestDataSO>();

    [Header("Story Broadcast Channel")]
    [SerializeField] private StringEventChannelSO storyFlagChangedBroadcastChannel;

    public System.Action OnQuestListChanged;
    public List<QuestDataSO> GetActiveQuest => activeQuests;
    public QuestDataSO CurrentStoryFlag;

    private void OnEnable()
    {
        if (storyFlagChangedBroadcastChannel != null)
            storyFlagChangedBroadcastChannel.OnEventRaised += EvaluateQuests; 
    }

    private void OnDisable()
    {
        if (storyFlagChangedBroadcastChannel != null)
            storyFlagChangedBroadcastChannel.OnEventRaised -= EvaluateQuests;
    }


    private void EvaluateQuests(string changedFlag)
    {
        foreach(var quest in allAvailableQuests)
        {
            if(changedFlag == quest.startFlag && !activeQuests.Contains(quest) && !completedQuests.Contains(quest))
            {
                ActivateQuest(quest);
            }

            if (changedFlag == quest.completionFlag && activeQuests.Contains(quest))
            {
                CompleteQuest(quest);
            }
        }
    }

    private void ActivateQuest(QuestDataSO quest)
    {
        activeQuests.Add(quest);
        CurrentStoryFlag = quest;
        Debug.Log($"Quest gestartet: {quest.questName}");
        OnQuestListChanged?.Invoke();
    }

    private void CompleteQuest(QuestDataSO quest)
    {
        activeQuests.Remove(quest);
        completedQuests.Add(quest);
        Debug.Log($"Quest abgeschlossen: {quest.questName}");
        OnQuestListChanged?.Invoke();
    }
}
