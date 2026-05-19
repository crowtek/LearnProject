using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private BoolEventChannelSO toggleInputChannel;
    [SerializeField] private BoolEventChannelSO dialogueEventChannel;
    [SerializeField] private DialogueEventChannelSO dialogueChannelData;
    [SerializeField] private VoidEventChannelSO dialogueEndedChannel;

    [Header("Story Channel")]
    [SerializeField] private StringEventChannelSO setStoryFlagRequestChannel;

    private string activeResultFlag;

    public System.Action<string, string> OnDialogueStarted;
    private void OnEnable()
    {
        dialogueChannelData.OnEventRaised += StartDialogue;
        dialogueEndedChannel.OnEventRaised += EndDialogue;
    }

    private void OnDisable()
    {
        dialogueChannelData.OnEventRaised -= StartDialogue;
        dialogueEndedChannel.OnEventRaised -= EndDialogue;
    }

    public void StartDialogue(DialogueData data)
    {
        activeResultFlag = data.ResultFlag;
        toggleInputChannel.RaiseEvent(false);
        dialogueEventChannel.RaiseEvent(true);
    }

    public void EndDialogue()
    {
        if (!string.IsNullOrEmpty(activeResultFlag) && setStoryFlagRequestChannel != null)
        {
            setStoryFlagRequestChannel.RaiseEvent(activeResultFlag);
            activeResultFlag = null;
        }

        toggleInputChannel?.RaiseEvent(true);
        dialogueEventChannel?.RaiseEvent(false);
    }

}
