using System.Collections.Generic;
using UnityEngine;

public enum SkillNodeType { StatBonus, ActiveSkill }

[System.Serializable]
public class WeaponSkillNode
{
    public string skillName;
    [TextArea(2, 4)] public string description;
    public int pointsRequired;
    public SkillNodeType nodeType;

    [Header("Stat Bonus Settings")]
    public int attackBonus;
    public int defenseBonus;

    [Header("Active Skill Settings")]
    [Tooltip("The skill that gets added to the player's battle skill list when this node is reached.")]
    public BattleSkillData skillToUnlock;
}

[CreateAssetMenu(fileName = "NewWeaponCategory", menuName = "Scriptable Objects/Skills/Weapon Category")]
public class WeaponCategorySO : ScriptableObject
{
    public string weaponName;
    public Sprite weaponIcon;
    [TextArea(3, 5)] public string categoryDescription;

    [Header("Skill Progression Path")]
    public List<WeaponSkillNode> skillNodes = new List<WeaponSkillNode>();
}