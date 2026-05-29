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
    private VisualElement choiceContainer;
    private Label nameLabel;
    private Label textLabel;
    private Image npcImage;

    private string fullText, activeResultFlag;
    private bool isDialogueActive = false;
    private string[] currentLines;
    private DialogueChoice[] currentChoices;
    private System.Func<string, DialogueData> currentDialogueResolver;
    private int currentLineIndex = 0;


    private void OnEnable()
    {
        root = uiDocument.rootVisualElement;
        dialogueBox = root.Q<VisualElement>("DialogueBox");
        choiceContainer = root.Q<VisualElement>("ChoiceContainer");
        nameLabel = root.Q<Label>("NPCNameLabel");
        textLabel = root.Q<Label>("DialogueText");
        npcImage = root.Q<Image>("NPCImage");

        dialogueBox.RegisterCallback<PointerDownEvent>(OnBoxClicked);
        dialogueDataChannel.OnEventRaised += StartDialogue;
    }
    private void OnDisable()
    {
        if (dialogueBox != null)
        {
            dialogueBox.UnregisterCallback<PointerDownEvent>(OnBoxClicked);
        }

        if (dialogueDataChannel != null)
        {
            dialogueDataChannel.OnEventRaised -= StartDialogue;
        }
    }

    private void StartDialogue(DialogueData data)
    {
        activeResultFlag = data.ResultFlag;
        currentChoices = data.Choices;
        currentDialogueResolver = data.DialogueResolver;

        toggleInputChannel?.RaiseEvent(false);
        dialogueEventChannel?.RaiseEvent(true);

        isDialogueActive = true;
        dialogueBox.style.display = DisplayStyle.Flex;
        HideChoices();
        nameLabel.text = data.SpeakerName;
        npcImage.sprite = data.SpeakerPortrait;

        currentLines = data.Lines ?? System.Array.Empty<string>();
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
        else if (HasChoices())
        {
            ShowChoices();
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
        else if (!AreChoicesVisible())
        {
            DisplayNextLine();
        }
    }

    private bool HasChoices() => currentChoices != null && currentChoices.Length > 0;

    private bool AreChoicesVisible()
    {
        return choiceContainer != null && choiceContainer.style.display == DisplayStyle.Flex;
    }

    private void ShowChoices()
    {
        if (choiceContainer == null)
        {
            CloseDialogue();
            return;
        }

        ApplyResultFlag(activeResultFlag);
        activeResultFlag = null;

        choiceContainer.Clear();
        choiceContainer.style.display = DisplayStyle.Flex;

        foreach (DialogueChoice choice in currentChoices)
        {
            var button = new Button(() => SelectChoice(choice))
            {
                text = choice.displayText
            };

            button.AddToClassList("choiceButton");
            button.RegisterCallback<PointerDownEvent>(evt => evt.StopPropagation());
            choiceContainer.Add(button);
        }
    }

    private void HideChoices()
    {
        if (choiceContainer == null) return;

        choiceContainer.Clear();
        choiceContainer.style.display = DisplayStyle.None;
    }

    private void SelectChoice(DialogueChoice choice)
    {
        ApplyResultFlag(choice.resultFlag);
        HideChoices();

        if (!string.IsNullOrEmpty(choice.nextDialogueKey) && currentDialogueResolver != null)
        {
            StartDialogue(currentDialogueResolver.Invoke(choice.nextDialogueKey));
            return;
        }

        CloseDialogue();
    }

    private void CloseDialogue()
    {
        if (typewriter != null)
        {
            typewriter.StopTyping();
        }

        HideChoices();
        dialogueBox.style.display = DisplayStyle.None;
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
}