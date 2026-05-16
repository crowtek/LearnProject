using NUnit.Framework;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerRuntimeState", menuName = "Scriptable Objects/PlayerRuntimeState")]
public class PlayerRuntimeState : ScriptableObject
{
    [SerializeField] private PlayerRuntimeStateEventChannelSO playerStateChangedChannel;
    [SerializeField] private EquipmentChangeChannelSO equipmentChannel;

    public int currentLevel = 1;
    public int currentEXP;
    public int expToNextLevel;
    public int currentHP;
    public bool isDead;
    public bool isPoisoned;

    public int maxHP;
    public int maxMP;
    public int attack;
    public int defense;
    public int agility;
    public int resilience;
    public int luck;
    public int stamina;
    public int wisdom;

    private void OnEnable()
    {
        equipmentChannel.OnEventRaised += UpdateStats;
    }

    private void OnDisable()
    {
        if (equipmentChannel != null)equipmentChannel.OnEventRaised -= UpdateStats;
    }

    private void UpdateStats(EquipmentChange change)
    {
        int multiplier = change.isEquipping ? 1 : -1;
        string action = change.isEquipping ? "Equipped" : "Unequipped";

        // Track old stats for the log
        int oldAtk = attack;
        int oldDef = defense;
        int oldAgi = agility;

        // Apply changes
        attack += change.attackBonus * multiplier;
        defense += change.defenseBonus * multiplier;
        agility += change.agilityBonus * multiplier;

        RaiseStateChanged();

        // Detailed Debug Log
        Debug.Log($"<color=green>[Equipment Event]</color> {action} Item in slot: {change.slot}\n" +
                  $"Attack: {oldAtk} -> {attack} ({change.attackBonus * multiplier})\n" +
                  $"Defense: {oldDef} -> {defense} ({change.defenseBonus * multiplier})\n" +
                  $"Agility: {oldAgi} -> {agility} ({change.agilityBonus * multiplier})");
    }


    public void RaiseStateChanged()
    {
        if (playerStateChangedChannel != null)
        {
            playerStateChangedChannel.RaiseEvent(this);
        }
    }
}
