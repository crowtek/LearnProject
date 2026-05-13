using UnityEngine;

public struct BattleResult
{
    public int earnedEXP;
    public int earnedGold;
    public bool isVictory;
}

[CreateAssetMenu(fileName = "BattleResultEventChannelSO", menuName = "Scriptable Objects/Battle/BattleResultEventChannelSO")]
public class BattleResultEventChannelSO : EventChannelSO<BattleResult> {}