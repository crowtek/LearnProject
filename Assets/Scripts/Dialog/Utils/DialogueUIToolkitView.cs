using System;
using UnityEngine;
using UnityEngine.UIElements;

// UI Toolkit implementation of IDialogueView.
public class DialogueUIToolkitView : MonoBehaviour, IDialogueView
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private TypewriterHandler typewriter;
    [SerializeField] private float typewritingSpeed = 0.05f;

    private VisualElement root;
    private VisualElement dialogueBox;
    private VisualElement choiceContainer;
    private Label nameLabel;
    private Label textLabel;
    private Image npcImage;

    public event Action ContinueRequested;
    public event Action<DialogueChoice> ChoiceSelected;

    public bool IsDisplayingLine => typewriter != null && typewriter.IsTyping;

    private void Awake()
    {
        InitializeElements();
    }

    private void OnEnable()
    {
        InitializeElements();

        if (dialogueBox != null)
        {
            dialogueBox.RegisterCallback<PointerDownEvent>(OnBoxClicked);
        }
    }

    private void OnDisable()
    {
        if (dialogueBox != null)
        {
            dialogueBox.UnregisterCallback<PointerDownEvent>(OnBoxClicked);
        }
    }

    public void Show(DialogueData data)
    {
        InitializeElements();

        if (dialogueBox != null)
        {
            dialogueBox.style.display = DisplayStyle.Flex;
        }

        HideChoices();

        if (nameLabel != null)
        {
            nameLabel.text = data.SpeakerName;
        }

        if (npcImage != null)
        {
            npcImage.sprite = data.SpeakerPortrait;
        }
    }

    public void DisplayLine(string line)
    {
        if (textLabel == null)
        {
            return;
        }

        if (typewriter != null)
        {
            typewriter.RunText(textLabel, line, typewritingSpeed);
            return;
        }

        textLabel.text = line;
    }

    public void CompleteLine(string line)
    {
        if (typewriter != null)
        {
            typewriter.StopTyping();
        }

        if (textLabel != null)
        {
            textLabel.text = line;
        }
    }

    public void ShowChoices(DialogueChoice[] choices)
    {
        if (choiceContainer == null)
        {
            return;
        }

        choiceContainer.Clear();
        choiceContainer.style.display = DisplayStyle.Flex;

        if (choices == null)
        {
            return;
        }

        foreach (DialogueChoice choice in choices)
        {
            DialogueChoice capturedChoice = choice;
            var button = new Button(() => ChoiceSelected?.Invoke(capturedChoice))
            {
                text = capturedChoice.displayText
            };

            button.AddToClassList("choiceButton");
            button.RegisterCallback<PointerDownEvent>(evt => evt.StopPropagation());
            choiceContainer.Add(button);
        }
    }

    public void Hide()
    {
        if (typewriter != null)
        {
            typewriter.StopTyping();
        }

        HideChoices();

        if (dialogueBox != null)
        {
            dialogueBox.style.display = DisplayStyle.None;
        }
    }

    private void InitializeElements()
    {
        if (uiDocument == null || root != null)
        {
            return;
        }

        root = uiDocument.rootVisualElement;
        dialogueBox = root.Q<VisualElement>("DialogueBox");
        choiceContainer = root.Q<VisualElement>("ChoiceContainer");
        nameLabel = root.Q<Label>("NPCNameLabel");
        textLabel = root.Q<Label>("DialogueText");
        npcImage = root.Q<Image>("NPCImage");
    }

    private void HideChoices()
    {
        if (choiceContainer == null)
        {
            return;
        }

        choiceContainer.Clear();
        choiceContainer.style.display = DisplayStyle.None;
    }

    private void OnBoxClicked(PointerDownEvent evt)
    {
        if (choiceContainer != null && choiceContainer.style.display == DisplayStyle.Flex)
        {
            return;
        }

        ContinueRequested?.Invoke();
    }
}