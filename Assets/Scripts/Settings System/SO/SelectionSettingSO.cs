using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Selection Setting", menuName = "Settings/Selection Option")]
public class SelectionSettingSO : GameSettingSO
{
    [Header("Selection Parameters")]
    public List<string> choices = new List<string>(); // ["Low", "Medium", "High"]
    [SerializeField] private int defaultIndex;

    private int currentIndex;
    public int CurrentIndex
    {
        get => currentIndex;
        set
        {
            currentIndex = Mathf.Clamp(value, 0, choices.Count - 1);
            ApplySetting();
        }
    }

    private void OnEnable() => currentIndex = PlayerPrefs.GetInt(name, defaultIndex);

    public override void ApplySetting()
    {
        Debug.Log($"Applied {settingName}: {choices[currentIndex]}");

        PlayerPrefs.SetInt(name, currentIndex);
        PlayerPrefs.Save();
    }

    public override System.Type GetValueType() => typeof(int);
}