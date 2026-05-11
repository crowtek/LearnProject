using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GlobalStoryStateSO", menuName = "Scriptable Objects/Story/GlobalStoryStateSO")]
public class GlobalStoryStateSO : ScriptableObject
{
    [SerializeField] private List<string> completedFlags = new List<string>();

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

    // Für Quests Dictionary oder Enums nutzen
}
