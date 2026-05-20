using UnityEngine;

public class StoryCutsceneTrigger : MonoBehaviour
{
    [Header("Database Asset Reference")]
    [SerializeField] private StoryCutsceneDatabaseSO cutsceneDatabase;

    [Header("Channels (Asset References)")]
    [SerializeField] private DialogueDatabaseSO dialogueDatabase;
    [SerializeField] private StringEventChannelSO storyFlagChangedBroadcastChannel;
    [SerializeField] private DialogueEventChannelSO dialogueChannel;

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
        if (cutsceneDatabase == null || dialogueDatabase == null || dialogueChannel == null) return;

        // Check if story flag is in DB
        if (cutsceneDatabase.TryGetCutscene(incomingFlag, out CutsceneDialogueEntry cutscene))
        {
            ExecuteCutsceneDialogue(cutscene);
        }
    }

    private void ExecuteCutsceneDialogue(CutsceneDialogueEntry cutscene)
    {
        var entry = dialogueDatabase.GetEntry(cutscene.dialogueKey);

        if (entry.conversationLines == null || entry.conversationLines.Length == 0)
        {
            Debug.LogWarning($"[StoryCutsceneTrigger] Dialogue key '{cutscene.dialogueKey}' has no lines.");
            return;
        }

        //Prep data for UI controller
        var data = new DialogueData
        {
            Lines = entry.conversationLines,
            SpeakerName = cutscene.speakerName,
            SpeakerPortrait = cutscene.speakerPortrait,
            ResultFlag = entry.resultFlag
        };

        dialogueChannel.RaiseEvent(data);
    }
}