using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ConditionCheckedObject : MonoBehaviour
{
    [Header("Story flag Broacast Channel")]
    [SerializeField] private StringEventChannelSO StoryFlagBroadcast;

    [Header("Flag condition")]
    [SerializeField] private FlagCondition flagCondition;

    [FormerlySerializedAs("requiredFlag")]
    [StoryFlag][SerializeField] private string legacyRequiredFlag;

    [Header("Show or Hide this element when condition is met")]
    [SerializeField] private bool showWhenRight = true;

    private readonly HashSet<string> completedStoryFlags = new HashSet<string>();

    void OnEnable()
    {
        if(StoryFlagBroadcast != null)
        {
            StoryFlagBroadcast.OnEventRaised += HandleStoryFlagChanged;
        }
    }

    private void OnDisable()
    {
        if (StoryFlagBroadcast != null)
        {
            StoryFlagBroadcast.OnEventRaised -= HandleStoryFlagChanged;
        }
    }

    // Show or hide the element depending on the full flag condition.
    private void HandleStoryFlagChanged(string changedFlag)
    {
        if (string.IsNullOrEmpty(changedFlag))
        {
            return;
        }
        completedStoryFlags.Add(changedFlag);

        bool usesCompoundCondition = flagCondition.HasAnyFlag();
        if (usesCompoundCondition && !flagCondition.ReferencesFlag(changedFlag))
        {
            return;
        }

        if (!usesCompoundCondition && changedFlag != legacyRequiredFlag)
        {
            return;
        }

        bool conditionMet = usesCompoundCondition
            ? flagCondition.IsMet(completedStoryFlags)
            : completedStoryFlags.Contains(legacyRequiredFlag);

        gameObject.SetActive(conditionMet == showWhenRight);
    }
}