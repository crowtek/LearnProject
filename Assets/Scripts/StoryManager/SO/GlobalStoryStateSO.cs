using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GlobalStoryStateSO", menuName = "Scriptable Objects/Story/GlobalStoryStateSO")]
public class GlobalStoryStateSO : ScriptableObject
{
    [SerializeField] public List<string> completedFlags = new List<string>();
    private string lastAddedFlag;

    public void SetFlag(string flagName)
    {
        if (!completedFlags.Contains(flagName))
        {
            completedFlags.Add(flagName);
        }
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
}
