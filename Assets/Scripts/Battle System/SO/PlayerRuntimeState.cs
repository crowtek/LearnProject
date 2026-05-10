using UnityEngine;

[CreateAssetMenu(fileName = "PlayerRuntimeState", menuName = "Scriptable Objects/PlayerRuntimeState")]
public class PlayerRuntimeState : ScriptableObject
{
    public int currentHP;
    public bool isDead;

    public void ResetStats(BattleEntityData template)
    {
        currentHP = template.maxHP;
        isDead = false;
    }
}
