using System.Collections.Generic;
using UnityEngine;

public class NPC_Interactable : MonoBehaviour, IInteractable
{
    [Header("Identity")]
    [SerializeField] private string npcName;
    [SerializeField] private Sprite npcImage;

    [Header("Dialogue Logic")]
    [DialogueKey] [SerializeField] private string fallbackDialogueKey; 
    [SerializeField] private List<DialogueCondition> prioritizedDialogues;

    [Header("Story Chanenel")]
    [SerializeField] private StringEventChannelSO StoryBroadcastChannel;

    [Header("Diallog Channels")]
    [SerializeField] private DialogueEventChannelSO dialogueChannel;
    [SerializeField] private DialogueDatabaseSO dialogueDatabase;
    [SerializeField] private LanguageManagerSO languageManager;

    [Header("UI Placement")]
    [SerializeField] private Transform interactionPoint;

    private HashSet<string> completedStoryFlags = new HashSet<string>(); // save all completed story flags

    private DialogueDatabaseSO ActiveDialogueDatabase =>
       languageManager != null ? languageManager.CurrentDatabase : dialogueDatabase;

    private void OnEnable()
    {
        if(StoryBroadcastChannel != null)
        {
            StoryBroadcastChannel.OnEventRaised += RegisterStoryFlag;
        }
    }

    private void OnDisable()
    {
        if (StoryBroadcastChannel != null)
        {
            StoryBroadcastChannel.OnEventRaised -= RegisterStoryFlag;
        }
    }

    private void RegisterStoryFlag(string setFlag) // Gets all completed story flag at game start
    {
        if (!string.IsNullOrEmpty(setFlag) && !completedStoryFlags.Contains(setFlag))
        {
            completedStoryFlags.Add(setFlag);
        }
    }

    public void Interact()
    {
        string selectedKey = fallbackDialogueKey;

        // Check for the dialog option with highest Prio in the list
        foreach (var condition in prioritizedDialogues)
        {
            if (condition.IsMet(completedStoryFlags))
            {
                selectedKey = condition.dialogueKey;
                break;
            }
        }

        dialogueChannel.RaiseEvent(CreateDialogueData(selectedKey));
    }
    private DialogueData CreateDialogueData(string dialogueKey)
    {
        DialogueDatabaseSO database = ActiveDialogueDatabase;
        if (database == null)
        {
            Debug.LogWarning($"[NPC_Interactable] No dialogue database assigned for NPC {npcName}.");
            return default;
        }

        var entry = database.GetEntry(dialogueKey);

        return new DialogueData
        {
            Lines = entry.conversationLines,
            SpeakerName = npcName,
            SpeakerPortrait = npcImage,
            ResultFlag = entry.resultFlag,
            Choices = entry.choices,
            DialogueResolver = CreateDialogueData
        };
    }

    public string GetInteractPrompt()
    {
        DialogueDatabaseSO database = ActiveDialogueDatabase;
        return database != null ? database.GetInteractPrompt(npcName) : npcName;
    }

    public Transform GetInteractionPoint()
    {
        return interactionPoint != null ? interactionPoint : transform;
    }
}