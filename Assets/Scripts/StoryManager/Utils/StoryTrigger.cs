using UnityEngine;

// Sets given Story flag on collider enter
[RequireComponent(typeof(BoxCollider))]
public class StoryTrigger : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private StringEventChannelSO setFlagRequestChannel;

    [Header("Flag to Set")]
    [StoryFlag] [SerializeField] private string flagToSet;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            ExecuteTrigger();
        }
    }

    public void ExecuteTrigger()
    {
        if (string.IsNullOrEmpty(flagToSet))
        {
            Debug.LogWarning($"[{gameObject.name}] StoryTrigger hat kein zugewiesenes Flag!");
            return;
        }

        if (setFlagRequestChannel == null)
        {
            Debug.LogError($"[{gameObject.name}] setFlagRequestChannel fehlt auf dem Trigger!");
            return;
        }

        hasTriggered = true;

        setFlagRequestChannel.RaiseEvent(flagToSet);
        Debug.Log($"[StoryTrigger] Flag '{flagToSet}' wurde in den Kanal gejagt.");
    }
}