using System;
using UnityEngine;

public class ConditionCheckedObject : MonoBehaviour
{
    [SerializeField] private GlobalStoryStateSO storyState;
    [SerializeField] private StoryEventChannelSO storyEventChannel;
    [StoryFlag] [SerializeField] private string requiredFlag;
    [SerializeField] private bool hideIfFlagCompleted = false;

    void OnEnable()
    {
        CheckCondition();

        if(storyEventChannel != null)
        {
            storyEventChannel.OnEventRaised += HandleStoryFlagChanged;
        }
    }

    private void OnDisable()
    {
        if (storyEventChannel != null)
        {
            storyEventChannel.OnEventRaised -= HandleStoryFlagChanged;
        }
    }

    public void CheckCondition()
    {
        bool isCompleted = storyState.IsFlagCompleted(requiredFlag);

        if (hideIfFlagCompleted)
            gameObject.SetActive(!isCompleted);
        else
            gameObject.SetActive(isCompleted);
    }

    private void HandleStoryFlagChanged(string changedFlag)
    {
        if (changedFlag == requiredFlag)
        {
            CheckCondition();
        }
    }
}