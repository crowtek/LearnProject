using System.Collections.Generic;
using UnityEngine;

public class NPC_Interactable : MonoBehaviour, IInteractable
{
    [System.Serializable]
    public struct DialogueCondition
    {
        [StoryFlag] public string requiredFlag; 
        public string dialogueKey;
    }

    [Header("Identity")]
    [SerializeField] private string npcName;
    [SerializeField] private Sprite npcImage;

    [Header("Dialogue Logic")]
    [SerializeField] private GlobalStoryStateSO storyState; 
    [SerializeField] private string fallbackDialogueKey; 
    [SerializeField] private List<DialogueCondition> prioritizedDialogues; 

    [Header("Channels")]
    [SerializeField] private DialogueEventChannelSO dialogueChannel;
    [SerializeField] private DialogueDatabaseSO dialogueDatabase;

    public void Interact()
    {
        string selectedKey = fallbackDialogueKey;

        foreach (var condition in prioritizedDialogues)
        {
            if (storyState.IsFlagCompleted(condition.requiredFlag))
            {
                selectedKey = condition.dialogueKey;
                break;
            }
        }

        var entry = dialogueDatabase.dialogueEntries.Find(e => e.key == selectedKey);

        var data = new DialogueData
        {
            Lines = dialogueDatabase.GetText(selectedKey),
            SpeakerName = npcName,
            SpeakerPortrait = npcImage,
            ResultFlag = entry.resultFlag,
        };

        dialogueChannel.RaiseEvent(data);
    }

    public string GetInteractPrompt() => $"Mit {npcName} sprechen";
}