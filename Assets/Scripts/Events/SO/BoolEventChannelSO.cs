using UnityEngine;

[CreateAssetMenu(fileName = "BoolEventChannelSO", menuName = "Scriptable Objects/Events/BoolEventChannelSO")]
public class BoolEventChannelSO : ScriptableObject
{
    public System.Action<bool> OnEventRaised;

    public void RaiseEvent(bool value)
    {
        OnEventRaised?.Invoke(value);
    }
}
