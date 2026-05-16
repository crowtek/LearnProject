using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerInventory", menuName = "Systems/Inventory")]
public class InventorySO : ScriptableObject
{
    [System.Serializable]
    public class InventorySlot
    {
        public BaseItemSO item;
        public int amount;
    }

    [SerializeField] private EquipmentChangeChannelSO equipmentChannel;
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

            // Clean up: If the stack reaches zero, remove the slot entirely
            if (slot.amount <= 0)
            {
                slots.Remove(slot);
            }
        }
        else
        {
            Debug.LogWarning($"Attempted to remove {item.itemName}, but it wasn't found in inventory.");
        }
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
                ToggleState(oldItem, false);
            }

            ToggleState(equipment, true);
        }
        else
        {
            ToggleState(equipment, false);
        }
    }
    private void ToggleState(EquipmentItemSO item, bool state)
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
    }

}