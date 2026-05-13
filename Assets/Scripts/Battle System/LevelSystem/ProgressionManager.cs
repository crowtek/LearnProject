using UnityEngine;

public class ProgressionManager : MonoBehaviour
{
    [SerializeField] private PlayerRuntimeState playerState;
    [SerializeField] private BattleResultEventChannelSO battleResultChannel;

    [Header("Growth Settings")]
    [SerializeField] private int hpGainPerLevel = 10;
    [SerializeField] private int atkGainPerLevel = 2;

    private void OnEnable() => battleResultChannel.OnEventRaised += HandleBattleResult;
    private void OnDisable() => battleResultChannel.OnEventRaised -= HandleBattleResult;

    private void HandleBattleResult(BattleResult result)
    {
        if (!result.isVictory) return;

        AddExperience(result.earnedEXP);
    }

    private void AddExperience(int amount)
    {
        playerState.currentEXP += amount;

        while (playerState.currentEXP >= playerState.expToNextLevel)
        {
            LevelUp();
        }
        Debug.Log($"Exp gained: {amount} current Level: {playerState.currentLevel}");
    }

    private void LevelUp()
    {
        playerState.currentEXP -= playerState.expToNextLevel;
        playerState.currentLevel++;

        // Stat Progression
        playerState.maxHP += hpGainPerLevel;
        playerState.attack += atkGainPerLevel;

        // Full Heal (Dragon Quest Style)
        playerState.currentHP = playerState.maxHP;

        // Recalculate next requirement
        playerState.expToNextLevel = LevelCalculator.GetRequiredEXP(playerState.currentLevel);

        Debug.Log($"Level Up! Now Level {playerState.currentLevel}");
        // Here you would trigger the UI Level Up Overlay
    }
}