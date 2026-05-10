using UnityEngine;
using UnityEngine.UIElements;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    [SerializeField] private UIDocument battleHUD;
    [SerializeField] private BattleEntityData playerTemplate; 
    [SerializeField] private PlayerRuntimeState playerRuntime;
    [SerializeField] private BoolEventChannelSO battleStateEventChannel;

    private int currentPlayerHP;
    private int currentEnemyHP;
    private string currentEnemyName;

    private VisualElement root;
    private Button closeBtn;
    private Button attackBtn;
    private Label playerHPLabel;
    private Label enemyHPLabel;

    public enum BattleState { Idle, Start, PlayerTurn, EnemyTurn, Busy, Won, Lost, End }
    private BattleState currentState = BattleState.Idle;

    void Awake()
    {
        Instance = this;
        battleHUD.gameObject.SetActive(false);
    }

    public void StartBattle(BattleEntityData enemyData)
    {
        if (currentState != BattleState.Idle) return;

        currentPlayerHP = playerRuntime.currentHP;
        currentEnemyHP = enemyData.maxHP;
        currentEnemyName = enemyData.entityName;

        battleHUD.gameObject.SetActive(true);
        currentState = BattleState.Start;
        root = battleHUD.rootVisualElement;

        playerHPLabel = root.Q<Label>("PlayerHP");
        enemyHPLabel = root.Q<Label>("EnemyHP");

        root.Q<Label>("PlayerName").text = playerTemplate.entityName;
        root.Q<Label>("EnemyName").text = currentEnemyName;

        UpdateUI();

        attackBtn = root.Q<Button>("AttackButton");
        attackBtn.clicked += PlayerAttack;
        closeBtn = root.Q<Button>("CloseButton");
        closeBtn.clicked += EndBattle;

        battleStateEventChannel.RaiseEvent(false);
        Debug.Log($"Kampf gegen {currentEnemyName} beginnt!");
    }

    private void UpdateUI()
    {
        playerHPLabel.text = $"HP: {currentPlayerHP} / {playerTemplate.maxHP}";
        enemyHPLabel.text = $"HP: {currentEnemyHP}";
    }

    public void PlayerAttack()
    {
        currentEnemyHP -= playerTemplate.attack;
        UpdateUI();

        if (currentEnemyHP <= 0) EndBattle();
    }
    public void EnemyAttacks(int damage)
    {
        currentPlayerHP -= damage;

        if (currentPlayerHP <= 0)
        {
            currentPlayerHP = 0;
            playerRuntime.isDead = true;
            // Handle Game Over
        }
        UpdateUI();
    }


    private void EndBattle()
    {
        currentState = BattleState.Idle;
        playerRuntime.currentHP = currentPlayerHP;

        battleHUD.gameObject.SetActive(false);
        battleStateEventChannel.RaiseEvent(true);

        closeBtn.clicked -= EndBattle;
        attackBtn.clicked -= PlayerAttack;
    }
}