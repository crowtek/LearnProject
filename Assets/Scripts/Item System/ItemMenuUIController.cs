using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class ItemMenuUIController : MonoBehaviour
{
    [Header("Data & Events")]
    [SerializeField] private InventorySO inventoryData;
    [SerializeField] private VoidEventChannelSO onInventoryChanged;

    [Header("UI Setup")]
    [SerializeField] private UIDocument inventoryDocument;
    [SerializeField] private VisualTreeAsset itemTemplate;
    [SerializeField] private InputActionReference inventoryToggleAction;

    private VisualElement root, container, itemListContainer, itemMenu;
    private Label itemDetailLabel;
    private Button equipButton;

    private bool isMenuOpen = false;
    private VisualElement currentlyFocusedItem;
    private InventorySlot currentlySelectedSlot;

    private void Awake()
    {
        root = inventoryDocument.rootVisualElement;
        container = root.Q<VisualElement>("Inventory");
        itemListContainer = root.Q<VisualElement>("ItemList");
        itemMenu = root.Q<VisualElement>("ItemMenu");
        itemDetailLabel = root.Q<Label>("ItemDetailText");
        equipButton = root.Q<Button>("EquipButton");

        root.Q<Button>("UseButton").clicked += () => UseSelectedItem();
        root.Q<Button>("DiscardButton").clicked += () => DiscardSelectedItem();
        root.Q<Button>("EquipButton").clicked += () => UseSelectedEQ();
    }

    private void OnEnable()
    {
        onInventoryChanged.OnEventRaised += RefreshUI;

        if (inventoryToggleAction?.action != null)
        {
            inventoryToggleAction.action.Enable();
            inventoryToggleAction.action.performed += OnTogglePerformed;
        }
    }

    private void OnDisable()
    {
        onInventoryChanged.OnEventRaised -= RefreshUI;
        if (inventoryToggleAction?.action != null)
        {
            inventoryToggleAction.action.performed -= OnTogglePerformed;
        }
    }
    private void OnTogglePerformed(InputAction.CallbackContext context)
    {
        ToggleMenu();
    }

    public void ToggleMenu()
    {
        isMenuOpen = !isMenuOpen;
        container.style.display = isMenuOpen ? DisplayStyle.Flex : DisplayStyle.None;

        if (isMenuOpen)
        {
            RefreshUI();
        }
    }

    private void RefreshUI()
    {
        if (!isMenuOpen) return;

        itemListContainer.Clear();

        foreach (var slot in inventoryData.slots)
        {
            CreateItemElement(slot);
        }
    }

    private void CreateItemElement(InventorySlot slot)
    {
        if (slot == null || slot.item == null) return;

        VisualElement templateRoot = itemTemplate.CloneTree();
        VisualElement itemBox = templateRoot[0];

        // Set item Icon
        VisualElement itemIcon = itemBox.Q<VisualElement>("ItemIcon");
        if (itemIcon != null && slot.item.icon != null)
        {
            itemIcon.style.backgroundImage = new StyleBackground(slot.item.icon);
        }

        // Set item counter
        Label countLabel = itemBox.Q<Label>("ItemCount");
        countLabel.text = slot.amount > 1 ? $"x{slot.amount}" : string.Empty;
        
        // If item = Equioment check if equiped
        Label equippedBadge = itemBox.Q<Label>("EQBadge");
        if (slot.item is EquipmentItemSO equipment && inventoryData.IsAlreadyEquipped(equipment))
        {
            equippedBadge.text = "E";
        }

        // Add click event
        itemBox.RegisterCallback<PointerDownEvent>(evt =>
        {
            if (evt.button == 0) // Left click
            {
                FocusItem(itemBox, slot);
                itemMenu.style.display = DisplayStyle.Flex;
            }
        });

        itemListContainer.Add(itemBox);
    }

    private void FocusItem(VisualElement element, InventorySlot slot)
    {
        currentlyFocusedItem?.RemoveFromClassList("item-focused");
        itemMenu.style.display = DisplayStyle.Flex; // Show Item menu

        currentlyFocusedItem = element;
        currentlySelectedSlot = slot;

        currentlyFocusedItem.AddToClassList("item-focused");
        currentlyFocusedItem.Focus();

        string detailText = $"<b>{slot.item.itemName}</b>\n\n{slot.item.description}";

        if (slot.item is EquipmentItemSO focusedEq)
        {
            equipButton.style.display = DisplayStyle.Flex; // Show equip button

            detailText += "\n";
            EquipmentItemSO currentEquipped = inventoryData.currentlyEquipped.Find(x => x.slot == focusedEq.slot);

            if (currentEquipped == focusedEq)
            {
                detailText += $"\nAttack: {focusedEq.attackBonus}";
                detailText += $"\nDefense: {focusedEq.defenseBonus}";
                detailText += $"\nAgility: {focusedEq.agilityBonus}";
            }
            else
            {
                // Compare stats (if nothing equipped, current stats are 0)
                int curAtk = currentEquipped != null ? currentEquipped.attackBonus : 0;
                int curDef = currentEquipped != null ? currentEquipped.defenseBonus : 0;
                int curAgi = currentEquipped != null ? currentEquipped.agilityBonus : 0;

                detailText += $"\n{FormatStatComparison("Attack", focusedEq.attackBonus, curAtk)}";
                detailText += $"\n{FormatStatComparison("Defense", focusedEq.defenseBonus, curDef)}";
                detailText += $"\n{FormatStatComparison("Agility", focusedEq.agilityBonus, curAgi)}";
            }

            detailText += $"\n\nSlot: <color=#FFFF55>{focusedEq.slot}</color>";
        }

        itemDetailLabel.text = detailText;
    }

    private string FormatStatComparison(string statName, int newValue, int oldValue)
    {
        int diff = newValue - oldValue;

        if (diff > 0) // Better
        {
            return $"<color=#55FF55>{statName}: {newValue} (+{diff})</color>";
        }
        else if (diff < 0) // Worse
        {
            return $"<color=#FF5555>{statName}: {newValue} ({diff})</color>";
        }
        else // Same
        {
            return $"{statName}: {newValue}";
        }
    }

    private void UseSelectedEQ()
    {
        if (currentlySelectedSlot?.item is EquipmentItemSO equipment)
        {
            inventoryData.RequestEquip(equipment);

            itemMenu.style.display = DisplayStyle.None;
        }
    }
    private void UseSelectedItem()
    {
        if (currentlySelectedSlot?.item != null)
        {
            inventoryData.UseItemFromInventory(currentlySelectedSlot.item);

            itemMenu.style.display = DisplayStyle.None;
            RefreshUI();
        }
    }

    private void DiscardSelectedItem()
    {
        if (currentlySelectedSlot != null)
        {
            inventoryData.RemoveItem(currentlySelectedSlot.item, 1);
            itemMenu.style.display = DisplayStyle.None;
        }
    }
}