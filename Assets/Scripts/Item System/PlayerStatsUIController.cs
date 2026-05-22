using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerStatsUIController : MonoBehaviour
{

    [SerializeField] private UIDocument playerStatsUI;
    [SerializeField] private InputActionReference toggleStatsUIAction;
    [SerializeField] private PlayerRuntimeState playerRuntimeState;
    [SerializeField] private InventorySO inventoryData;

    private VisualElement root;
    private VisualElement container;

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

    // Equipment List
    private VisualElement equipmentListContainer;
    private Label wapponSlotValue;
    private Label armorSlotValue;
    private Label shieldSlotValue;
    private Label accessorySlotValue;

    // Player info
    private VisualElement generalDataContainer;
    private Label level;
    private Label exp;
    private Label neededExp;
    private Label playerName;


    private bool isMenuOpen = false;

    private void Awake()
    {
        root = playerStatsUI.rootVisualElement;
        container = root.Q<VisualElement>("PlayerStatsContainer");

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

        // Equipment List
        equipmentListContainer = root.Q<VisualElement>("EquipmentDataContainer");
        wapponSlotValue = equipmentListContainer.Q<Label>("WeaponSlotValue");
        armorSlotValue = equipmentListContainer.Q<Label>("ArmorSlotValue");
        shieldSlotValue = equipmentListContainer.Q<Label>("ShieldSlotValue");
        accessorySlotValue = equipmentListContainer.Q<Label>("AccessorySlotValue");

        // Player info 
        generalDataContainer = root.Q<VisualElement>("GeneralDataContainer");
        level = generalDataContainer.Q<Label>("LevelValue");
        exp = generalDataContainer.Q<Label>("ExpValue");
        neededExp = generalDataContainer.Q<Label>("NeededExpValue");
        playerName = generalDataContainer.Q<Label>("PlayerNameValue");

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
            PopulateEquipmentList();
            RefreshUI();
            container.style.display = DisplayStyle.Flex;
        }
        else
        {
            container.style.display = DisplayStyle.None;
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

        level.text = playerRuntimeState.currentLevel.ToString();
        exp.text = playerRuntimeState.currentEXP.ToString();
        neededExp.text = playerRuntimeState.expToNextLevel.ToString();
        playerName.text = playerRuntimeState.playerName;
    }

    private void PopulateEquipmentList()
    {
        if (inventoryData == null) return;

        wapponSlotValue.text = "None";
        shieldSlotValue.text = "None";
        armorSlotValue.text = "None";
        accessorySlotValue.text = "None";

        Debug.Log("UI Opened! Updating fixed equipment slots layout...");

        foreach (EquipmentItemSO equippedItem in inventoryData.currentlyEquipped)
        {
            if (equippedItem == null) continue;

            switch (equippedItem.slot)
            {
                case EquipmentSlot.Weapon:
                    wapponSlotValue.text = equippedItem.itemName;
                    break;
                case EquipmentSlot.Shield:
                    shieldSlotValue.text = equippedItem.itemName;
                    break;
                case EquipmentSlot.Armor:
                    armorSlotValue.text = equippedItem.itemName;
                    break;
                case EquipmentSlot.Accessory:
                    accessorySlotValue.text = equippedItem.itemName;
                    break;
            }
        }
    }
}
