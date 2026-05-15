using NUnit.Framework;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerRuntimeState", menuName = "Scriptable Objects/PlayerRuntimeState")]
public class PlayerRuntimeState : ScriptableObject
{
    [SerializeField] private EquipmentChangeChannelSO equipmentChannel;

    public int currentLevel = 1;
    public int currentEXP;
    public int expToNextLevel;
    public int currentHP;
    public int maxHP;
    public int attack;
    public int defense;
    public int agility;
    public bool isDead;
    public bool isPoisoned;

    private void OnEnable()
    {
        if (equipmentChannel != null)
        {
            equipmentChannel.OnEventRaised += UpdateStats;
        }
        else
        {
            Debug.LogWarning("OnEnable: EquipmentChannel is missing!");
        }
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

        // Detailed Debug Log
        Debug.Log($"<color=green>[Equipment Event]</color> {action} Item in slot: {change.slot}\n" +
                  $"Attack: {oldAtk} -> {attack} ({change.attackBonus * multiplier})\n" +
                  $"Defense: {oldDef} -> {defense} ({change.defenseBonus * multiplier})\n" +
                  $"Agility: {oldAgi} -> {agility} ({change.agilityBonus * multiplier})");
    }

    public void ResetStats(BattleEntityData template)
    {
        currentLevel = 1;
        currentEXP = 0;
        expToNextLevel = 100;
        maxHP = template.maxHP;
        attack = template.attack;
        defense = template.defense;
        agility = template.agility;
        currentHP = template.maxHP;
        isDead = false;

        expToNextLevel = LevelCalculator.GetRequiredEXP(currentLevel);
    }
}
