using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerRuntimeState", menuName = "Scriptable Objects/PlayerRuntimeState")]
public class PlayerRuntimeState : ScriptableObject
{
    [SerializeField] private PlayerRuntimeStateEventChannelSO playerStateChangedChannel;
    [SerializeField] private EquipmentChangeChannelSO equipmentChannel;

    [Header("Skill System Configuration")]
    public int unspentSkillPoints;

    [Header("3D Visuals")]
    public GameObject playerCombatPrefab;

    [Header("Animation Triggers")]
    [Tooltip("Animator trigger for normal attack")]
    public string normalAttackTrigger = "Attack";
    [Tooltip("Animator trigger when taking damage")]
    public string hurtTrigger = "Hurt";
    [Tooltip("Animator trigger for death")]
    public string deathTrigger = "Die";

    [SerializeField] private List<string> allocatedWeaponNames = new List<string>();
    [SerializeField] private List<int> allocatedWeaponPoints = new List<int>();
    private Dictionary<string, int> weaponPointsMap = new Dictionary<string, int>();

    // Tracks which skill nodes have already been applied to prevent stacking on re-calculation
    private HashSet<string> appliedSkillNodeIds = new HashSet<string>();

    [Header("Identity")]
    public string playerName = "Hero";
    public int currentLevel = 1;
    public int currentEXP;
    public int expToNextLevel;
    public int currentHP;
    public bool isDead;
    public bool isPoisoned;

    [Header("Base Stats")]
    public int maxHP;
    public int maxMP;
    public int currentMP;
    public int attack;
    public int defense;
    public int agility;
    public int resilience;
    public int luck;
    public int stamina;
    public int wisdom;

    [Header("Legacy Weapon Points (kept for compatibility)")]
    public int swordPoints;
    public int spearPoints;
    public int boomerangPoints;
    public int fisticuffsPoints;

    [Header("Known Battle Skills")]
    [Tooltip("Skills available to the player in battle. Populated by the skill tree unlock system.")]
    public List<BattleSkillData> knownSkills = new List<BattleSkillData>();



    private void OnEnable()
    {
        weaponPointsMap.Clear();
        for (int i = 0; i < allocatedWeaponNames.Count; i++)
        {
            weaponPointsMap[allocatedWeaponNames[i]] = allocatedWeaponPoints[i];
        }

        if (equipmentChannel != null)
            equipmentChannel.OnEventRaised += UpdateStats;
    }

    private void OnDisable()
    {
        if (equipmentChannel != null)
            equipmentChannel.OnEventRaised -= UpdateStats;
    }

    private void UpdateStats(EquipmentChange change)
    {
        int multiplier = change.isEquipping ? 1 : -1;
        attack += change.attackBonus * multiplier;
        defense += change.defenseBonus * multiplier;
        agility += change.agilityBonus * multiplier;
        RaiseStateChanged();
    }

    public void RaiseStateChanged()
    {
        if (playerStateChangedChannel != null)
            playerStateChangedChannel.RaiseEvent(this);
    }

    public void SetPointsForWeapon(string weaponName, int points)
    {
        weaponPointsMap[weaponName] = points;
        int idx = allocatedWeaponNames.IndexOf(weaponName);
        if (idx >= 0)
            allocatedWeaponPoints[idx] = points;
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
        appliedSkillNodeIds.Clear();
    }

    public void RecalculateWeaponSkillBonuses(List<WeaponCategorySO> categories)
    {
        foreach (var category in categories)
        {
            int allocatedPoints = GetPointsForWeapon(category.weaponName);

            foreach (var node in category.skillNodes)
            {
                // Build a unique ID from category + node name
                string nodeId = $"{category.weaponName}::{node.skillName}";

                if (allocatedPoints >= node.pointsRequired && !appliedSkillNodeIds.Contains(nodeId))
                {
                    appliedSkillNodeIds.Add(nodeId);

                    if (node.nodeType == SkillNodeType.StatBonus)
                    {
                        attack += node.attackBonus;
                        defense += node.defenseBonus;
                        Debug.Log($"[Progression] Applied +{node.attackBonus} ATK / +{node.defenseBonus} DEF from '{nodeId}'");
                    }
                    else if (node.nodeType == SkillNodeType.ActiveSkill)
                    {
                        UnlockActiveSkill(node.skillToUnlock);
                    }
                }
            }
        }

        RaiseStateChanged();
    }

    private void UnlockActiveSkill(BattleSkillData skill)
    {
        if (skill == null)
        {
            Debug.LogWarning("[Progression] An ActiveSkill node has no skill assigned — check your WeaponCategorySO.");
            return;
        }

        if (knownSkills.Contains(skill))
        {
            Debug.Log($"[Progression] '{skill.skillName}' already known — skipped.");
            return;
        }

        knownSkills.Add(skill);
        Debug.Log($"[Progression] Unlocked: '{skill.skillName}'");
    }
}