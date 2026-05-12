using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private DialogueDatabaseSO currentDatabase;
    [SerializeField] private BoolEventChannelSO toggleInputChannel;
    [SerializeField] private BoolEventChannelSO dialogueEventChannel;
    [SerializeField] private DialogueEventChannelSO dialogueEventChannelString;
    [SerializeField] private VoidEventChannelSO dialogueEndedChannel;


    public System.Action<string, string> OnDialogueStarted;
    private void OnEnable()
    {
        dialogueEndedChannel.OnEventRaised += EndDialogue;
    }

    private void OnDisable()
    {
        dialogueEndedChannel.OnEventRaised -= EndDialogue;
    }

    public void StartDialogue(string dialogueKey, string npcName = "???")
    {
        string localizedText = currentDatabase.GetText(dialogueKey);

        DialogueData data = new DialogueData();
        data.Text = localizedText;
        data.SpeakerName = npcName;

        toggleInputChannel.RaiseEvent(false);
        dialogueEventChannel.RaiseEvent(true);

        dialogueEventChannelString.RaiseEvent(data);
        Debug.Log($"Dialog gestartet: {npcName} sagt {dialogueKey}");
    }

    public void EndDialogue()
    {
        toggleInputChannel?.RaiseEvent(true);
        dialogueEventChannel?.RaiseEvent(false);
    }

}
