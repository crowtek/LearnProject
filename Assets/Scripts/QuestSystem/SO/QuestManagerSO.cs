using System.Collections.Generic;
using UnityEngine;

// Needs to get initialisieren over GameInitializer
[CreateAssetMenu(fileName = "QuestManagerSO", menuName = "Scriptable Objects/Quest/QuestManagerSO")]
public class QuestManagerSO : ScriptableObject
{
    [Header("Quest Library")]
    [SerializeField] private QuestDatabaseSO allAvailableQuests;

    [Header("Channels")]
    [SerializeField] private StringEventChannelSO storyFlagChangedBroadcastChannel;
    [SerializeField] private BoolEventChannelSO questChangedChannel;

    [Header("Runtime Status (Read-Only)")]
    [SerializeField] private QuestDataSO activeQuest;
    [SerializeField] private List<QuestDataSO> completedQuests = new List<QuestDataSO>();

    public QuestDataSO ActiveQuest => activeQuest;

    public void Initialize()
    {
        // Zustand zurücksetzen, damit beim neuen Spielstart im Editor alles leer ist
        activeQuest = null;
        completedQuests.Clear();

        if (storyFlagChangedBroadcastChannel != null)
        {
            storyFlagChangedBroadcastChannel.OnEventRaised -= EvaluateQuests;
            storyFlagChangedBroadcastChannel.OnEventRaised += EvaluateQuests;
        }
    }

    private void EvaluateQuests(string changedFlag)
    {
        if (allAvailableQuests == null || allAvailableQuests.questList == null) return;

        foreach (QuestDataSO quest in allAvailableQuests.questList)
        {
            if (quest == null) continue;

            // 1. Quest aktivieren mit dem benötigten Start-Flag
            if (changedFlag == quest.requiredStoryFlag && activeQuest != quest && !completedQuests.Contains(quest))
            {
                ActivateQuest(quest);
            }

            // 2. Quest beenden mit dem benötigten Completion-Flag
            if (changedFlag == quest.completionFlag && activeQuest == quest)
            {
                CompleteQuest(quest);
            }
        }
    }

    private void ActivateQuest(QuestDataSO quest)
    {
        activeQuest = quest;
        Debug.Log($"[QuestManagerSO] Quest gestartet: {quest.questName}");

        if (questChangedChannel != null)
            questChangedChannel.RaiseEvent(true);
    }

    private void CompleteQuest(QuestDataSO quest)
    {
        if (activeQuest == quest)
        {
            activeQuest = null;
        }

        if (!completedQuests.Contains(quest))
        {
            completedQuests.Add(quest);
        }

        Debug.Log($"[QuestManagerSO] Quest abgeschlossen: {quest.questName}");

        if (questChangedChannel != null)
            questChangedChannel.RaiseEvent(true);
    }
}