using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GlobalStoryStateSO", menuName = "Scriptable Objects/Story/GlobalStoryStateSO")]
public class GlobalStoryStateSO : ScriptableObject
{
    [SerializeField] public List<string> completedFlags = new List<string>();
    private string lastAddedFlag;

    public bool SetFlag(string flagName)
    {
        if (string.IsNullOrEmpty(flagName) || completedFlags.Contains(flagName))
        {
            return false;
        }

        completedFlags.Add(flagName);
        lastAddedFlag = flagName;
        return true;
    }

    public bool IsFlagCompleted(string flagName)
    {
        return completedFlags.Contains(flagName);
    }

    public string GetLastFlag()
    {
        if(completedFlags.Count > 0)
        {
            lastAddedFlag = completedFlags[^1];
        }

        return lastAddedFlag;
    }

    public StoryProgressSaveData CreateSaveData()
    {
        return new StoryProgressSaveData
        {
            completedFlags = new List<string>(completedFlags),
            lastFlag = GetLastFlag()
        };
    }

    public void ApplySaveData(StoryProgressSaveData saveData)
    {
        completedFlags.Clear();
        lastAddedFlag = null;

        if (saveData?.completedFlags == null)
        {
            return;
        }

        foreach (string flag in saveData.completedFlags)
        {
            SetFlag(flag);
        }

        if (!string.IsNullOrEmpty(saveData.lastFlag) && completedFlags.Contains(saveData.lastFlag))
        {
            lastAddedFlag = saveData.lastFlag;
        }
    }

    public void Clear()
    {
        completedFlags.Clear();
        lastAddedFlag = null;
    }
}
