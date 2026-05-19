using UnityEngine;

public class StoryManager : MonoBehaviour
{
    [Header("Story State Storage")]
    [SerializeField] private GlobalStoryStateSO storyState;

    [Header("Channel Input")]
    // Other system request the setting of flags through here.
    [SerializeField] private StringEventChannelSO setFlagRequestChannel;

    [Header("Channel Output")]
    // Other systems are informed about flag changes through here.
    [SerializeField] private StringEventChannelSO storyEventOutputChannel;

    // Somthing like a last quest on top of the stack to now wiche story flag is the newest right now. 
    private string lastStoryFlag;

    private void Start()
    {
        // Other system get all completed story flag at the start of the game
        RevealCurrentStoryFlag();
    }

    private void OnEnable()
    {
        if(setFlagRequestChannel != null)
        {
            setFlagRequestChannel.OnEventRaised += HandleFlagChangeRequest;
        }
    }

    private void OnDisable()
    {
        if (setFlagRequestChannel != null)
        {
            setFlagRequestChannel.OnEventRaised -= HandleFlagChangeRequest;
        }
    }

    public void HandleFlagChangeRequest(string flagName)
    {
        if (string.IsNullOrEmpty(flagName) || storyState == null) return;

        storyState.SetFlag(flagName); // Story flag are set

        // Other system get Info about Flag change
        if(storyEventOutputChannel != null)
        {
            storyEventOutputChannel.RaiseEvent(flagName);
        }
    }

    private void RevealCurrentStoryFlag()
    {
        if (storyState != null && storyEventOutputChannel != null)
        {
            foreach (string flag in storyState.completedFlags)
            {
                storyEventOutputChannel.RaiseEvent(flag);
            }
        }

        lastStoryFlag = storyState.GetLastFlag();
    }
}
