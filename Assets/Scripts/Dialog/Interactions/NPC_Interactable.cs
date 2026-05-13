using UnityEngine;

public class NPC_Interactable : MonoBehaviour, IInteractable
{
    [SerializeField] private string npcName;
    [SerializeField] private string dialogueKey;
    [SerializeField] private DialogueEventChannelSO dialogueChannel;
    [SerializeField] private DialogueDatabaseSO dialogueDatabase;

    public void Interact()
    {
        var data = new DialogueData
        {
            Lines = dialogueDatabase.GetText(dialogueKey),
            SpeakerName = npcName
        };
        dialogueChannel.RaiseEvent(data);
    }

    public string GetInteractPrompt() => $"Mit {npcName} sprechen";
}