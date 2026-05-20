using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Equipment", menuName = "Items/Equipment")]
public class EquipmentItemSO : BaseItemSO, IEquipableItem
{
    public override string ItemTypeDisplayName => "Equipment";

    public EquipmentSlot slot;

    [Header("Visual Prefab")]
    [Tooltip("Das 3D-Modell / Prefab, das in der Hand des Spielers platziert wird.")]
    public GameObject weaponPrefab;

    [Header("Stat Bonuses")]
    public int attackBonus;
    public int defenseBonus;
    public int agilityBonus;

    public void Equip(PlayerRuntimeState player)
    {
        player.attack += attackBonus;
        player.defense += defenseBonus;
        player.agility += agilityBonus;

        Debug.Log($"{itemName} equipped. Attack +{attackBonus}, Defense +{defenseBonus}, Agility +{agilityBonus}");
    }

    public void Unequip(PlayerRuntimeState player) { 
        player.attack -= attackBonus;
        player.defense -= defenseBonus;
        player.agility -= agilityBonus;
        Debug.Log($"{itemName} unequipped. Attack -{attackBonus}, Defense -{defenseBonus}, Agility -{agilityBonus}");
    }
}