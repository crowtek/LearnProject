using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    public BaseItemSO item;
    public int amount;
}


[CreateAssetMenu(fileName = "PlayerInventory", menuName = "Systems/Inventory")]
public class InventorySO : ScriptableObject
{
    [SerializeField] private EquipmentChangeChannelSO equipmentChannel;
    [SerializeField] private VoidEventChannelSO onInventoryChanged;
    [SerializeField] private PlayerRuntimeState playerState;

    public List<InventorySlot> slots = new List<InventorySlot>();
    public List<EquipmentItemSO> currentlyEquipped = new List<EquipmentItemSO>();

    public void AddItem(BaseItemSO newItem, int amount = 1)
    {
        var slot = slots.Find(s => s.item == newItem);

        if (slot != null && !(newItem is EquipmentItemSO)) // Equipment doesn't stack
        {
            slot.amount += amount;
        }
        else
        {
            slots.Add(new InventorySlot { item = newItem, amount = amount });
        }

        onInventoryChanged.RaiseEvent(); // Tell system Inventory changed
    }

    public bool HasItem(BaseItemSO item, int amount = 1)
    {
        var slot = slots.Find(s => s.item == item);
        return slot != null && slot.amount >= amount;
    }

    public void RemoveItem(BaseItemSO item, int amount = 1)
    {
        var slot = slots.Find(s => s.item == item);

        if (slot != null)
        {
            slot.amount -= amount;
            if (slot.amount <= 0)
            {
                slots.Remove(slot);// If the stack reaches zero, remove the slot entirely
            }
        }
        else
        {
            Debug.LogWarning($"Attempted to remove {item.itemName}, but it wasn't found in inventory.");
        }

        onInventoryChanged.RaiseEvent(); // Tell system Inventory changed
    }

    public bool IsAlreadyEquipped(EquipmentItemSO item)
    {
        return currentlyEquipped.Contains(item);
    }

    public void RequestEquip(EquipmentItemSO equipment)
    {
        bool isEquipping = !IsAlreadyEquipped(equipment);

        if (isEquipping)
        {
            // Logic for "Unique" slots: Unequip item already in that slot
            EquipmentItemSO oldItem = currentlyEquipped.Find(x => x.slot == equipment.slot);
            if (oldItem != null)
            {
                ToggleEquipmentState(oldItem, false);
            }

            ToggleEquipmentState(equipment, true);
        }
        else
        {
            ToggleEquipmentState(equipment, false);
        }
    }
    private void ToggleEquipmentState(EquipmentItemSO item, bool state)
    {
        if (state)
        {
            currentlyEquipped.Add(item);
        }
        else
        {
            currentlyEquipped.Remove(item);
        }

        equipmentChannel.RaiseEvent(new EquipmentChange
        {
            slot = item.slot,
            attackBonus = item.attackBonus,
            defenseBonus = item.defenseBonus,
            agilityBonus = item.agilityBonus,
            isEquipping = state
        });

        onInventoryChanged.RaiseEvent();
    }

    public void UseItemFromInventory(BaseItemSO item)
    {
        if (item is IUsableItem usable)
        {
            if (usable.Use(playerState))
            {
                RemoveItem(item, 1);
                onInventoryChanged?.RaiseEvent();
            }
        }
    }

}