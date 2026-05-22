using UnityEngine;

public enum SkillTargetType { Enemy, Self, AllEnemies }
public enum SkillEffectType { Damage, Heal, Buff, Debuff }

[CreateAssetMenu(fileName = "NewSkill", menuName = "Scriptable Objects/Battle/Skill Data")]
public class BattleSkillData : ScriptableObject
{
    [Header("Identity")]
    public string skillName = "Unnamed Skill";
    [TextArea(2, 4)]
    public string description;
    public Sprite icon;

    [Header("Cost")]
    public int mpCost = 0;

    [Header("Effect")]
    public SkillTargetType targetType = SkillTargetType.Enemy;
    public SkillEffectType effectType = SkillEffectType.Damage;

    [Tooltip("Base damage or heal amount")]
    public int basePower = 20;

    [Tooltip("Multiplier applied to the user's attack stat (0 = flat damage only)")]
    public float attackMultiplier = 1.2f;

    [Header("Animation")]
    [Tooltip("Animator trigger parameter name to set on the ATTACKER's Animator")]
    public string attackerAnimationTrigger = "Attack";

    [Tooltip("Animator trigger parameter name to set on the TARGET's Animator")]
    public string targetAnimationTrigger = "Hurt";

    [Header("AI Weight (Enemy only)")]
    [Tooltip("Relative chance this skill is chosen by an enemy vs normal attack (0 = never used)")]
    [Range(0f, 1f)]
    public float aiUseProbability = 0.3f;
}
