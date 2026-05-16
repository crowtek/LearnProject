using UnityEngine;
using UnityEngine.InputSystem;

public class ItemPickup : MonoBehaviour, IInteractable
{
    [Header("Item Settings")]
    [SerializeField] private BaseItemSO itemData;
    [SerializeField] private int amount = 1;

    [Header("Inventory Communication")]
    [SerializeField] private InventorySO inventory;
    [SerializeField] private InputActionReference interactAction;

    [Header("UI Placement")]
    [SerializeField] private Transform interactionPoint;

    public void Interact()
    {
        if (itemData == null || inventory == null) return;

        inventory.AddItem(itemData, amount);
        Destroy(gameObject);
    }

    public string GetInteractPrompt()
    {
        return $"Pick up {itemData?.itemName}";
    }

    public Transform GetInteractionPoint()
    {
        return interactionPoint != null ? interactionPoint : transform;
    }
}