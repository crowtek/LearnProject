using UnityEngine;

[CreateAssetMenu(fileName = "NewEntity", menuName = "Scriptable Objects/Battle/Entity Data")]
public class BattleEntityData : ScriptableObject
{
    public string entityName;
    public int maxHP;
    public int maxMP;
    public int attack;
    public int defense;
    public int agility;
    public int resilience;
    public int luck;
    public int stamina;
    public int wisdom;

    public int expReward;
    public Sprite portrait;
    public Sprite battleImage;

    [Header("3D Visuals")]
    public GameObject combatPrefab;

    [Header("Animation Triggers")]
    [Tooltip("Animator trigger for a normal attack")]
    public string normalAttackTrigger = "Attack";
    [Tooltip("Animator trigger when this entity takes damage")]
    public string hurtTrigger = "Hurt";
    [Tooltip("Animator trigger for the entity's death")]
    public string deathTrigger = "Die";

    [Header("Enemy Skills")]
    [Tooltip("Skills this enemy can use in battle. Leave empty for attack-only enemies.")]
    public BattleSkillData[] availableSkills;
}