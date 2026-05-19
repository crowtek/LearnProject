using System;
using UnityEngine;
using UnityEngine.UIElements;

public class QuestUIController : MonoBehaviour
{
    [SerializeField] private QuestManager questManager;
    [SerializeField] private UIDocument uiDocument;

    private VisualElement questContainer;
    private Label questName;

    private QuestDataSO currentQuest;

    private void OnEnable()
    {
        var root = uiDocument.rootVisualElement;
        questContainer = root.Q<VisualElement>("QuestContainer");
        questName = root.Q<Label>("QuestName");

        if (questManager != null)
        {
            questManager.OnQuestListChanged += RefreshQuestUI;
        }
    }

    void OnDisable()
    {
        if(questManager != null)
        {
            questManager.OnQuestListChanged -= RefreshQuestUI;
        }
    }

    private void RefreshQuestUI()
    {
        questContainer.Clear();

        if (questManager.GetActiveQuest.Count == 0) return;
        
        foreach (var quest in questManager.GetActiveQuest)
        {
            Label newQuestLabel = new Label($"Current Quest: {quest.description}");
            questContainer.Add(newQuestLabel);
        }

        // Den Namen der neuesten Quest als Haupttitel setzen
        questName.text = questManager.GetActiveQuest[questManager.GetActiveQuest.Count - 1].questName;
    }
}
