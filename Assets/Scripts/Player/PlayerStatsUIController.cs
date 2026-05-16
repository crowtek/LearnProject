using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerStatsUIController : MonoBehaviour
{

    [SerializeField] private UIDocument playerStatsUI;
    [SerializeField] private InputActionReference toggleStatsUIAction;
    [SerializeField] private PlayerRuntimeState playerRuntimeState;

    private VisualElement root;

    // Stats
    private VisualElement StatsContainer;
    private Label maxHPValue;
    private Label maxMPValue;
    private Label attackValue;
    private Label defenseValue;
    private Label agilityValue;
    private Label resilienceValue;
    private Label luckValue;
    private Label staminaValue;
    private Label wisdomValue;

    private bool isMenuOpen = false;

    private void Awake()
    {
        root = playerStatsUI.rootVisualElement;

        // Stats
        StatsContainer = root.Q<VisualElement>("StatsContainer");
        maxHPValue = StatsContainer.Q<Label>("MaxHPValue");
        maxMPValue = StatsContainer.Q<Label>("MaxMPValue");
        attackValue = StatsContainer.Q<Label>("AttackValue");
        defenseValue = StatsContainer.Q<Label>("DefenseValue");
        agilityValue = StatsContainer.Q<Label>("AgilityValue");
        resilienceValue = StatsContainer.Q<Label>("ResilienceValue");
        luckValue = StatsContainer.Q<Label>("LuckValue");
        staminaValue = StatsContainer.Q<Label>("StaminaValue");
        wisdomValue = StatsContainer.Q<Label>("WisdomValue");

        root.style.display = DisplayStyle.None;
        isMenuOpen = false;

        if (toggleStatsUIAction != null) Debug.Log("toggleStatsUIAction missing in Playerstats Ui controller");
       }
        

    private void OnEnable()
    {
            toggleStatsUIAction.action.Enable();
            toggleStatsUIAction.action.performed += ToggleStatsUI;
    }

    private void OnDisable()
    {
        toggleStatsUIAction.action.Disable();
        toggleStatsUIAction.action.performed -= ToggleStatsUI; 
    }

    private void ToggleStatsUI(InputAction.CallbackContext context)
    {
        isMenuOpen = !isMenuOpen;

        if (isMenuOpen)
        {
            RefreshUI();
            root.style.display = DisplayStyle.Flex;
        }
        else
        {
            root.style.display = DisplayStyle.None;
        }
    }

    private void RefreshUI()
    {
        maxHPValue.text = playerRuntimeState.maxHP.ToString();
        maxMPValue.text = playerRuntimeState.maxMP.ToString();
        attackValue.text = playerRuntimeState.attack.ToString();
        defenseValue.text = playerRuntimeState.defense.ToString();
        agilityValue.text = playerRuntimeState.agility.ToString();
        resilienceValue.text = playerRuntimeState.resilience.ToString(); ;
        luckValue.text = playerRuntimeState.luck.ToString(); ;
        staminaValue.text = playerRuntimeState.stamina.ToString(); ;
        wisdomValue.text = playerRuntimeState.wisdom.ToString();
    }
}
