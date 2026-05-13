using UnityEngine;

[CreateAssetMenu(fileName = "PlayerRuntimeState", menuName = "Scriptable Objects/PlayerRuntimeState")]
public class PlayerRuntimeState : ScriptableObject
{
    public int currentLevel = 1;
    public int currentEXP;
    public int expToNextLevel;
    public int currentHP;
    public int maxHP;
    public int attack;
    public bool isDead;

    public void ResetStats(BattleEntityData template)
    {
        currentLevel = 1;
        currentEXP = 0;
        maxHP = template.maxHP;
        attack = template.attack;
        currentHP = maxHP;
        isDead = false;

        expToNextLevel = LevelCalculator.GetRequiredEXP(currentLevel);
    }
}
