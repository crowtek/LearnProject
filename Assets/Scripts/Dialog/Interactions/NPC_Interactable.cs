using UnityEngine;

public class NPC_Interactable : MonoBehaviour, IInteractable
{
    [SerializeField] private string npcName;
    [SerializeField] private string dialogueKey;
    [SerializeField] private DialogueManager dialogueManager;

    public void Interact()
    {
        dialogueManager.StartDialogue(dialogueKey, npcName);
    }

    public string GetInteractPrompt() => $"Mit {npcName} sprechen";
}