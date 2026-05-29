using UnityEngine;
using UnityEngine.UIElements;

public class DialogueUIController : MonoBehaviour
{
    [Header("Dialog Channals")]
    [SerializeField] private BoolEventChannelSO dialogueEventChannel; // Tells other systems if dialog is happening
    [SerializeField] private DialogueEventChannelSO dialogueDataChannel; // Get dialog data and starts dialog

    [Header("Story Channel")]
    [SerializeField] private StringEventChannelSO setStoryFlagRequestChannel; // posible set of story flag after dialog

    [Header("Player Input stop handler")]
    [SerializeField] private BoolEventChannelSO toggleInputChannel; // Stops player input while dialog aktive

    [Header("Audio Feedback")]
    [SerializeField] private AudioEventChannelSO sfxRequestChannel;
    [SerializeField] private AudioConfigurationSO dialogueLineSfx;

    [Header("Dialogue View")]
    [Tooltip("Component that implements IDialogueView (UI Toolkit, uGUI, TextMeshPro, world-space bubble, etc.).")]
    [SerializeField] private MonoBehaviour dialogueViewComponent;

    private IDialogueView dialogueView;
    private string fullText, activeResultFlag;
    private bool isDialogueActive = false;
    private string[] currentLines;
    private DialogueChoice[] currentChoices;
    private System.Func<string, DialogueData> currentDialogueResolver;
    private int currentLineIndex = 0;

    private void Awake()
    {
        ResolveDialogueView();
    }

    private void OnEnable()
    {
        ResolveDialogueView();

        if (dialogueView != null)
        {
            dialogueView.ContinueRequested += OnContinueRequested;
            dialogueView.ChoiceSelected += SelectChoice;
        }

        if (dialogueDataChannel != null)
        {
            dialogueDataChannel.OnEventRaised += StartDialogue;
        }
    }
    private void OnDisable()
    {
        if (dialogueView != null)
        {
            dialogueView.ContinueRequested -= OnContinueRequested;
            dialogueView.ChoiceSelected -= SelectChoice;
        }

        if (dialogueDataChannel != null)
        {
            dialogueDataChannel.OnEventRaised -= StartDialogue;
        }
    }

    private void StartDialogue(DialogueData data)
    {
        if (dialogueView == null)
        {
            Debug.LogError($"{nameof(DialogueUIController)} requires a component that implements {nameof(IDialogueView)}.", this);
            return;
        }

        activeResultFlag = data.ResultFlag;
        currentChoices = data.Choices;
        currentDialogueResolver = data.DialogueResolver;

        toggleInputChannel?.RaiseEvent(false);
        dialogueEventChannel?.RaiseEvent(true);

        isDialogueActive = true;
        dialogueView.Show(data);

        currentLines = data.Lines ?? System.Array.Empty<string>();
        currentLineIndex = 0;

        DisplayNextLine();
    }

    private void DisplayNextLine()
    {
        if (currentLineIndex < currentLines.Length)
        {
            fullText = currentLines[currentLineIndex];
            dialogueView.DisplayLine(fullText);
            PlayDialogueLineSfx();

            currentLineIndex++;
        }
        else if (HasChoices())
        {
            ShowChoices();
        }
        else
        {
            CloseDialogue();
        }
    }

    private void PlayDialogueLineSfx()
    {
        if (sfxRequestChannel != null && dialogueLineSfx != null)
        {
            sfxRequestChannel.RaiseEvent(dialogueLineSfx);
        }
    }

    private void OnContinueRequested()
    {
        if (!isDialogueActive) return;

        if (dialogueView.IsDisplayingLine)
        {
            dialogueView.CompleteLine(fullText);
        }
        else
        {
            DisplayNextLine();
        }
    }

    private bool HasChoices() => currentChoices != null && currentChoices.Length > 0;

    private void ShowChoices()
    {

        ApplyResultFlag(activeResultFlag);
        activeResultFlag = null;

        dialogueView.ShowChoices(currentChoices);
    }

    private void SelectChoice(DialogueChoice choice)
    {
        ApplyResultFlag(choice.resultFlag);

        if (!string.IsNullOrEmpty(choice.nextDialogueKey) && currentDialogueResolver != null)
        {
            StartDialogue(currentDialogueResolver.Invoke(choice.nextDialogueKey));
            return;
        }

        CloseDialogue();
    }

    private void CloseDialogue()
    {
        dialogueView?.Hide();
        isDialogueActive = false;

        ApplyResultFlag(activeResultFlag);
        activeResultFlag = null;

        toggleInputChannel?.RaiseEvent(true);
        dialogueEventChannel?.RaiseEvent(false);
    }

    private void ApplyResultFlag(string resultFlag)
    {
        if (!string.IsNullOrEmpty(resultFlag) && setStoryFlagRequestChannel != null)
        {
            setStoryFlagRequestChannel.RaiseEvent(resultFlag);
        }
    }

    private void ResolveDialogueView()
    {
        if (dialogueView != null)
        {
            return;
        }

        if (dialogueViewComponent != null)
        {
            dialogueView = dialogueViewComponent as IDialogueView;
        }

        if (dialogueView == null)
        {
            dialogueView = GetComponent<IDialogueView>();
        }
    }
}