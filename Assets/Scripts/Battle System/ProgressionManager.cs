using UnityEngine;

public class ProgressionManager : MonoBehaviour
{
    [SerializeField] private PlayerRuntimeState playerState;
    [SerializeField] private BattleResultEventChannelSO battleResultChannel;

    [Header("Growth Settings")]
    [SerializeField] private int hpGainPerLevel = 10;
    [SerializeField] private int atkGainPerLevel = 2;
    [SerializeField] private int mpGainPerLevel = 5;
    [SerializeField] private int skillPointsPerLevel = 3;

    private void OnEnable() => battleResultChannel.OnEventRaised += HandleBattleResult;
    private void OnDisable() => battleResultChannel.OnEventRaised -= HandleBattleResult;

    private void HandleBattleResult(BattleResult result)
    {
        if (!result.isVictory) return;

        int startLevel = playerState.currentLevel;
        AddExperience(result.earnedEXP);

        if (playerState.currentLevel > startLevel)
        {
            Debug.Log($"[Progression] Level {startLevel} → {playerState.currentLevel}");
            // TODO: fire level-up UI event channel here
        }
    }

    private void AddExperience(int amount)
    {
        playerState.currentEXP += amount;
        while (playerState.currentEXP >= playerState.expToNextLevel)
            LevelUp();
    }

    private void LevelUp()
    {
        playerState.currentEXP -= playerState.expToNextLevel;
        playerState.currentLevel++;

        playerState.maxHP += hpGainPerLevel;
        playerState.maxMP += mpGainPerLevel;
        playerState.attack += atkGainPerLevel;

        // Restore to full on level up (classic Dragon Quest feel)
        playerState.currentHP = playerState.maxHP;
        playerState.currentMP = playerState.maxMP;

        playerState.unspentSkillPoints += skillPointsPerLevel;

        playerState.expToNextLevel = LevelCalculator.GetRequiredEXP(playerState.currentLevel);
    }
}