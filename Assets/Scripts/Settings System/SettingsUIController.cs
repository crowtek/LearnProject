using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class SettingsUIController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private SettingsCategorySO targetCategory;
    [SerializeField] private InputActionReference settingsToggleAction;

    private VisualElement root;
    private VisualElement container;
    private bool isMenuOpen = false;

    private void Awake()
    {
        root = uiDocument.rootVisualElement;
        container = root.Q<VisualElement>("SettingsContainer");

        root.style.display = DisplayStyle.None;
        isMenuOpen = false;
    }

    private void OnEnable()
    {
        if(settingsToggleAction != null)
        {
            settingsToggleAction.action.Enable();
            settingsToggleAction.action.performed += ToggleSettingsMenu;
        }

        BuildSettingsMenu();
    }

    private void OnDisable()
    {
        settingsToggleAction.action.Disable();
        settingsToggleAction.action.performed -= ToggleSettingsMenu;
    }

    private void ToggleSettingsMenu(InputAction.CallbackContext context)
    {
        isMenuOpen = !isMenuOpen;

        if (isMenuOpen)
        {
            root.style.display = DisplayStyle.Flex;
        }
        else
        {
            root.style.display = DisplayStyle.None;
        }
    }

    private void BuildSettingsMenu()
    {
        container.Clear();

        foreach (GameSettingSO setting in targetCategory.settingsInGroup)
        {
            if (setting.GetValueType() == typeof(bool))
            {
                var toggleSetting = (ToggleSettingSO)setting;

                Toggle uiToggle = new Toggle(setting.settingName);
                uiToggle.value = toggleSetting.CurrentValue;
                uiToggle.RegisterValueChangedCallback(evt => toggleSetting.CurrentValue = evt.newValue);

                container.Add(uiToggle);
            }
            else if (setting.GetValueType() == typeof(int))
            {
                var selectSetting = (SelectionSettingSO)setting;

                DropdownField uiDropdown = new DropdownField(setting.settingName, selectSetting.choices, selectSetting.CurrentIndex);
                uiDropdown.RegisterValueChangedCallback(evt => selectSetting.CurrentIndex = uiDropdown.index);

                container.Add(uiDropdown);
            }
        }
    }
}