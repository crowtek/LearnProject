using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    [SerializeField] private GlobalStoryStateSO storyState;
    [SerializeField] private StoryEventChannelSO storyEventChannel;

    [Header("DB for Quests")]
    [SerializeField] private List<QuestDataSO> allAvailableQuests;

    [Header("Current Status ")]
    [SerializeField] private List<QuestDataSO> activeQuests = new List<QuestDataSO>();
    [SerializeField] private List<QuestDataSO> completedQuests = new List<QuestDataSO>();

    public System.Action OnQuestListChanged;
    public List<QuestDataSO> GetActiveQuest => activeQuests;

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
        OnQuestListChanged?.Invoke();
    }

    private void CompleteQuest(QuestDataSO quest)
    {
        activeQuests.Remove(quest);
        completedQuests.Add(quest);
        Debug.Log($"Quest abgeschlossen: {quest.questName}");
        OnQuestListChanged?.Invoke();
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

        OnQuestListChanged?.Invoke();
    }
}
