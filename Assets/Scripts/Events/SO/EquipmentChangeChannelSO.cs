using UnityEngine;

public enum EquipmentSlot { Weapon, Shield, Armor, Accessory }

public struct EquipmentChange
{
    public EquipmentSlot slot;
    public int attackBonus;
    public int defenseBonus;
    public int agilityBonus;
    public bool isEquipping;
}

[CreateAssetMenu(menuName = "Events/EquipmentChangeChannel")]
public class EquipmentChangeChannelSO : EventChannelSO<EquipmentChange> { }