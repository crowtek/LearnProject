using Codice.CM.Common;
using NUnit.Framework;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerRuntimeState", menuName = "Scriptable Objects/PlayerRuntimeState")]
public class PlayerRuntimeState : ScriptableObject
{
    [SerializeField] private PlayerRuntimeStateEventChannelSO playerStateChangedChannel;
    [SerializeField] private EquipmentChangeChannelSO equipmentChannel;

    [Header("Skill System Configuration")]
    public int unspentSkillPoints;

    [SerializeField] private List<string> allocatedWeaponNames = new List<string>();
    [SerializeField] private List<int> allocatedWeaponPoints = new List<int>();
    private Dictionary<string, int> weaponPointsMap = new Dictionary<string, int>();

    public string playerName = "Hero";
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

    [Header("Skill System")]
    public int swordPoints;
    public int spearPoints;
    public int boomerangPoints;
    public int fisticuffsPoints;

    // Instead of adding directly to raw attack, separate Base Stats from Skill Bonus Stats

    private void OnEnable()
    {
        // Rebuild runtime dictionary from serialized lists
        weaponPointsMap.Clear();
        for (int i = 0; i < allocatedWeaponNames.Count; i++)
        {
            weaponPointsMap[allocatedWeaponNames[i]] = allocatedWeaponPoints[i];
        }
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
    }


    public void RaiseStateChanged()
    {
        if (playerStateChangedChannel != null)
        {
            playerStateChangedChannel.RaiseEvent(this);
        }
    }

    public void SetPointsForWeapon(string weaponName, int points)
    {
        weaponPointsMap[weaponName] = points;

        // Sync back to lists for serialization/inspector visibility
        int idx = allocatedWeaponNames.IndexOf(weaponName);
        if (idx >= 0)
        {
            allocatedWeaponPoints[idx] = points;
        }
        else
        {
            allocatedWeaponNames.Add(weaponName);
            allocatedWeaponPoints.Add(points);
        }
    }
    public int GetPointsForWeapon(string weaponName)
    {
        if (weaponPointsMap.TryGetValue(weaponName, out int points)) return points;
        return 0;
    }

    public void ResetSkillPoints()
    {
        unspentSkillPoints = 0;
        weaponPointsMap.Clear();
        allocatedWeaponNames.Clear();
        allocatedWeaponPoints.Clear();
    }

    public void RecalculateWeaponSkillBonuses(List<WeaponCategorySO> categories)
    {
        foreach (var category in categories)
        {
            int allocatedPoints = GetPointsForWeapon(category.weaponName);

            foreach (var node in category.skillNodes)
            {
                // If the player has enough points, grant the bonus!
                if (allocatedPoints >= node.pointsRequired)
                {
                    if (node.nodeType == SkillNodeType.StatBonus)
                    {
                        this.attack += node.attackBonus;
                        this.defense += node.defenseBonus;

                        Debug.Log($"Permanently added +{node.attackBonus} Atk from {category.weaponName} path!");
                    }
                    else if (node.nodeType == SkillNodeType.ActiveSkill)
                    {
                        // Handle adding the skill to your player's known spells/skills list
                        UnlockActiveSkill(node.skillIdToUnlock);
                    }
                }
            }
        }

        // Notify any active UIs that stats changed
        RaiseStateChanged();
    }

    private void UnlockActiveSkill(string skillId)
    {
        Debug.Log($"Player unlocked active skill: {skillId}");
    }
}
