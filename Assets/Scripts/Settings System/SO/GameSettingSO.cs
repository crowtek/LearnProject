using UnityEngine;

public abstract class GameSettingSO : ScriptableObject
{
    [Header("Base Settings Info")]
    public string settingName;
    [TextArea] public string description;

    // Force every child setting type to implement its own application logic
    public abstract void ApplySetting();

    // Abstract hook so the UI knows how to build the visuals
    public abstract System.Type GetValueType();
}