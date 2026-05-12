using UnityEngine;

public abstract class EventChannelSO<T> : ScriptableObject
{
    public System.Action<T> OnEventRaised;
    public void RaiseEvent(T value)
    {
        OnEventRaised?.Invoke(value);
    }
}

[CreateAssetMenu(fileName = "BoolEventChannelSO", menuName = "Scriptable Objects/Events/BoolEventChannelSO")]
public class BoolEventChannelSO : EventChannelSO<bool> { }