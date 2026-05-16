using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Settings Category", menuName = "Settings/Category Hub")]
public class SettingsCategorySO : ScriptableObject
{
    public string categoryName;
    public List<GameSettingSO> settingsInGroup = new List<GameSettingSO>();

    public void ApplyAllInGroup()
    {
        foreach (var setting in settingsInGroup)
        {
            setting.ApplySetting();
        }
    }
}