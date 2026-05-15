using UnityEngine;

public enum EquipmentSlot { Weapon, Shield, Armor, Accessory }

[CreateAssetMenu(fileName = "New Equipment", menuName = "Items/Equipment")]
public class EquipmentItemSO : BaseItemSO
{
    public override string ItemTypeDisplayName => "Equipment";

    public EquipmentSlot slot;

    [Header("Stat Bonuses")]
    public int attackBonus;
    public int defenseBonus;
    public int agilityBonus;
}