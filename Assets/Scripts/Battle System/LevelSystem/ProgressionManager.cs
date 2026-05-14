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

        // 1. Capture the "Before" state for the UI
        int startLevel = playerState.currentLevel;
        int startHP = playerState.maxHP;
        int startAtk = playerState.attack;

        // 2. Perform the actual logic (Only once!)
        AddExperience(result.earnedEXP);

        // 3. Compare and Notify
        if (playerState.currentLevel > startLevel)
        {
            string changes = $"Level {startLevel} > {playerState.currentLevel}\n" +
                             $"HP: {startHP} > {playerState.maxHP}\n" +
                             $"ATK: {startAtk} > {playerState.attack}";

            Debug.Log($"Level Up Details: {changes}");
            // Trigger your UI event here
        }
    }

    private void AddExperience(int amount)
    {
        playerState.currentEXP += amount;

        while (playerState.currentEXP >= playerState.expToNextLevel)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        playerState.currentEXP -= playerState.expToNextLevel;
        playerState.currentLevel++;

        playerState.maxHP += hpGainPerLevel;
        playerState.attack += atkGainPerLevel;
        playerState.currentHP = playerState.maxHP;

        playerState.expToNextLevel = LevelCalculator.GetRequiredEXP(playerState.currentLevel);
    }
}