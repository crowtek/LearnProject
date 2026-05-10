using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    [SerializeField] private GlobalStoryStateSO storyState;
    [SerializeField] private StoryEventChannelSO storyEventChannel;

    [Header("Datenbank aller Quests")]
    [SerializeField] private List<QuestDataSO> allAvailableQuests;

    [Header("Aktueller Status (nur zur Info)")]
    [SerializeField] private List<QuestDataSO> activeQuests = new List<QuestDataSO>();
    [SerializeField] private List<QuestDataSO> completedQuests = new List<QuestDataSO>();

    private void OnEnable()
    {
        if (storyEventChannel != null)
            storyEventChannel.OnStoryFlagChange += EvaluateQuests; 
    }

    private void OnDisable()
    {
        if (storyEventChannel != null)
            storyEventChannel.OnStoryFlagChange -= EvaluateQuests;
    }

    private void Start()
    {
        EvaluateAllQuestsOnStart();
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
        Debug.Log($"Quest gestartet: {quest.questName}");
        // Hier Event für UI
    }

    private void CompleteQuest(QuestDataSO quest)
    {
        activeQuests.Remove(quest);
        completedQuests.Add(quest);
        Debug.Log($"Quest abgeschlossen: {quest.questName}");
    }

    private void EvaluateAllQuestsOnStart()
    {
        foreach (var quest in allAvailableQuests)
        {
            if (storyState.IsFlagCompleted(quest.completionFlag)) 
                completedQuests.Add(quest);
            else if (storyState.IsFlagCompleted(quest.startFlag))
                activeQuests.Add(quest);
        }
    }
}
