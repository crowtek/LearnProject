using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private BoolEventChannelSO toggleInputChannel;
    [SerializeField] private BoolEventChannelSO dialogueEventChannel;
    [SerializeField] private DialogueEventChannelSO dialogueEventChannelString;
    [SerializeField] private VoidEventChannelSO dialogueEndedChannel;


    public System.Action<string, string> OnDialogueStarted;
    private void OnEnable()
    {
        dialogueEventChannelString.OnEventRaised += StartDialogue;
        dialogueEndedChannel.OnEventRaised += EndDialogue;
    }

    private void OnDisable()
    {
        dialogueEventChannelString.OnEventRaised -= StartDialogue;
        dialogueEndedChannel.OnEventRaised -= EndDialogue;
    }

    public void StartDialogue(DialogueData data)
    {
        toggleInputChannel.RaiseEvent(false);
        dialogueEventChannel.RaiseEvent(true);
    }

    public void EndDialogue()
    {
        toggleInputChannel?.RaiseEvent(true);
        dialogueEventChannel?.RaiseEvent(false);
    }

}
