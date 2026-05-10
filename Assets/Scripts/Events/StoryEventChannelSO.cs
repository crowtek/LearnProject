using UnityEngine;
using System;

[CreateAssetMenu(fileName = "StoryEventChannelSO", menuName = "Scriptable Objects/StoryEventChannelSO")]
public class StoryEventChannelSO : ScriptableObject
{
    public Action<string> OnStoryFlagChange;

    public void RaiseEvent(string FlagName)
    {
        OnStoryFlagChange?.Invoke(FlagName);
    }
}