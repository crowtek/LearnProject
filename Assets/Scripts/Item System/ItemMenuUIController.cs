using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class ItemMenuUIController : MonoBehaviour
{
    [Header("Data & Events")]
    [SerializeField] private InventorySO inventoryData;
    [SerializeField] private VoidEventChannelSO onInventoryChanged;
    [SerializeField] private PlayerRuntimeState playerState; // For using items that affect player stats

    [Header("UI Setup")]
    [SerializeField] private UIDocument inventoryDocument;
    [SerializeField] private InputActionReference inventoryToggleAction;

    private VisualElement root;
    private VisualElement itemListContainer;
    private VisualElement itemMenu;
    private Label itemDetailLabel;

    private bool isMenuOpen = false;
    private VisualElement currentlyFocusedItem;
    private InventorySO.InventorySlot currentlySelectedSlot;

    private void Awake()
    {
        root = inventoryDocument.rootVisualElement;
        itemListContainer = root.Q<VisualElement>("ItemList");
        itemMenu = root.Q<VisualElement>("ItemMenu");
        itemDetailLabel = root.Q<Label>("ItemDetailText");

        root.style.display = DisplayStyle.None;
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
        root.style.display = isMenuOpen ? DisplayStyle.Flex : DisplayStyle.None;

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

    private void CreateItemElement(InventorySO.InventorySlot slot)
    {
        // Create a new VisualElement (the "box")
        VisualElement itemBox = new VisualElement();
        itemBox.AddToClassList("item");

        // Create the Icon
        VisualElement icon = new VisualElement();
        icon.style.backgroundImage = new StyleBackground(slot.item.icon);
        icon.style.width = Length.Percent(100);
        icon.style.height = Length.Percent(80);
        itemBox.Add(icon);

        if (slot.item is EquipmentItemSO equipment && inventoryData.IsAlreadyEquipped(equipment))
        {
            Label equippedBadge = new Label("E");

            // Styling the "E" to appear in a corner (e.g., top-left)
            equippedBadge.style.position = Position.Absolute;
            equippedBadge.style.top = 2;
            equippedBadge.style.left = 5;
            equippedBadge.style.color = Color.yellow;
            equippedBadge.style.unityFontStyleAndWeight = FontStyle.Bold;
            equippedBadge.style.fontSize = 12;

            itemBox.Add(equippedBadge);
        }

        Label itemLabel = new Label($" x{slot.amount}");
        itemLabel.style.fontSize = 10;
        itemLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        itemLabel.style.whiteSpace = WhiteSpace.Normal;
        itemBox.Add(itemLabel);

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

    private void FocusItem(VisualElement element, InventorySO.InventorySlot slot)
    {
        currentlyFocusedItem?.RemoveFromClassList("item-focused");

        currentlyFocusedItem = element;
        currentlySelectedSlot = slot;

        currentlyFocusedItem.AddToClassList("item-focused");
        currentlyFocusedItem.Focus();

        itemDetailLabel.text = $"{slot.item.itemName}\n\n{slot.item.description}";
    }

    private void UseSelectedEQ()
    {
        if (currentlySelectedSlot?.item is EquipmentItemSO equipment)
        {
            inventoryData.RequestEquip(equipment);

            itemMenu.style.display = DisplayStyle.None;
            RefreshUI();
        }
    }
    private void UseSelectedItem()
    {
        if (currentlySelectedSlot?.item is IUsableItem usable)
        {
            if (usable.Use(playerState))
            {
                inventoryData.RemoveItem(currentlySelectedSlot.item, 1);
                itemMenu.style.display = DisplayStyle.None;
            }
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