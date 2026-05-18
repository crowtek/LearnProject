using UnityEngine;

[CreateAssetMenu(fileName = "SceneTransitionEventChannelSO", menuName = "Scriptable Objects/Events/SceneTransitionEventChannelSO")]
public class SceneTransitionEventChannelSO : ScriptableObject
{
    public System.Action<string, string> OnTransitionRequested; // SceneName, SpawnPointTag

    public void RaiseEvent(string sceneName, string spawnPointTag)
    {
        OnTransitionRequested?.Invoke(sceneName, spawnPointTag);
    }
}