using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Events/VoidEventChannelSO")]
public class VoidEventChannelSO : ScriptableObject
{
    public System.Action OnEventRaised;
    public void RaiseEvent() => OnEventRaised?.Invoke();
}