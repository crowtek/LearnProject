using UnityEngine;
using UnityEngine.InputSystem;

public class ItemPickup : MonoBehaviour, IInteractable
{
    [Header("Item Settings")]
    [SerializeField] private BaseItemSO itemData; // The ScriptableObject for the item
    [SerializeField] private int amount = 1;

    [Header("Inventory Communication")]
    [SerializeField] private InventorySO inventory; // Reference to your Inventory ScriptableObject
    [SerializeField] private VoidEventChannelSO onInventoryChanged; // To notify UI to refresh

    [SerializeField] private InputActionReference interactAction;

    [Header("UI Placement")]
    [SerializeField] private Transform interactionPoint;

    public void Interact()
    {
        Debug.LogWarning($"Interacted with pickupitem");
        if (itemData == null || inventory == null)
        {
            Debug.LogWarning($"{gameObject.name}: ItemData or Inventory reference missing!");
            return;
        }

        // 1. Add item to the inventory system
        inventory.AddItem(itemData, amount);

        // 2. Notify the system that inventory has changed (for UI Refresh)
        if (onInventoryChanged != null)
        {
            onInventoryChanged.RaiseEvent();
        }

        // 3. Remove the object from the world
        Debug.Log($"Picked up {amount}x {itemData.itemName}");
        Destroy(gameObject);
    }

    public string GetInteractPrompt()
    {
        return $"Pick up {itemData?.itemName}";
    }

    public Transform GetInteractionPoint()
    {
        // Return the custom point if set, otherwise the item's own position
        return interactionPoint != null ? interactionPoint : transform;
    }
}