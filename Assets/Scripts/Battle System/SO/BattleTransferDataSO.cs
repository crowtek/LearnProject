using UnityEngine;

// is used for scene transition on Monster Encounter
[CreateAssetMenu(fileName = "BattleTransferData", menuName = "Scriptable Objects/Battle/Transfer Data")]
public class BattleTransferDataSO : ScriptableObject
{
    [Header("Current Battle Data")]
    public BattleEntityData currentEnemyData;

    public void PrepareBattle(BattleEntityData enemy)
    {
        currentEnemyData = enemy;
    }

    public void Clear()
    {
        currentEnemyData = null;
    }
}