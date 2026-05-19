using UnityEngine;

public class ConditionCheckedObject : MonoBehaviour
{
    [Header("Story flag Broacast Channel")]
    [SerializeField] private StringEventChannelSO StoryFlagBroadcast;

    [Header("Flag to set on enter")]
    [StoryFlag] [SerializeField] private string requiredFlag;

    [Header("Show or Hide this element when flag is set")]
    [SerializeField] private bool showWhenRight = true;

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

    // Show or hide the element depanding on right flag
    private void HandleStoryFlagChanged(string changedFlag)
    {
        if (changedFlag != requiredFlag) return;

        gameObject.SetActive(showWhenRight);
    }
}