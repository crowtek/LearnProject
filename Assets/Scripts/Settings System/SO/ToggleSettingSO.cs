using UnityEngine;

[CreateAssetMenu(fileName = "New Toggle Setting", menuName = "Settings/Toggle Option")]
public class ToggleSettingSO : GameSettingSO
{
    [SerializeField] private bool defaultValue;
    private bool currentValue;

    public bool CurrentValue
    {
        get => currentValue;
        set
        {
            currentValue = value;
            ApplySetting();
        }
    }

    private void OnEnable() => currentValue = PlayerPrefs.GetInt(name, defaultValue ? 1 : 0) == 1;

    public override void ApplySetting()
    {
        Screen.fullScreen = currentValue;
        PlayerPrefs.SetInt(name, currentValue ? 1 : 0);
        PlayerPrefs.Save();
    }

    public override System.Type GetValueType() => typeof(bool);
}