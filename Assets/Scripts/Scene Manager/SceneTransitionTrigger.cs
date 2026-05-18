using UnityEngine;

public class SceneTransitionTrigger : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private string targetSceneName;
    [SerializeField] private string targetSpawnPointTag;

    [Header("Channels")]
    [SerializeField] private SceneTransitionEventChannelSO transitionChannel;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (transitionChannel != null)
            {
                transitionChannel.RaiseEvent(targetSceneName, targetSpawnPointTag);
            }
            else
            {
                Debug.LogWarning($"Transition Channel auf {gameObject.name} fehlt!");
            }
        }
    }
}