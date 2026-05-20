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

    [Header("UI configs")]
    [SerializeField] private float typewritingSpeed = 0.05f;
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private TypewriterHandler typewriter; // Handlle typeing effect

    // UI Elemenets
    private VisualElement root;
    private VisualElement dialogueBox;
    private Label nameLabel;
    private Label textLabel;
    private Image npcImage;

    private string fullText, activeResultFlag;
    private bool isDialogueActive = false;
    private string[] currentLines; 
    private int currentLineIndex = 0;


    private void OnEnable()
    {
        root = uiDocument.rootVisualElement;
        dialogueBox = root.Q<VisualElement>("DialogueBox");
        nameLabel = root.Q<Label>("NPCNameLabel");
        textLabel = root.Q<Label>("DialogueText");
        npcImage = root.Q<Image>("NPCImage");

        dialogueBox.RegisterCallback<PointerDownEvent>(OnBoxClicked);
        dialogueDataChannel.OnEventRaised += StartDialogue;
    }
    private void OnDisable()
    {
        dialogueDataChannel.OnEventRaised -= StartDialogue;
    }

    private void StartDialogue(DialogueData data)
    {
        activeResultFlag = data.ResultFlag;
        toggleInputChannel.RaiseEvent(false);
        dialogueEventChannel.RaiseEvent(true);

        isDialogueActive = true;
        dialogueBox.style.display = DisplayStyle.Flex;
        nameLabel.text = data.SpeakerName;
        npcImage.sprite = data.SpeakerPortrait;

        currentLines = data.Lines; 
        currentLineIndex = 0;

        DisplayNextLine();
    }

    private void DisplayNextLine()
    {
        if (currentLineIndex < currentLines.Length)
        {
            fullText = currentLines[currentLineIndex];
            typewriter.RunText(textLabel, fullText, typewritingSpeed);

            currentLineIndex++;
        }
        else
        {
            CloseDialogue();
        }
    }

    private void OnBoxClicked(PointerDownEvent evt)
    {
        if (!isDialogueActive) return;

        if (typewriter != null && typewriter.IsTyping)
        {
            // Stop type Coroutine
            typewriter.StopTyping();

            // Show complete text
            textLabel.text = fullText;
        }
        else
        {
            DisplayNextLine();
        }
    }

    private void CloseDialogue()
    {
        if (typewriter != null)
        {
            typewriter.StopTyping();
        }

        dialogueBox.style.display = DisplayStyle.None;
        isDialogueActive = false;

        if (!string.IsNullOrEmpty(activeResultFlag) && setStoryFlagRequestChannel != null)
        {
            setStoryFlagRequestChannel.RaiseEvent(activeResultFlag);
            activeResultFlag = null;
        }

        toggleInputChannel?.RaiseEvent(true);
        dialogueEventChannel?.RaiseEvent(false);
    }
}