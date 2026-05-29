using System.IO;
using UnityEngine;

public class StoryManager : MonoBehaviour
{
    [Header("Story State Storage")]
    [SerializeField] private GlobalStoryStateSO storyState;

    [Header("Save Settings")]
    [SerializeField] private bool loadProgressOnAwake = true;
    [SerializeField] private bool saveProgressOnFlagChange = true;
    [SerializeField] private bool saveProgressOnQuit = true;
    [SerializeField] private string saveFileName = "story-progress.json";

    [Header("Channel Input")]
    // Other system request the setting of flags through here.
    [SerializeField] private StringEventChannelSO setFlagRequestChannel;

    [Header("Channel Output")]
    // Other systems are informed about flag changes through here.
    [SerializeField] private StringEventChannelSO storyEventOutputChannel;

    // Somthing like a last quest on top of the stack to now wiche story flag is the newest right now. 
    private string lastStoryFlag;
    private ISaveHandler saveHandler;

    private void Awake()
    {
        saveHandler = new JsonStorySaveHandler();

        if (loadProgressOnAwake)
        {
            LoadProgress(false);
        }
    }

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

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && saveProgressOnQuit)
        {
            SaveProgress();
        }
    }

    private void OnApplicationQuit()
    {
        if (saveProgressOnQuit)
        {
            SaveProgress();
        }
    }

    public void HandleFlagChangeRequest(string flagName)
    {
        if (string.IsNullOrEmpty(flagName) || storyState == null) return;

        bool wasNewFlag = storyState.SetFlag(flagName); // Story flag are set
        lastStoryFlag = storyState.GetLastFlag();

        if (wasNewFlag && saveProgressOnFlagChange)
        {
            SaveProgress();
        }

        // Other system get Info about Flag change
        if (storyEventOutputChannel != null)
        {
            storyEventOutputChannel.RaiseEvent(flagName);
        }
    }

    public void SaveProgress()
    {
        if (storyState == null)
        {
            Debug.LogWarning("Story progress was not saved because no GlobalStoryStateSO is assigned.", this);
            return;
        }

        saveHandler ??= new JsonStorySaveHandler();
        saveHandler.Save(storyState.CreateSaveData(), GetSaveFilePath());
    }

    public void LoadProgress()
    {
        LoadProgress(true);
    }

    public void LoadProgress(bool revealLoadedFlags)
    {
        if (storyState == null)
        {
            Debug.LogWarning("Story progress was not loaded because no GlobalStoryStateSO is assigned.", this);
            return;
        }

        saveHandler ??= new JsonStorySaveHandler();
        string saveFilePath = GetSaveFilePath();
        if (!saveHandler.SaveExists(saveFilePath))
        {
            return;
        }

        StoryProgressSaveData saveData = saveHandler.Load(saveFilePath);
        storyState.ApplySaveData(saveData);
        lastStoryFlag = storyState.GetLastFlag();

        if (revealLoadedFlags)
        {
            RevealCurrentStoryFlag();
        }
    }

    public void DeleteSave()
    {
        saveHandler ??= new JsonStorySaveHandler();
        saveHandler.Delete(GetSaveFilePath());
    }

    public string GetSaveFilePath()
    {
        string normalizedFileName = GetSaveFileNameForFormat();
        if (Path.IsPathRooted(normalizedFileName))
        {
            return normalizedFileName;
        }

        return Path.Combine(Application.persistentDataPath, normalizedFileName);
    }

    private string GetSaveFileNameForFormat()
    {
        string normalizedFileName = string.IsNullOrWhiteSpace(saveFileName) ? "story-progress" : saveFileName;

        if (!normalizedFileName.EndsWith(".json", System.StringComparison.OrdinalIgnoreCase))
        {
            normalizedFileName = Path.ChangeExtension(normalizedFileName, ".json");
        }

        return normalizedFileName;
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

        if (storyState != null)
        {
            lastStoryFlag = storyState.GetLastFlag();
        }
    }
}
