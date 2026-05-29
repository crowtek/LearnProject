using UnityEngine;

public class StoryCutsceneTrigger : MonoBehaviour
{
    [Header("Database Asset Reference")]
    [SerializeField] private StoryCutsceneDatabaseSO cutsceneDatabase;

    [Header("Channels (Asset References)")]
    [SerializeField] private LanguageManagerSO languageManager;
    [SerializeField] private DialogueDatabaseSO dialogueDatabase;
    [SerializeField] private StringEventChannelSO storyFlagChangedBroadcastChannel;
    [SerializeField] private DialogueEventChannelSO dialogueChannel;

    private DialogueDatabaseSO ActiveDialogueDatabase =>
    languageManager != null ? languageManager.CurrentDatabase : dialogueDatabase;

    private void OnEnable()
    {
        if (storyFlagChangedBroadcastChannel != null)
        {
            storyFlagChangedBroadcastChannel.OnEventRaised += OnStoryFlagBroadcasted;
        }
    }

    private void OnDisable()
    {
        if (storyFlagChangedBroadcastChannel != null)
        {
            storyFlagChangedBroadcastChannel.OnEventRaised -= OnStoryFlagBroadcasted;
        }
    }

    private void OnStoryFlagBroadcasted(string incomingFlag)
    {
        if (cutsceneDatabase == null || ActiveDialogueDatabase == null || dialogueChannel == null) return;

        // Check if story flag is in DB
        if (cutsceneDatabase.TryGetCutscene(incomingFlag, out CutsceneDialogueEntry cutscene))
        {
            ExecuteCutsceneDialogue(cutscene);
        }
    }

    private DialogueData CreateDialogueData(string dialogueKey, string speakerName, Sprite speakerPortrait)
    {
        var entry = ActiveDialogueDatabase.GetEntry(dialogueKey);

        return new DialogueData
        {
            Lines = entry.conversationLines,
            SpeakerName = speakerName,
            SpeakerPortrait = speakerPortrait,
            ResultFlag = entry.resultFlag,
            Choices = entry.choices,
            DialogueResolver = key => CreateDialogueData(key, speakerName, speakerPortrait)
        };
    }

    private void ExecuteCutsceneDialogue(CutsceneDialogueEntry cutscene)
    {
        var data = CreateDialogueData(cutscene.dialogueKey, cutscene.speakerName, cutscene.speakerPortrait);

        if (data.Lines == null || data.Lines.Length == 0)
        {
            Debug.LogWarning($"[StoryCutsceneTrigger] Dialogue key '{cutscene.dialogueKey}' has no lines.");
            return;
        }

        dialogueChannel.RaiseEvent(data);
    }
}