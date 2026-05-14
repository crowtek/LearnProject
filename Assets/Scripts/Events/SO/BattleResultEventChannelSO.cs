using UnityEngine;

public struct BattleResult
{
    public int earnedEXP;
    public int earnedGold;
    public bool isVictory;
    public bool leveledUp;
    public string statChanges;
}

[CreateAssetMenu(fileName = "BattleResultEventChannelSO", menuName = "Scriptable Objects/Battle/BattleResultEventChannelSO")]
public class BattleResultEventChannelSO : EventChannelSO<BattleResult> {}