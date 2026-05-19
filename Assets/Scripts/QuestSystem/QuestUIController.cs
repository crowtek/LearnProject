using System;
using UnityEngine;
using UnityEngine.UIElements;

public class QuestUIController : MonoBehaviour
{
    [Header("Dependencies (Asset References)")]
    [SerializeField] private QuestManagerSO questManagerSO;
    [SerializeField] private BoolEventChannelSO questChangedChannel;

    [Header("Scene Dependencies")]
    [SerializeField] private UIDocument uiDocument;

    private VisualElement questContainer;
    private Label questName;
    private Label questDiscription;

    private void Awake()
    {
        var root = uiDocument.rootVisualElement;
        questContainer = root.Q<VisualElement>("QuestContainer");
        questName = root.Q<Label>("QuestName");
        questDiscription = root.Q<Label>("QuestDiscription");
    }

    private void OnEnable()
    {
        if (questChangedChannel != null)
        {
            questChangedChannel.OnEventRaised += RefreshQuestUI;
        }

        RefreshQuestUI(true);
    }

    void OnDisable()
    {
        if(questChangedChannel != null)
        {
            questChangedChannel.OnEventRaised -= RefreshQuestUI;
        }
    }

    private void RefreshQuestUI(bool isUpdated)
    {
        if (questContainer == null || questName == null || questManagerSO == null) return;

        questContainer.Clear();

        if (questManagerSO.ActiveQuest == null)
        {
            questName.text = "Keine aktive Quest";
            return;
        }

        questName.text = questManagerSO.ActiveQuest.questName;
        questDiscription.text = questManagerSO.ActiveQuest.description;
    }
}