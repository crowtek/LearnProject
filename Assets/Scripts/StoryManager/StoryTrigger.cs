using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class StoryTrigger : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GlobalStoryStateSO storyState;
    [SerializeField] private StoryEventChannelSO storyEventChannel;
    [SerializeField] private BoolEventChannelSO toggleInputChannel;

    [Header("Flag to Set")]
    [SerializeField] private string flagToSet;
    [SerializeField] private bool triggerOnlyOnce = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (triggerOnlyOnce && hasTriggered) return;

            ExecuteTrigger();
        }
    }

    public void ExecuteTrigger()
    {
        if (string.IsNullOrEmpty(flagToSet)) return;

        storyState.SetFlag(flagToSet);

        if (storyEventChannel != null)
        {
            storyEventChannel.RaiseEvent(flagToSet);
        }

        hasTriggered = true;
        Debug.Log($"Story Trigger: Flag '{flagToSet}' wurde gesetzt!");

        if (triggerOnlyOnce)
        {
            this.enabled = false;
        }
    }

    public void StartStoryMoment()
    {
        toggleInputChannel.RaiseEvent(false);

        Debug.Log("Story-Moment aktiv: Input gesperrt.");
    }
}