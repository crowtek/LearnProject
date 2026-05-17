using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    [SerializeField] private BattleUIController uiController;
    [SerializeField] private BattleEntityData playerTemplate;
    [SerializeField] private PlayerRuntimeState playerRuntime;
    [SerializeField] private BoolEventChannelSO battleStateEventChannel;
    [SerializeField] private BattleResultEventChannelSO battleResultEventChannel;

    private int currentPlayerHP;
    private int currentEnemyHP;
    private int accumulatedEXP;

    public enum BattleState { Idle, Start, PlayerTurn, EnemyTurn, Busy, Won, Lost, End }
    private BattleState currentState = BattleState.Idle;

    void Awake()
    {
        Instance = this;
        uiController.Initialize();

        uiController.Initialize();
        uiController.SkillDistro.SetupDynamicWeaponRows(playerRuntime);
    }

    public void StartBattle(BattleEntityData enemyData)
    {
        if (currentState != BattleState.Idle) return;

        currentPlayerHP = playerRuntime.currentHP;
        currentEnemyHP = enemyData.maxHP;
        accumulatedEXP = enemyData.expReward;
        currentState = BattleState.Start;

        uiController.SetActive(true);
        uiController.SetupBattleImages(playerTemplate.portrait, playerTemplate.battleImage, enemyData.battleImage, playerTemplate.entityName);

        uiController.BindButtons(PlayerAttack, () => EnemyAttacks(enemyData.attack), EndBattle);

        UpdateGameState();
        battleStateEventChannel.RaiseEvent(false);
    }

    private void UpdateGameState()
    {
        uiController.UpdateStats(currentPlayerHP, playerRuntime.maxHP, playerRuntime.currentLevel);
    }

    public void PlayerAttack()
    {
        currentEnemyHP -= playerRuntime.attack;
        uiController.ShowMonsterDamage(playerRuntime.attack);
        UpdateGameState();

        if (currentEnemyHP <= 0) EndBattle();
    }

    public void EnemyAttacks(int damage)
    {
        currentPlayerHP -= damage;
        uiController.ShowPlayerDamage(damage);

        if (currentPlayerHP <= 0)
        {
            currentPlayerHP = 0;
            playerRuntime.isDead = true;
        }

        UpdateGameState();
    }

    private void EndBattle()
    {
        if (currentEnemyHP <= 0)
        {
            string msg = $"You won! \nEarned {accumulatedEXP} EXP.";

            // Note: Check level projection status matching ProgressionManager's threshold logic
            if (playerRuntime.currentEXP + accumulatedEXP >= playerRuntime.expToNextLevel)
            {
                msg += "\n\nLEVEL UP!";
                // Show screen, but route continue pipeline through the skill point check sequence instead!
                uiController.ShowVictoryScreen(msg, EvaluatePostBattleSkillFlow);
            }
            else
            {
                uiController.ShowVictoryScreen(msg, CloseBattleUI);
            }
        }
        else
        {
            CloseBattleUI();
        }
    }

    private void EvaluatePostBattleSkillFlow()
    {
        if (playerRuntime.unspentSkillPoints > 0)
        {
            uiController.SkillDistro.OpenSkillDistribution(playerRuntime, () =>
            {
                uiController.SkillDistro.ApplyAllocatedPoints(playerRuntime);
                CloseBattleUI();
            });
        }
        else
        {
            CloseBattleUI();
        }
    }

    private void CloseBattleUI()
    {
        uiController.UnbindButtons(PlayerAttack, null, EndBattle);
        uiController.SetActive(false);

        playerRuntime.currentHP = currentPlayerHP;
        currentState = BattleState.Idle;
        battleStateEventChannel.RaiseEvent(true);

        battleResultEventChannel.RaiseEvent(new BattleResult
        {
            earnedEXP = (currentEnemyHP <= 0) ? accumulatedEXP : 0,
            isVictory = currentEnemyHP <= 0
        });
    }
}