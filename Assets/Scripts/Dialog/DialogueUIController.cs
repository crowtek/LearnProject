using Cysharp.Threading.Tasks; // UniTask Namespace
using System.Data;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueUIController : MonoBehaviour
{
    [SerializeField] private float typewritingSpeed = 0.05f;
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private DialogueEventChannelSO dialogueChannel;
    [SerializeField] private VoidEventChannelSO dialogEndedChannel;

    private VisualElement root;
    private VisualElement dialogueBox;
    private Label nameLabel;
    private Label textLabel;

    private string fullText;
    private bool isTyping = false;
    private bool isDialogueActive = false;

    private CancellationTokenSource cts;

    private void OnEnable()
    {
        root = uiDocument.rootVisualElement;
        dialogueBox = root.Q<VisualElement>("DialogueBox");
        nameLabel = root.Q<Label>("NPCNameLabel");
        textLabel = root.Q<Label>("DialogueText");

        // DEBUG: Prüfen ob alles gefunden wurde
        if (nameLabel == null) Debug.LogError("UI Error: NPCNameLabel nicht im UXML gefunden!");
        if (textLabel == null) Debug.LogError("UI Error: DialogueText nicht im UXML gefunden!");

        dialogueBox.RegisterCallback<PointerDownEvent>(OnBoxClicked);

        dialogueChannel.OnEventRaised += StartDialogue;
    }
    private void OnDisable()
    {
        dialogueChannel.OnEventRaised -= StartDialogue;
    }

    private void StartDialogue(DialogueData data)
    {
        isDialogueActive = true;
        dialogueBox.style.display = DisplayStyle.Flex;
        nameLabel.text = data.SpeakerName; // Korrigiert
        fullText = data.Text;              // Korrigiert

        RunTypewriter(fullText).Forget();
    }

    private async UniTaskVoid RunTypewriter(string text)
    {
        isTyping = true;
        textLabel.text = "";

        cts?.Cancel();
        cts = new CancellationTokenSource();

        try
        {
            foreach (char c in text)
            {
                textLabel.text += c;
                await UniTask.Delay((int)(typewritingSpeed * 1000), cancellationToken: cts.Token);
            }
            isTyping = false;
        }
        catch (System.OperationCanceledException)
        {
            // Task abgebrochen
        }
    }

    private void OnBoxClicked(PointerDownEvent evt)
    {
        if (!isDialogueActive) return;

        if (isTyping)
        {
            cts?.Cancel();
            textLabel.text = fullText;
            isTyping = false;
        }
        else
        {
            CloseDialogue();
        }
    }

    private void CloseDialogue()
    {
        dialogueBox.style.display = DisplayStyle.None;
        isDialogueActive = false;
        cts?.Cancel();

        dialogEndedChannel.RaiseEvent();
    }
}